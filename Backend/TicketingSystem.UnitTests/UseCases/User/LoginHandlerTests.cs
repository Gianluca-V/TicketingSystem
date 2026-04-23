using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.Login;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new LoginHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
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
        var user = new TicketingSystem.Domain.Entities.User 
        { 
            Id = 1, 
            Email = command.Email, 
            PasswordHash = "hashed_password" 
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.Verify(command.Password, user.PasswordHash))
            .Returns(true);

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
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.User?)null);

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
        var user = new TicketingSystem.Domain.Entities.User { Email = command.Email, PasswordHash = "hashed" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.Verify(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("Invalid credentials");
    }
}
