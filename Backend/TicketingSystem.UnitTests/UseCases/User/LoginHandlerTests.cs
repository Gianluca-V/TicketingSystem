using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.Login;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;
using TicketingSystem.UnitTests.Helpers;
using Xunit;
using DomainUser = TicketingSystem.Domain.Entities.User;

namespace TicketingSystem.UnitTests.UseCases.User;

public class LoginHandlerTests
{
    private readonly Mock<UserManager<DomainUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<DomainUser>();
        _jwtServiceMock = new Mock<IJwtService>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new LoginHandler(
            _userManagerMock.Object,
            _jwtServiceMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };
        var user = new DomainUser 
        { 
            Id = 1, 
            Email = command.Email,
            UserName = command.Email
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _jwtServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("fake_jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("fake_jwt_token");
        _auditRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => 
            l.Action == AuditAction.Login && 
            l.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new LoginCommand { Email = "wrong@example.com", Password = "password123" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Email))
            .ReturnsAsync((DomainUser?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "wrong_password" };
        var user = new DomainUser { Email = command.Email };

        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("Invalid credentials");
    }
}
