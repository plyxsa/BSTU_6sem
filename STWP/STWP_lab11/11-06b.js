const rpcWSC = require('rpc-websockets').Client;
const ws = new rpcWSC('ws://localhost:4000');

const EVENT_NAME = 'B';

ws.on('open', () => {
    console.log(`Client B connected. Subscribing to event '${EVENT_NAME}'...`);
    ws.subscribe(EVENT_NAME);
});

// Обработчик для конкретного события 'B'
ws.on(EVENT_NAME, (payload) => {
    console.log(`Received event '${EVENT_NAME}':`, payload);
});

ws.on('error', (error) => console.error(`Client B WebSocket error: ${error.message}`));
ws.on('close', (code) => console.log(`Client B disconnected. Code: ${code}`));