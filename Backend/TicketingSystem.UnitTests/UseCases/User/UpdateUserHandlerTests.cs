using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.UpdateUser;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new UpdateUserHandler(
            _userRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateUser()
    {
        // Arrange
        var command = new UpdateUserCommand { Id = 1, Name = "New Name" };
        var user = new TicketingSystem.Domain.Entities.User { Id = 1, Name = "Old Name", Email = "old@example.com" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Name.Should().Be("New Name");
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateUserCommand { Id = 99 };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
