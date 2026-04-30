using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Reservation.DeleteReservation;

public class DeleteReservationCommand : ICommand
{
    public Guid ReservationId { get; set; }

    public DeleteReservationCommand(Guid reservationId)
    {
        ReservationId = reservationId;
    }
}
