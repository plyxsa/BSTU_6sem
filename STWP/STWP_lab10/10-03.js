const WebSocket = require('ws');

const PORT = 4001;

const wss = new WebSocket.Server({ port: PORT }, () => {
    console.log(`Broadcast WebSocket server started on ws://localhost:${PORT}`);
});

wss.on('connection', (ws, req) => {
    const clientId = req.headers['sec-websocket-key'];
    console.log(`Broadcast Server: Client ${clientId} connected.`);

    ws.send(`Welcome, client ${clientId}! You are connected to the broadcast server.`);

    wss.clients.forEach(client => {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
            client.send(`Server: Client ${clientId} has joined.`);
        }
    });

    ws.on('message', (message) => {
        const messageString = message.toString();
        console.log(`Broadcast Server: Received from ${clientId}: ${messageString}`);

        console.log(`Broadcasting message from ${clientId} to ${wss.clients.size} clients.`);
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                 client.send(`Client ${clientId}: ${messageString}`);
            }
        });
    });

    ws.on('close', () => {
        console.log(`Broadcast Server: Client ${clientId} disconnected.`);
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(`Server: Client ${clientId} has left.`);
            }
        });
    });

    ws.on('error', (error) => {
        console.error(`Broadcast Server: Error for client ${clientId}:`, error);
    });
});

wss.on('error', (e) => { console.log('Broadcast WS server general error: ', e.message); });

console.log(`Waiting for connections on ws://localhost:${PORT}...`);