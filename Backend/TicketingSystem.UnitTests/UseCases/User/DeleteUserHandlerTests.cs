using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.DeleteUser;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new DeleteUserHandler(
            _userRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteUser()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = 1 };
        var user = new TicketingSystem.Domain.Entities.User { Id = 1 };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
