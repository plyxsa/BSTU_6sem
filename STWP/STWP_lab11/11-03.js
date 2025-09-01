// С помощью ping/pong-механизма сервер проверяет работоспособность соединений, каждые 5 секунд
const WebSocket = require('ws');

const PORT = 4000;
const wss = new WebSocket.Server({ port: PORT });

let messageCounter = 0;
const BROADCAST_INTERVAL = 15000;
const PING_INTERVAL = 5000;

console.log(`WebSocket server started on port ${PORT}`);

// Broadcast
setInterval(() => {
    messageCounter++;
    const message = `11-03-server: ${messageCounter}`;
    let activeClients = 0;
    wss.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(message);
            activeClients++;
        }
    });
    console.log(`Broadcasted "${message}" to ${activeClients} clients.`);
}, BROADCAST_INTERVAL);

// Ping/Pong
const pingInterval = setInterval(() => {
    let activeConnections = 0;
    wss.clients.forEach((client) => {
        // Если клиент не ответил на предыдущий ping (isAlive == false), разрываем соединение
        if (client.isAlive === false) {
            console.log('Client unresponsive, terminating connection.');
            return client.terminate();
        }

        // Считаем, что клиент жив, но ставим флаг в false. Ждем pong
        if (client.readyState === WebSocket.OPEN) {
           client.isAlive = false;
           client.ping(() => { });
           activeConnections++;
        }
    });
    console.log(`Ping check: ${activeConnections} active connections found.`);

}, PING_INTERVAL);

wss.on('connection', (ws) => {
    console.log('Client connected');
    ws.isAlive = true;

    ws.on('pong', () => {
        ws.isAlive = true;
    });

    ws.on('message', (message) => {
        console.log(`Received message from client: ${message}`);
    });

    ws.on('close', () => {
        console.log('Client disconnected');
    });

    ws.on('error', (err) => {
        console.error('Client WebSocket error:', err);
    });
});

wss.on('close', () => {
    console.log('WebSocket server shutting down.');
    clearInterval(pingInterval);
});

wss.on('error', (err) => {
    console.error('WebSocket server error:', err);
    clearInterval(pingInterval);
});