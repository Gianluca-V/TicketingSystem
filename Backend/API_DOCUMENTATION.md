# API Documentation - Ticketing System (RESTful Version)

## Base URL
`http://localhost:<port>/api/v1`

---

## 1. Authentication & Users

### Auth Tokens (Login)
*   **URL**: `/auth/tokens`
*   **Method**: `POST`
*   **Description**: Authenticates a user and returns a JWT token.
*   **Body**:
    ```json
    {
      "email": "user@example.com",
      "password": "yourpassword"
    }
    ```
*   **Response (200 OK)**:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1..."
    }
    ```

### Users
*   **URL**: `/users`
*   **Method**: `GET` (List), `POST` (Register)
*   **URL**: `/users/{id}`
*   **Method**: `GET`, `PUT`, `DELETE`
*   **Query Params (List)**: `Name`, `Email`, `Page`, `Take`

---

## 2. Events, Sectors & Seats

### Events
*   **URL**: `/events`
*   **Method**: `GET` (List), `POST` (Create)
*   **URL**: `/events/{id}`
*   **Method**: `GET`, `PUT`, `DELETE`

### Sectors (Hierarchy)
*   **URL**: `/events/{eventId}/sectors`
*   **Method**: `GET` (List), `POST` (Create)
*   **URL**: `/events/{eventId}/sectors/{sectorId}`
*   **Method**: `GET`, `PUT`, `DELETE`

### Seats (Hierarchy)
*   **URL**: `/events/{eventId}/sectors/{sectorId}/seats`
*   **Method**: `GET` (List)
*   **URL**: `/seats`
*   **Method**: `POST`
*   **URL**: `/seats/{id}`
*   **Method**: `GET`, `PUT`, `DELETE`

---

## 3. Reservations & Payments

### Reservations
*   **URL**: `/seats/{seatId}/reservations`
*   **Method**: `POST`
*   **Description**: Creates a temporary reservation for a specific seat.
*   **Body**:
    ```json
    {
      "seatId": 101,
      "userId": 1
    }
    ```
*   **Response (201 Created)**:
    ```json
    {
      "reservationId": "Guid",
      "expiresAt": "DateTime"
    }
    ```

### Reservation Details
*   **URL**: `/reservations/{reservationId}`
*   **Method**: `GET`
*   **Description**: Retrieves status and details of a reservation.
*   **Response (200 OK)**: `ReservationDto`

### Payments (Hierarchy)
*   **URL**: `/reservations/{reservationId}/payments`
*   **Method**: `POST`
*   **Description**: Processes a payment for a specific reservation.
*   **Body**:
    ```json
    {
      "reservationId": "Guid",
      "transactionId": "string"
    }
    ```
*   **Response (200 OK)**:
    ```json
    {
      "status": "Success"
    }
    ```

---

## 4. Auditing

### Audit Logs
*   **URL**: `/audit-logs`
*   **Method**: `GET`
*   **Query Params**: `UserId`, `Action`, `From`, `To`, `Page`, `Take`
*   **Response (200 OK)**: `Array<AuditLogDto>`

---

## Common Structures (DTOs)

### EventDto
```json
{ "id": 1, "name": "Festival", "date": "...", "venue": "Stadium", "sectorCount": 2 }
```

### SectorDto
```json
{ "id": 1, "eventId": 1, "eventName": "Festival", "name": "VIP", "price": 150.0, "capacity": 100 }
```

### SeatDto
```json
{ "id": 101, "seatNumber": "VIP-1", "sectorId": 1, "sectorName": "VIP", "price": 150.0, "status": "Available" }
```

### ReservationDto
```json
{ "id": "Guid", "seatId": 101, "userId": "1", "reservedAt": "...", "expiresAt": "...", "isExpired": false }
```

### AuditLogDto
```json
{ "id": "Guid", "userId": 1, "action": "Login", "resourceType": "User", "resourceId": "1", "details": "...", "occurredAt": "..." }
```
