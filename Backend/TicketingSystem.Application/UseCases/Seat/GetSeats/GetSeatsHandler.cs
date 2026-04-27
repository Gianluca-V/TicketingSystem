using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.GetSeats;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Seat.Handlers
{
    public class GetSeatsHandler : IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>>
    {
        private readonly ISeatRepository _seatRepository;

        public GetSeatsHandler(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public async Task<IEnumerable<SeatDto>> Handle(GetSeatsQuery query, CancellationToken cancellationToken)
        {
            var seats = await _seatRepository.GetAllAsync(new SeatFilter
            {
                EventId = query.EventId,
                SectorId = query.SectorId,
                Page = query.Page,
                Take = query.Take
            }, cancellationToken);

            return seats.Select(s => new SeatDto(
               s.Id,
               s.SeatNumber,
               s.SectorId,
               "",
               s.Price,
               s.Status.ToString()
           ));
        }
    }
}
