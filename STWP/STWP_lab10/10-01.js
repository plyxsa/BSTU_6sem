const http = require('http');
const fs = require('fs');
const WebSocket = require('ws');

const HTTP_PORT = 3000;
const WS_PORT = 4000;

// --- HTTP Server ---
const httpServer = http.createServer((req, res) => {
    if (req.method === 'GET' && req.url === '/start') {
        console.log(`HTTP Server: Serving /start`);
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        fs.readFile('10-01.html', (err, data) => {
            if (err) {
                console.error("Error reading HTML file:", err);
                res.writeHead(500);
                res.end('Server Error');
            } else {
                res.end(data);
            }
        });
    } else {
        console.log(`HTTP Server: Received ${req.method} ${req.url} - Responding 400`);
        res.writeHead(400, { 'Content-Type': 'text/plain' });
        res.end('400 Bad Request');
    }
});

httpServer.listen(HTTP_PORT, () => {
    console.log(`HTTP server started on http://localhost:${HTTP_PORT}`);
    console.log(`Access http://localhost:${HTTP_PORT}/start`);
});

httpServer.on('error', (e) => { console.log('HTTP server error: ', e.message); });

// --- WebSocket Server ---
const wss = new WebSocket.Server({ port: WS_PORT }, () => {
    console.log(`WebSocket server started on ws://localhost:${WS_PORT}`);
});

let serverK = 0;
let connections = new Map();

wss.on('connection', (ws, req) => {
    const clientId = req.headers['sec-websocket-key'];
    console.log(`WS Server: Client ${clientId} connected.`);

    connections.set(clientId, {
        lastClientN: null,
        serverInterval: null
    });

    // WS-сервер периодически каждые 5 сек. Отправляет клиенту сообщения
    const clientData = connections.get(clientId);
    clientData.serverInterval = setInterval(() => {
        serverK++;
        const messageToServer = `10-01-server: ${clientData.lastClientN === null ? '?' : clientData.lastClientN}->${serverK}`;
        if (ws.readyState === WebSocket.OPEN) {
             console.log(`WS Server: Sending to ${clientId}: ${messageToServer}`);
             ws.send(messageToServer);
        } else {
             console.log(`WS Server: Client ${clientId} not open, skipping send.`);
        }
    }, 5000);

    // WS-сервер принимает сообщения от клиента.
    ws.on('message', (message) => {
        const messageString = message.toString();
        console.log(`WS Server: Received from ${clientId}: ${messageString}`);

        // WS-сервер отображает принятые сообщения от клиента. (Already done above)
        try {
            const parts = messageString.split(':');
            if (parts.length >= 2 && parts[0].trim() === '10-01-client') {
                const n = parseInt(parts[1].trim());
                if (!isNaN(n)) {
                     connections.get(clientId).lastClientN = n;
                     console.log(`WS Server: Updated lastClientN for ${clientId} to ${n}`);
                }
            }
        } catch (e) {
            console.error(`WS Server: Error parsing client message "${messageString}":`, e);
        }
    });

    ws.on('close', () => {
        console.log(`WS Server: Client ${clientId} disconnected.`);
        const clientData = connections.get(clientId);
        if (clientData && clientData.serverInterval) {
            clearInterval(clientData.serverInterval);
            console.log(`WS Server: Cleared interval for ${clientId}`);
        }
        connections.delete(clientId);
    });

    ws.on('error', (error) => {
        console.error(`WS Server: Error for client ${clientId}:`, error);
        const clientData = connections.get(clientId);
        if (clientData && clientData.serverInterval) {
            clearInterval(clientData.serverInterval);
            console.log(`WS Server: Cleared interval for ${clientId} due to error.`);
        }
         connections.delete(clientId);
    });
});

wss.on('error', (e) => { console.log('WS server general error: ', e.message); });