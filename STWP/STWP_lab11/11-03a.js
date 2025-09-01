const WebSocket = require('ws');

const SERVER_URL = 'ws://localhost:4000';
const ws = new WebSocket(SERVER_URL);

ws.on('open', () => {
    console.log('Connected to server.');
    // клиент сам ответит на ping сервера
});

ws.on('message', (message) => {
    console.log(`Received from server: ${message}`);
});

ws.on('ping', () => {
    console.log('Ping received from server.');
    // ws сам отправит pong
});

ws.on('close', (code, reason) => {
    console.log(`Disconnected from server. Code: ${code}, Reason: ${reason ? reason.toString() : 'No reason given'}`);
});

ws.on('error', (err) => {
    console.error('WebSocket connection error:', err.message);
});