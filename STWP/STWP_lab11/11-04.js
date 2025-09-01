// 14.Сервер  отправляет  в ответ клиенту сообщение вида: 
// {server: n  client:x, timestamp:t}, где n –номер сообщения, x-имя клиента, а t–штамп времени.

const WebSocket = require('ws');

const PORT = 4000;
const wss = new WebSocket.Server({ port: PORT });

let serverMessageCounter = 0;

console.log(`WebSocket server started on port ${PORT}`);

wss.on('connection', (ws) => {
    console.log('Client connected');

    ws.on('message', (message) => {
        try {
            const clientMessage = JSON.parse(message);
            console.log('Received JSON from client:', clientMessage);

            if (clientMessage && clientMessage.client && clientMessage.timestamp) {
                serverMessageCounter++;
                const serverResponse = {
                    server: serverMessageCounter,
                    client: clientMessage.client,
                    timestamp: clientMessage.timestamp // Используем timestamp клиента
                    // timestamp: new Date().toISOString() // Или можно использовать время сервера
                };
                console.log('Sending JSON to client:', serverResponse);
                ws.send(JSON.stringify(serverResponse));
            } else {
                console.log('Received invalid JSON structure.');
                ws.send(JSON.stringify({ error: 'Invalid message format. Expected {client: x, timestamp: t}' }));
            }
        } catch (e) {
            console.error('Failed to parse message as JSON or process:', e);
            ws.send(JSON.stringify({ error: 'Invalid JSON received' }));
        }
    });

    ws.on('close', () => {
        console.log('Client disconnected');
    });

    ws.on('error', (err) => {
        console.error('Client WebSocket error:', err);
    });
});

wss.on('error', (err) => {
    console.error('WebSocket server error:', err);
});