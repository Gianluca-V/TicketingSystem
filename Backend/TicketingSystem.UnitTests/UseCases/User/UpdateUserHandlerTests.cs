using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.UpdateUser;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;
using DomainUser = TicketingSystem.Domain.Entities.User;

namespace TicketingSystem.UnitTests.UseCases.User;

public class UpdateUserHandlerTests
{
    private readonly Mock<UserManager<DomainUser>> _userManagerMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<DomainUser>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new UpdateUserHandler(
            _userManagerMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateUser()
    {
        // Arrange
        var command = new UpdateUserCommand { Id = 1, Name = "New Name" };
        var user = new DomainUser { Id = 1, Name = "Old Name", Email = "old@example.com", UserName = "old@example.com" };

        _userManagerMock.Setup(m => m.FindByIdAsync("1"))
            .ReturnsAsync(user);
        
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Name.Should().Be("New Name");
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateUserCommand { Id = 99 };
        _userManagerMock.Setup(m => m.FindByIdAsync("99"))
            .ReturnsAsync((DomainUser?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
