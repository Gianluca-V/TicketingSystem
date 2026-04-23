using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.GetUsers;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class GetUsersHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUsersHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnUserDtos()
    {
        // Arrange
        var users = new List<TicketingSystem.Domain.Entities.User>
        {
            new() { Id = 1, Name = "User 1", Email = "u1@e.com", PasswordHash = "h" },
            new() { Id = 2, Name = "User 2", Email = "u2@e.com", PasswordHash = "h" }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("User 1");
    }
}
