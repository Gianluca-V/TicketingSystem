import http from 'k6/http';
import { check } from 'k6';

/**
 * CONCURRENCY RESERVATION TEST
 * 
 * This script tests how the system handles multiple users trying to reserve 
 * the SAME seat simultaneously.
 * 
 * Expectations:
 * - Only 1 user should succeed (201 Created).
 * - All others should receive a 409 Conflict.
 */

export const options = {
    scenarios: {
        reservation_clash: {
            executor: 'per-vu-iterations',
            vus: 10, 
            iterations: 1,
            maxDuration: '30s',
        },
    },
    thresholds: {
        'http_req_duration': ['p(95)<2000'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://192.168.1.57:8080/api/v1';
const SEAT_ID = __ENV.SEAT_ID || 2; // Default to Seat 2 to avoid likely taken Seat 1

export default function () {
    const vuId = __VU;
    // Use timestamp to ensure unique users each run
    const uniqueId = `${Date.now()}_${vuId}`;
    const email = `testuser_${uniqueId}@example.com`;
    const password = 'Password123!';
    const name = `Test User ${vuId}`;

    const headers = { 'Content-Type': 'application/json' };

    // 1. Register User
    const regPayload = JSON.stringify({ name, email, password });
    let regRes = http.post(`${BASE_URL}/auth/register`, regPayload, { headers });
    
    if (regRes.status !== 200) {
        console.error(`Registration failed for ${email}: ${regRes.status} ${regRes.body}`);
        return;
    }
    const userId = regRes.json().id;

    // 2. Login
    const loginPayload = JSON.stringify({ email, password });
    let loginRes = http.post(`${BASE_URL}/auth/login`, loginPayload, { headers });
    
    if (loginRes.status !== 200) {
        console.error(`Login failed for ${email}: ${loginRes.status}`);
        return;
    }
    const token = loginRes.json().token;

    // 3. ATTEMPT RESERVATION
    const authHeaders = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };

    const reservePayload = JSON.stringify({
        seatId: parseInt(SEAT_ID),
        userId: userId
    });

    // All VUs will reach this point and fire almost simultaneously
    let reserveRes = http.post(`${BASE_URL}/seats/${SEAT_ID}/reservations`, reservePayload, { headers: authHeaders });

    check(reserveRes, {
        'is 201 or 409': (r) => r.status === 201 || r.status === 409,
    });

    if (reserveRes.status === 201) {
        console.log(`VU ${vuId} WON the seat ${SEAT_ID}!`);
    } else if (reserveRes.status === 409) {
        console.log(`VU ${vuId} lost (409 Conflict)`);
    } else {
        console.error(`VU ${vuId} failed with ${reserveRes.status}: ${reserveRes.body}`);
    }
}
