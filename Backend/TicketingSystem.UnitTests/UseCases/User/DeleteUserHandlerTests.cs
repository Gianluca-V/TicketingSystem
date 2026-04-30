using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.DeleteUser;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;
using DomainUser = TicketingSystem.Domain.Entities.User;

namespace TicketingSystem.UnitTests.UseCases.User;

public class DeleteUserHandlerTests
{
    private readonly Mock<UserManager<DomainUser>> _userManagerMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<DomainUser>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = MockHelpers.MockUnitOfWork();
        _cacheServiceMock = MockHelpers.MockCacheService();

        _handler = new DeleteUserHandler(
            _userManagerMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteUser()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = 1 };
        var user = new DomainUser { Id = 1, Email = "test@example.com" };

        _userManagerMock.Setup(m => m.FindByIdAsync("1"))
            .ReturnsAsync(user);
        
        _userManagerMock.Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = 99 };
        _userManagerMock.Setup(m => m.FindByIdAsync("99"))
            .ReturnsAsync((DomainUser?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
