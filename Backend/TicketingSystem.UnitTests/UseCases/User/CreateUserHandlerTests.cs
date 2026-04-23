using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new CreateUserHandler(
            _userRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _passwordHasherMock.Object,
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

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.User?)null);

        _passwordHasherMock.Setup(h => h.Hash(command.Password))
            .Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<TicketingSystem.Domain.Entities.User>(u => 
            u.Name == command.Name && 
            u.Email == command.Email && 
            u.PasswordHash == "hashed_password"), It.IsAny<CancellationToken>()), Times.Once);
        
        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TicketingSystem.Domain.Entities.User { Email = command.Email });

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("User already exists");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryFails_ShouldRollbackAndRethrow()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "fail@example.com",
            Password = "Password123"
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.User?)null);

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
