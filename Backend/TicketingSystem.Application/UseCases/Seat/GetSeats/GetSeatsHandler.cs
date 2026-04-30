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
        private readonly ICacheService _cacheService;

        public GetSeatsHandler(ISeatRepository seatRepository, ICacheService cacheService)
        {
            _seatRepository = seatRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<SeatDto>> Handle(GetSeatsQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = $"Seats:List:{query.EventId}:{query.SectorId}:{query.Page}:{query.Take}";
            var cachedSeats = await _cacheService.GetAsync<IEnumerable<SeatDto>>(cacheKey, cancellationToken);
            if (cachedSeats != null)
            {
                return cachedSeats;
            }

            var seats = await _seatRepository.GetAllAsync(new SeatFilter
            {
                EventId = query.EventId,
                SectorId = query.SectorId,
                Page = query.Page,
                Take = query.Take
            }, cancellationToken);

            var result = seats.Select(s => new SeatDto(
               s.Id,
               s.SeatNumber,
               s.SectorId,
               "",
               s.Price,
               s.Status.ToString()
           )).ToList();

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1), cancellationToken);

            return result;
        }
    }
}
