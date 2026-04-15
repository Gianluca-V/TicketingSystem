# Ticketing System - Sistema de Venta de Entradas

Sistema robusto de venta de entradas para eventos, diseñado para resolver colapsos por sobreventa bajo alta concurrencia.

## 🚀 Inicio Rápido

### Docker Compose (Recomendado)

```bash
cd ticketing-system
docker compose up -d --build
```

El sistema estará disponible en:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

### Credenciales de Base de Datos

| Parámetro | Valor |
|-----------|-------|
| Usuario | `ticketing_user` |
| Contraseña | `SecurePass123!` |
| Base de datos | `ticketing_db` |
| Puerto | `5432` |

## 📋 Endpoints API

### Eventos y Butacas

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/events` | Listar todos los eventos |
| `GET` | `/api/v1/events/{eventId}/sectors/{sectorId}/seats` | Obtener mapa de butacas por sector |

### Reservas

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/v1/events/{eventId}/sectors/{sectorId}/reservations` | Reservar una butaca |

Body:
```json
{
  "userId": "user123",
  "seatId": 1
}
```

### Pagos

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/v1/reservations/{reservationId}/pay` | Pagar una reserva |

Body:
```json
{
  "transactionId": "txn_abc123"
}
```

## 🏗️ Arquitectura

### Clean Architecture (4 capas)

```
src/
├── Domain/                 # Entidades, Interfaces, Excepciones
├── Application/            # DTOs, Commands, Queries
├── Infrastructure/         # Data, Repositories, Services, Handlers
└── Api/                    # Controllers, Middleware, Program.cs
```

### Stack Tecnológico

- **Backend**: C# .NET 8, ASP.NET Core Web API
- **ORM/Base de Datos**: Entity Framework Core 8, PostgreSQL 16
- **Patrón**: Clean Architecture + CQRS
- **Concurrencia**: Optimistic Locking con RowVersion (xmin PostgreSQL)
- **Documentación**: OpenAPI/Swagger UI

## 📐 Características

### Funcionales

- ✅ **Gestión de Eventos/Sectores**: Creación de eventos, sectores y precarga automática (1 evento, 2 sectores, 100 butacas)
- ✅ **Mapa de Asientos**: Consulta del estado actual de butacas por sector
- ✅ **Reserva Temporal**: Bloqueo de butaca por 5 minutos
- ✅ **Control de Concurrencia**: Optimistic Locking con retorno 409 Conflict
- ✅ **Pago Transaccional**: Operación ACID con auditoría completa
- ✅ **Liberación Automática**: Worker libera reservas expiradas cada 30s
- ✅ **Auditoría Inmutable**: Registro de todas las acciones con timestamp UTC

### No Funcionales

- ✅ Optimistic Locking con `xmin` de PostgreSQL
- ✅ Transacciones ACID explícitas para pagos
- ✅ CQRS estricto (Lecturas → DTOs, Escrituras → Repos + UoW)
- ✅ RESTful standards con URLs jerárquicas
- ✅ Swagger UI autogenerado
- ✅ Code-First con migraciones automáticas

## 🗄️ Modelo de Dominio

### Entidades

- **Event**: Evento con fecha y nombre
- **Sector**: Sector del evento (VIP, General, etc.)
- **Seat**: Butaca numerada con precio y estado
- **Reservation**: Reserva temporal con expiración
- **AuditLog**: Registro inmutable de auditoría

### Estados de Butaca

- `Available` (0) - Disponible
- `Reserved` (1) - Reservada temporalmente
- `Sold` (2) - Vendida

## 🧪 Guía de Pruebas de Concurrencia

### Prueba con 50 solicitudes simultáneas

```bash
# Reservar la butaca 1 con 50 usuarios simultáneos
for i in {1..50}; do
  curl -X POST http://localhost:5000/api/v1/events/1/sectors/1/reservations \
    -H "Content-Type: application/json" \
    -d "{\"userId\":\"user$i\",\"seatId\":1}" &
done

wait
```

**Resultado esperado**: 
- 1x `201 Created` (reserva exitosa)
- 49x `409 Conflict` (conflictos de concurrencia)

### Verificación de consistencia

```bash
# Ver estado de la butaca
curl http://localhost:5000/api/v1/events/1/sectors/1/seats

# Ver logs de auditoría (consultar DB directamente)
docker compose exec postgres psql -U ticketing_user -d ticketing_db -c "SELECT * FROM \"AuditLogs\" ORDER BY \"OccurredAt\" DESC LIMIT 10;"
```

## 📦 Estructura del Proyecto

```
ticketing-system/
├── TicketingSystem.Domain/
│   ├── Entities/          # Event, Sector, Seat, Reservation, AuditLog
│   ├── Interfaces/        # IUnitOfWork, ISeatRepository, etc.
│   └── Exceptions/        # ConcurrencyException, BusinessException
├── TicketingSystem.Application/
│   ├── DTOs/              # SeatDto, EventDto, ReserveSeatRequest, etc.
│   ├── Commands/          # ReserveSeatCommand, PaymentCommand
│   └── Queries/           # GetEventsQuery, GetSeatsQuery
├── TicketingSystem.Infrastructure/
│   ├── Data/              # ApplicationDbContext, DbSeeder
│   ├── Repositories/      # SeatRepository, ReservationRepository, etc.
│   ├── Services/          # ExpiredReservationWorker
│   └── Handlers/          # MediatR handlers para Commands/Queries
├── TicketingSystem.Api/
│   ├── Controllers/       # EventsController, ReservationsController, etc.
│   ├── Middleware/        # GlobalExceptionMiddleware
│   └── Program.cs
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
└── TicketingSystem.sln
```

## 🔧 Desarrollo Local

### Requisitos

- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL 16 (opcional, si se ejecuta fuera de Docker)

### Ejecución sin Docker

1. Iniciar PostgreSQL:
```bash
docker compose up -d postgres
```

2. Ejecutar la API:
```bash
cd TicketingSystem.Api
dotnet run
```

### Crear migraciones

```bash
dotnet ef migrations add InitialCreate -p TicketingSystem.Infrastructure -s TicketingSystem.Api
```

## 📊 Datos Iniciales (Seed)

Al iniciar el sistema se precarga automáticamente:

- **Evento**: "Rock Festival 2026" (15 de agosto de 2026)
- **Sector VIP**: 50 butacas (VIP-001 a VIP-050) - $150.00
- **Sector General**: 50 butacas (GEN-001 a GEN-050) - $75.00

## ⏱️ Worker de Liberación

El `ExpiredReservationWorker`:
- Ejecuta cada 30 segundos
- Detecta reservas expiradas (sin pago después de 5 minutos)
- Libera la butaca (cambia a `Available`)
- Registra auditoría como `SYSTEM/Released`
- Elimina la reserva expirada

## 🔐 Swagger UI

Acceda a http://localhost:5000/swagger para ver la documentación interactiva de la API con todos los endpoints, request/response schemas y la posibilidad de probar las operaciones directamente.
