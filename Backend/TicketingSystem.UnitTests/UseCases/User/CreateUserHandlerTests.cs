using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;
using DomainUser = TicketingSystem.Domain.Entities.User;

namespace TicketingSystem.UnitTests.UseCases.User;

public class CreateUserHandlerTests
{
    private readonly Mock<UserManager<DomainUser>> _userManagerMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<DomainUser>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new CreateUserHandler(
            _userManagerMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUserAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123"
        };

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<DomainUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<DomainUser, string>((u, p) => u.Id = 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        _userManagerMock.Verify(m => m.CreateAsync(It.Is<DomainUser>(u => 
            u.Name == command.Name && 
            u.Email == command.Email), command.Password), Times.Once);
        
        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        _auditRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => 
            l.Action == AuditAction.Created && 
            l.ResourceType == "User"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowExceptionAndRollback()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "duplicate@example.com",
            Password = "Password123"
        };

        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<DomainUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User already exists" }));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*User already exists*");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<DomainUser>(), It.IsAny<string>()), Times.Once);
    }
}
