const WebSocket = require('ws');

// получаем имя клиента из аргументов командной строки
// process.argv[0] = node, process.argv[1] = script path, process.argv[2] = first arg
const clientName = process.argv[2];

if (!clientName) {
    console.error('Error: Please provide a client name as a command line argument.');
    console.log('Usage: node 11-04a.js <YourClientName>');
    process.exit(1);
}

const SERVER_URL = 'ws://localhost:4000';
const ws = new WebSocket(SERVER_URL);

ws.on('open', () => {
    console.log(`Client "${clientName}" connected to server.`);

    const messageToSend = {
        client: clientName,
        timestamp: new Date().toISOString()
    };

    console.log('Sending to server:', messageToSend);
    ws.send(JSON.stringify(messageToSend));
});

ws.on('message', (message) => {
    try {
        const serverResponse = JSON.parse(message);
        console.log(`Received from server:`, serverResponse);
    } catch (e) {
        console.error('Failed to parse server message as JSON:', message.toString());
    }
});

ws.on('close', (code, reason) => {
    console.log(`Client "${clientName}" disconnected. Code: ${code}, Reason: ${reason ? reason.toString() : 'No reason given'}`);
});

ws.on('error', (err) => {
    console.error(`Client "${clientName}" WebSocket error:`, err.message);
});