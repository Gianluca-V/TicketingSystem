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
        private readonly ICacheService _cacheService;

        public GetSeatByIdHandler(ISeatRepository seatRepository, ICacheService cacheService)
        {
            _seatRepository = seatRepository;
            _cacheService = cacheService;
        }

        public async Task<SeatDto?> Handle(GetSeatByIdQuery query, CancellationToken ct)
        {
            var cacheKey = $"Seat:{query.SeatId}";
            var cachedSeat = await _cacheService.GetAsync<SeatDto>(cacheKey, ct);
            if (cachedSeat != null)
            {
                return cachedSeat;
            }

            var seat = await _seatRepository.GetByIdAsync(query.SeatId, ct);

            if (seat is null || seat.SectorId != query.SectorId)
                return null;

            var result = new SeatDto(
                seat.Id,
                seat.SeatNumber,
                seat.SectorId,
                "",
                seat.Price,
                seat.Status.ToString()
            );

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1), ct);

            return result;
        }
    }
}
