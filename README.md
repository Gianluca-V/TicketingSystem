# Sistema de Ticketing StageFront ◈

Un sistema integral de ticketing y gestión de eventos diseñado para reservas de asientos de alto rendimiento y control administrativo. Esta plataforma permite a los usuarios explorar eventos, reservar asientos con expiración en tiempo real y simular pagos, mientras proporciona a los administradores herramientas robustas para la orquestación de eventos y auditoría del sistema.

### 🌐 Versión Alojada
Experimenta la aplicación en vivo en: **[https://softtp1.vespelabs.com/](https://softtp1.vespelabs.com/)**

---

## ✨ Características Principales

### 🎫 Para Usuarios
- **Catálogo de Eventos en Vivo:** Explora eventos próximos con información de sede y estado.
- **Selección Interactiva de Asientos:** Cuadrícula visual de asientos con disponibilidad en tiempo real.
- **Sistema Inteligente de Reservas:** Asegura un asiento durante 10 minutos con una cuenta regresiva de alta precisión, segundo a segundo.
- **Simulación de Pago Seguro:** Flujo de pago integrado con liberación automática del asiento al expirar o mediante cancelación manual.
- **Perfiles de Usuario:** Gestiona información personal y visualiza el historial de reservas.

### 🛠 Para Administradores
- **Panel de Control Integral:** Vista general de la actividad del sistema y estado de los recursos.
- **Gestión de Eventos y Sectores:** Operaciones CRUD completas para eventos y sus respectivos sectores (precios, capacidad, etc.).
- **Gestión Optimizada de Asientos:** 
    - **Carga Masiva:** Crea cientos de asientos en una sola operación mediante el endpoint optimizado de carga masiva.
    - **Control de Estado:** Anula manualmente los estados de los asientos (Disponible, Reservado, Vendido).
- **Registros de Auditoría:** Trazabilidad completa de las acciones del sistema (Inicios de sesión, Creaciones, Actualizaciones, Conflictos, Expiraciones) con descripciones legibles para humanos.
- **Administración de Usuarios:** Gestiona cuentas, restablece contraseñas y monitorea la actividad.

---

## 🚀 Primeros Pasos

### Requisitos Previos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Recomendado)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Para ejecución manual del backend)
- [Node.js 18+](https://nodejs.org/) (Para ejecución manual del frontend)
- [Redis](https://redis.io/) (Incluido automáticamente en Docker Compose)

### 🐳 Ejecución con Docker (Preferido)
La forma más sencilla de ejecutar toda la pila (API, Frontend, Base de datos y Caché) es usando Docker Compose:

1. Clona el repositorio:
   ```bash
   git clone https://github.com/your-repo/ticketing-system.git
   cd ticketing-system
   ```
2. Inicia los servicios:
   ```bash
   docker-compose up --build
   ```
3. Accede a la aplicación:
   - **Frontend:** [http://localhost](http://localhost)
   - **API Swagger:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
   - **PostgreSQL:** `localhost:5433`
   - **Redis:** `localhost:6379` (para inspección o conexión externa)

> 💡 **Nota:** Los servicios incluyen *healthchecks* para garantizar que la API solo inicie cuando PostgreSQL y Redis estén listos.

---

## 🛠 Stack Tecnológico

### Backend (.NET 8)
- **Arquitectura:** Clean Architecture con CQRS (Segregación de Responsabilidades de Consulta y Comando).
- **Base de Datos:** PostgreSQL con Entity Framework Core.
- **Caché Distribuido:** Redis para almacenamiento temporal de reservas, tokens y datos de sesión de alto rendimiento.
- **Seguridad:** Autenticación JWT y ASP.NET Core Identity.
- **Resiliencia:** Trabajadores en segundo plano para limpieza de reservas expiradas y manejo de concurrencia.
- **Validación:** Manejo robusto de errores basado en códigos de estado (Conflict 409, NotFound 404).

### Frontend (Vue 3)
- **Framework:** Vue 3 con Composition API.
- **Gestión de Estado:** Pinia (Stores reactivos para Autenticación y Reservas).
- **Enrutamiento:** Vue Router.
- **Estilos:** CSS Vanilla moderno con variables para facilitar la personalización de temas.
- **Cliente API:** Axios con manejo centralizado de errores basado en estado.

### Infraestructura
| Servicio | Versión | Propósito | Puerto |
|----------|---------|-----------|--------|
| **PostgreSQL** | 16-alpine | Persistencia relacional de datos | 5433 → 5432 |
| **Redis** | 7-alpine | Caché distribuido y gestión de sesiones | 6379 |
| **API (.NET 8)** | 8.0 | Lógica de negocio y endpoints REST | 8080, 5221 |
| **Frontend (Nginx)** | Alpine | Servidor estático para SPA Vue 3 | 80 |

---

## 📈 Arquitectura del Sistema
El sistema está construido sobre una arquitectura distribuida en capas:

```
┌─────────────────┐
│   Cliente SPA   │ ◄── Vue 3 + Pinia (Frontend)
└────────┬────────┘
         │ HTTP/REST
         ▼
┌─────────────────┐
│   Capa API      │ ◄── .NET 8 + CQRS + JWT
│   (Backend)     │
└────┬────┬──────┘
     │    │
     │    │ Cache-Aside / Session Store
     │    ▼
     │ ┌─────────────┐
     │ │   Redis     │ ◄── Caché de reservas, locks distribuidos
     │ └─────────────┘
     │
     │ Persistencia
     ▼
┌─────────────────┐
│  PostgreSQL     │ ◄── Datos transaccionales y auditoría
└─────────────────┘
```

- **Capa de Cliente:** Aplicación de Página Única (SPA) que se comunica mediante API REST.
- **Capa de Aplicación:** Gestiona la lógica de negocio a través de Handlers especializados con patrón CQRS.
- **Capa de Caché (Redis):** Almacena temporalmente reservas activas, tokens de sesión y datos de disponibilidad para reducir latencia y mejorar la concurrencia.
- **Capa de Persistencia:** Utiliza los patrones Unit of Work y Repository para garantizar la integridad de los datos y la seguridad transaccional.
- **Sistema de Auditoría:** Cada operación sensible genera una entrada en el registro de auditoría para seguridad y monitoreo.

---

## 🔑 Variables de Entorno Clave

La API utiliza las siguientes variables para la configuración de servicios externos:

```env
# Conexión a PostgreSQL
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=ticketing_db;Username=ticketing_user;Password=SecurePass123!

# Conexión a Redis (Caché)
ConnectionStrings__Redis=redis:6379

# Configuración JWT
Jwt__Secret=A_Very_Long_And_Secure_Secret_Key_At_Least_32_Chars
Jwt__Issuer=TicketingSystem
Jwt__Audience=TicketingSystemUsers
```

> ⚠️ **Importante:** En producción, reemplace los valores sensibles mediante secretos gestionados (Docker Secrets, Azure Key Vault, etc.).

---

## 📝 Notas de Desarrollo

Para ejecutar los servicios por separado durante el desarrollo:

**Backend:**
```bash
cd Backend
# Asegúrese de tener Redis corriendo localmente o ajuste la cadena de conexión
dotnet run --project TicketingSystem.Api
```

**Frontend:**
```bash
cd Frontend
npm install
npm run dev
```

**Utilidades útiles:**
```bash
# Ver logs de todos los servicios
docker-compose logs -f

# Reiniciar solo la API
docker-compose restart api

# Acceder a la CLI de Redis para depuración
docker exec -it ticketing-redis redis-cli
```

---

## 🧪 Testing y Salud de Servicios

Cada servicio incluye *healthchecks* configurados en `docker-compose.yml`:

| Servicio | Endpoint/Comando de Healthcheck | Intervalo |
|----------|--------------------------------|-----------|
| PostgreSQL | `pg_isready -U ticketing_user` | 5s |
| Redis | `redis-cli ping` | 5s |
| API | `GET /health` | 30s |

Puede consultar el estado general accediendo a:  
🔗 [http://localhost:8080/health](http://localhost:8080/health)

---

*Desarrollado como parte del curso de Proyecto de Software. 2026.* 🎓