using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketingSystem.Application.Services;
using TicketingSystem.Domain.Interfaces;
using TicketingSystem.Infrastructure.Data;
using TicketingSystem.Infrastructure.Repositories;
using TicketingSystem.Infrastructure.Services;

namespace TicketingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // Register CQRS Query Services
        services.AddScoped<IEventQueryService, EventQueryService>();
        services.AddScoped<ISeatQueryService, SeatQueryService>();

        // Register CQRS Command Services
        services.AddScoped<IReservationCommandService, ReservationCommandService>();
        services.AddScoped<IPaymentCommandService, PaymentCommandService>();

        // Register Background Worker
        services.AddHostedService<ExpiredReservationWorker>();

        return services;
    }
}
