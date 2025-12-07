# Load Testing Script for SCADA Data Acquisition
# Target: Validate 100k + tags / sec capability
# Tool: k6(https://k6.io)

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
    stages: [
        { duration: '1m', target: 1000 },    // Ramp up to 1k virtual users
        { duration: '3m', target: 5000 },    // Ramp up to 5k virtual users
        { duration: '5m', target: 10000 },   // Ramp up to 10k virtual users
        { duration: '5m', target: 10000 },   // Stay at 10k for 5 minutes
        { duration: '2m', target: 0 },       // Ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],    // 95% of requests < 500ms
        http_req_failed: ['rate<0.01'],      // Error rate < 1%
        errors: ['rate<0.01'],
    },
};

const BASE_URL = __ENV.API_URL || 'http://localhost:5000';

// Simulate tag data
function generateTagData() {
    const sites = ['SITE01', 'SITE02', 'SITE03'];
    const devices = ['TURBINE01', 'TURBINE02', 'SOLAR01', 'BATTERY01'];
    const site = sites[Math.floor(Math.random() * sites.length)];
    const device = devices[Math.floor(Math.random() * devices.length)];

    return {
        tagName: `${site}.${device}.${Math.random().toString(36).substring(7)}`,
        value: Math.random() * 1000,
        timestamp: new Date().toISOString(),
        quality: 'Good',
        site: site,
        device: device
    };
}

export default function () {
    // Generate batch of 10 tag updates
    const batch = [];
    for (let i = 0; i < 10; i++) {
        batch.push(generateTagData());
    }

    // Post to RabbitMQ or DataAcquisition endpoint
    const payload = JSON.stringify(batch);
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(`${BASE_URL}/api/data/ingest`, payload, params);

    // Verify response
    const success = check(res, {
        'status is 200': (r) => r.status === 200,
        'response time OK': (r) => r.timings.duration < 500,
    });

    errorRate.add(!success);

    // Small delay to avoid overwhelming the system
    sleep(0.01);  // 10ms delay
}

// Custom summary
export function handleSummary(data) {
    const tagsPerSecond = (data.metrics.http_reqs.values.count / data.state.testRunDuration) * 10;  // 10 tags per request

    console.log(`\\n=== SCADA Load Test Results ===`);
    console.log(`Total Requests: ${data.metrics.http_reqs.values.count}`);
    console.log(`Tags Processed: ${data.metrics.http_reqs.values.count * 10}`);
    console.log(`Tags per Second: ${tagsPerSecond.toFixed(2)}`);
    console.log(`Avg Response Time: ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms`);
    console.log(`P95 Response Time: ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms`);
    console.log(`Error Rate: ${(data.metrics.errors.values.rate * 100).toFixed(2)}%`);
    console.log(`Target: 100,000 tags/sec - ${tagsPerSecond >= 100000 ? 'PASS ✓' : 'FAIL ✗'}`);

    return {
        'stdout': JSON.stringify(data, null, 2),
    };
}
