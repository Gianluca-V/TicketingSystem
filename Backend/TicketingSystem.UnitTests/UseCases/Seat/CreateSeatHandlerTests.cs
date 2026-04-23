using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.CreateSeat;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class CreateSeatHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateSeatHandler _handler;

    public CreateSeatHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new CreateSeatHandler(
            _seatRepositoryMock.Object,
            _sectorRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_SectorExists_ShouldCreateSeat()
    {
        // Arrange
        var command = new CreateSeatCommand { SectorId = 1, SeatNumber = "A1", Price = 10.0m, RowIdentifier = "A" };
        var sector = new TicketingSystem.Domain.Entities.Sector 
        { 
            Id = 1, 
            Name = "S1", 
            Event = new TicketingSystem.Domain.Entities.Event { Name = "E1", Status = "A" } 
        };

        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        _seatRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.Seat>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
