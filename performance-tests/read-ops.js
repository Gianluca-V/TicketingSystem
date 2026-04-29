import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomItem } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

export const options = {
    stages: [
        { duration: '30s', target: 20 }, // ramp-up to 20 users
        { duration: '1m', target: 20 },  // stay at 20 users
        { duration: '30s', target: 50 }, // ramp-up to 50 users
        { duration: '1m', target: 50 },  // stay at 50 users
        { duration: '30s', target: 0 },  // ramp-down to 0 users
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
        http_req_failed: ['rate<0.01'],    // less than 1% errors
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://192.168.1.57:8080/api/v1';

export default function () {
    // 1. Get all events
    let eventsRes = http.get(`${BASE_URL}/events`);
    check(eventsRes, {
        'get events status is 200': (r) => r.status === 200,
    });

    let events = eventsRes.json();
    if (events && events.length > 0) {
        // Pick a random event
        let event = randomItem(events);
        let eventId = event.id;

        // 2. Get sectors for the event
        let sectorsRes = http.get(`${BASE_URL}/events/${eventId}/sectors`);
        check(sectorsRes, {
            'get sectors status is 200': (r) => r.status === 200,
        });

        let sectors = sectorsRes.json();
        if (sectors && sectors.length > 0) {
            // Pick a random sector
            let sector = randomItem(sectors);
            let sectorId = sector.id;

            // 3. Get seats for the sector
            let seatsRes = http.get(`${BASE_URL}/events/${eventId}/sectors/${sectorId}/seats`);
            check(seatsRes, {
                'get seats status is 200': (r) => r.status === 200,
            });
        }
    }

    sleep(1); // Wait 1 second between iterations
}
