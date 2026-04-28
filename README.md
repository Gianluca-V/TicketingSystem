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

### 🐳 Ejecución con Docker (Preferido)
La forma más sencilla de ejecutar toda la pila (API, Frontend y Base de datos) es usando Docker Compose:

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

---

## 🛠 Stack Tecnológico

### Backend (.NET 8)
- **Arquitectura:** Clean Architecture con CQRS (Segregación de Responsabilidades de Consulta y Comando).
- **Base de Datos:** PostgreSQL con Entity Framework Core.
- **Seguridad:** Autenticación JWT y ASP.NET Core Identity.
- **Resiliencia:** Trabajadores en segundo plano para limpieza de reservas expiradas.
- **Validación:** Manejo robusto de errores basado en códigos de estado (Conflict 409, NotFound 404).

### Frontend (Vue 3)
- **Framework:** Vue 3 con Composition API.
- **Gestión de Estado:** Pinia (Stores reactivos para Autenticación y Reservas).
- **Enrutamiento:** Vue Router.
- **Estilos:** CSS Vanilla moderno con variables para facilitar la personalización de temas.
- **Cliente API:** Axios con manejo centralizado de errores basado en estado.

---

## 📈 Arquitectura del Sistema
El sistema está construido sobre una arquitectura distribuida:
- **Capa de Cliente:** Aplicación de Página Única (SPA) que se comunica mediante API REST.
- **Capa de Aplicación:** Gestiona la lógica de negocio a través de Handlers especializados.
- **Capa de Persistencia:** Utiliza los patrones Unit of Work y Repository para garantizar la integridad de los datos y la seguridad transaccional.
- **Sistema de Auditoría:** Cada operación sensible genera una entrada en el registro de auditoría para seguridad y monitoreo.

---

## 📝 Notas de Desarrollo
Para ejecutar los servicios por separado durante el desarrollo:

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

Desarrollado como parte del curso de Proyecto de Software. 2026.