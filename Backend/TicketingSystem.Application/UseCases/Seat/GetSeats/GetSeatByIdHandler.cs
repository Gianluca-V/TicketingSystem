using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.GetSeats
{
    public class GetSeatByIdHandler : IQueryHandler<GetSeatByIdQuery, SeatDto?>
    {
        private readonly ISeatRepository _seatRepository;

        public GetSeatByIdHandler(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public async Task<SeatDto?> Handle(GetSeatByIdQuery query, CancellationToken ct)
        {
            var seat = await _seatRepository.GetByIdAsync(query.SeatId, ct);

            if (seat is null || seat.SectorId != query.SectorId)
                return null;

            return new SeatDto(
                seat.Id,
                seat.SeatNumber,
                seat.SectorId,
                "",
                seat.Price,
                seat.Status.ToString()
            );
        }
    }
}
