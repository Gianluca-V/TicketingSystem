# StageFront Ticketing System ◈

A comprehensive ticketing and event management system designed for high-performance seat reservations and administrative control. This platform allows users to browse events, reserve seats with real-time expiration, and simulate payments, while providing administrators with robust tools for event orchestration and system auditing.

### 🌐 Hosted Version
Experience the live application at: **[https://softtp1.vespelabs.com/](https://softtp1.vespelabs.com/)**

---

## ✨ Key Features

### 🎫 For Users
- **Live Event Catalog:** Browse upcoming events with venue and status information.
- **Interactive Seat Selection:** Visual seat grid with real-time availability.
- **Smart Reservation System:** Secure a seat for 10 minutes with a high-precision, second-by-second countdown.
- **Secure Payment Simulation:** Integrated payment flow with automatic seat release upon expiration or manual cancellation.
- **User Profiles:** Manage personal information and view reservation history.

### 🛠 For Administrators
- **Comprehensive Dashboard:** Overview of system activity and resource status.
- **Event & Sector Management:** Full CRUD operations for events and their respective sectors (prices, capacity, etc.).
- **Optimized Seat Management:** 
    - **Bulk Upload:** Create hundreds of seats in a single operation via the optimized bulk endpoint.
    - **Status Control:** Manually override seat statuses (Available, Reserved, Sold).
- **Audit Logs:** Complete traceability of system actions (Logins, Creations, Updates, Conflicts, Expirations) with human-readable descriptions.
- **User Administration:** Manage accounts, reset passwords, and monitor activity.

---

## 🚀 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Recommended)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (For manual backend execution)
- [Node.js 18+](https://nodejs.org/) (For manual frontend execution)

### 🐳 Running with Docker (Preferred)
The easiest way to run the entire stack (API, Frontend, and Database) is using Docker Compose:

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/ticketing-system.git
   cd ticketing-system
   ```
2. Launch the services:
   ```bash
   docker-compose up --build
   ```
3. Access the application:
   - **Frontend:** [http://localhost](http://localhost)
   - **API Swagger:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
   - **PostgreSQL:** `localhost:5433`

---

## 🛠 Technical Stack

### Backend (.NET 8)
- **Architecture:** Clean Architecture with CQRS (Command/Query Responsibility Segregation).
- **Database:** PostgreSQL with Entity Framework Core.
- **Security:** JWT Authentication and ASP.NET Core Identity.
- **Resilience:** Background workers for expired reservation cleanup.
- **Validation:** Strong status-code based error handling (Conflict 409, NotFound 404).

### Frontend (Vue 3)
- **Framework:** Vue 3 with Composition API.
- **State Management:** Pinia (Reactive stores for Auth and Reservations).
- **Routing:** Vue Router.
- **Styling:** Modern Vanilla CSS with variables for easy theming.
- **API Client:** Axios with centralized status-based error handling.

---

## 📈 System Architecture
The system is built on a distributed architecture:
- **Client Layer:** Single Page Application (SPA) communicating via REST API.
- **Application Layer:** Handles business logic through specialized Handlers.
- **Persistence Layer:** Uses the Unit of Work and Repository patterns to ensure data integrity and transaction safety.
- **Audit System:** Every sensitive operation triggers an audit log entry for security and monitoring.

---

## 📝 Development Notes
To run services separately during development:

**Backend:**
```bash
cd Backend
dotnet run --project TicketingSystem.Api
```

**Frontend:**
```bash
cd Frontend
npm install
npm run dev
```

Developed as part of the Software Project course. 2026.
