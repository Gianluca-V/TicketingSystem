using Microsoft.Extensions.DependencyInjection;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.CreateEvent;
using TicketingSystem.Application.UseCases.Event.DeleteEvent;
using TicketingSystem.Application.UseCases.Event.GetEvents;
using TicketingSystem.Application.UseCases.Event.UpdateEvent;
using TicketingSystem.Application.UseCases.Sector.CreateSector;
using TicketingSystem.Application.UseCases.Sector.DeleteSector;
using TicketingSystem.Application.UseCases.Sector.GetSectors;
using TicketingSystem.Application.UseCases.Sector.UpdateSector;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Application.UseCases.User.DeleteUser;
using TicketingSystem.Application.UseCases.User.GetUsers;
using TicketingSystem.Application.UseCases.User.UpdateUser;
using TicketingSystem.Application.UseCases.Seat.CreateSeat;
using TicketingSystem.Application.UseCases.Seat.UpdateSeat;
using TicketingSystem.Application.UseCases.Seat.DeleteSeat;
using TicketingSystem.Application.UseCases.Seat.GetSeats;
using TicketingSystem.Application.UseCases.Seat.Handlers;

namespace TicketingSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Event Handlers
        services.AddScoped<ICommandHandler<CreateEventCommand, int>, CreateEventHandler>();
        services.AddScoped<ICommandHandler<UpdateEventCommand>, UpdateEventHandler>();
        services.AddScoped<ICommandHandler<DeleteEventCommand>, DeleteEventHandler>();
        services.AddScoped<IQueryHandler<GetEventsQuery, IEnumerable<EventDto>>, GetEventsHandler>();

        // Sector Handlers
        services.AddScoped<ICommandHandler<CreateSectorCommand, int>, CreateSectorHandler>();
        services.AddScoped<ICommandHandler<UpdateSectorCommand>, UpdateSectorHandler>();
        services.AddScoped<ICommandHandler<DeleteSectorCommand>, DeleteSectorHandler>();
        services.AddScoped<IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>>, GetSectorsHandler>();

        // User Handlers
        services.AddScoped<ICommandHandler<CreateUserCommand, int>, CreateUserHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand>, UpdateUserHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>, GetUsersHandler>();

        // Seat Handlers
        services.AddScoped<ICommandHandler<CreateSeatCommand, int>, CreateSeatHandler>();
        services.AddScoped<ICommandHandler<UpdateSeatCommand>, UpdateSeatHandler>();
        services.AddScoped<ICommandHandler<DeleteSeatCommand>, DeleteSeatHandler>();
        services.AddScoped<IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>>, GetSeatsQueryHandler>();

        return services;
    }
}
