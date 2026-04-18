using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.UseCases.Seat.Queries;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.Handlers
{
    public class GetSeatsQueryHandler
    {
        private readonly ISeatRepository _seatRepository;

        public GetSeatsQueryHandler(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public async Task<IEnumerable<SeatDto>> HandleAsync( CancellationToken cancellationToken = default)
        {
            IEnumerable<Domain.Entities.Seat> seats = await _seatRepository.GetAll(cancellationToken);

            return seats
                .Select(s => new SeatDto(
                    s.Id,
                    s.SeatNumber,
                    s.Sector.Name,
                    s.Price,
                    s.Status.ToString()))
                .ToList();

        }

    }
}
