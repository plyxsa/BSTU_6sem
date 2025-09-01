const rpcWSC = require('rpc-websockets').Client;
const ws = new rpcWSC('ws://localhost:4000');

const EVENT_NAME = 'A';

ws.on('open', () => {
    console.log(`Client A connected. Subscribing to event '${EVENT_NAME}'...`);
    ws.subscribe(EVENT_NAME);
});

// Обработчик для конкретного события 'A'
ws.on(EVENT_NAME, (payload) => {
    console.log(`Received event '${EVENT_NAME}':`, payload);
});

ws.on('error', (error) => console.error(`Client A WebSocket error: ${error.message}`));
ws.on('close', (code) => console.log(`Client A disconnected. Code: ${code}`));