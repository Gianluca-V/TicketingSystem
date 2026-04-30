using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Reservation.GetReservation
{
    public class GetReservationsHandler
    : IQueryHandler<GetReservationsQuery, IEnumerable<ReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ICacheService _cacheService;

        public GetReservationsHandler(IReservationRepository reservationRepository, ICacheService cacheService)
        {
            _reservationRepository = reservationRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ReservationDto>> Handle(
            GetReservationsQuery query,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"Reservations:List:{query.UserId}:{query.SeatId}:{query.Expired}:{query.Page}:{query.Take}";
            var cachedReservations = await _cacheService.GetAsync<IEnumerable<ReservationDto>>(cacheKey, cancellationToken);
            if (cachedReservations != null)
            {
                return cachedReservations;
            }

            var filter = new ReservationFilter
            {
                SeatId = query.SeatId,
                UserId = query.UserId,
                IsActive = query.Expired.HasValue
                    ? !query.Expired.Value
                    : null,
                Page = query.Page,
                Take = query.Take
            };

            var reservations = await _reservationRepository.GetAllAsync(
                filter,
                cancellationToken);

            var result = reservations.Select(r => new ReservationDto(
                r.Id,
                r.SeatId,
                r.UserId.ToString(),
                r.ReservedAt,
                r.ExpiresAt,
                r.IsExpired
            )).ToList();

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

            return result;
        }
    }
}
