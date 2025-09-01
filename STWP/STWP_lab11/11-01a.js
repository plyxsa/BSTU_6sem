const WebSocket = require('ws');
const fs = require('fs');
const path = require('path');

const SERVER_URL = 'ws://localhost:4000';
const FILE_TO_SEND = path.join(__dirname, 'MyFileToSend.txt');

if (!fs.existsSync(FILE_TO_SEND)) {
    console.error(`Error: File not found at ${FILE_TO_SEND}`);
    console.log('Please create a file named MyFileToSend.txt in the project directory.');
    process.exit(1);
}

const ws = new WebSocket(SERVER_URL);

ws.on('open', () => {
    console.log('Connected to server.');

    const readStream = fs.createReadStream(FILE_TO_SEND);
    const duplex = WebSocket.createWebSocketStream(ws, { encoding: 'binary' });

    console.log(`Sending file: ${path.basename(FILE_TO_SEND)}...`);

    readStream.pipe(duplex);

    readStream.on('end', () => {
        console.log('File sending finished from client side.');
    });

    readStream.on('error', (err) => {
        console.error('ReadStream error:', err);
        ws.close();
    });

    duplex.on('error', (err) => {
        console.error('WebSocket stream error:', err);
        ws.close();
    });
});

ws.on('message', (message) => {
    console.log(`Received from server: ${message}`);
});

ws.on('close', () => {
    console.log(`Disconnected from server.`);
});

ws.on('error', (err) => {
    console.error('WebSocket connection error:', err.message);
});