// WS-сервер предназначен для отправки по ws-каналу файлов из директория download.

const WebSocket = require('ws');
const fs = require('fs');
const path = require('path');

const PORT = 4000;
const DOWNLOAD_DIR = path.join(__dirname, 'download');
const FILE_TO_SEND = path.join(DOWNLOAD_DIR, 'MyFileToDownload.txt');

if (!fs.existsSync(DOWNLOAD_DIR)) {
    fs.mkdirSync(DOWNLOAD_DIR);
    console.log(`Created directory: ${DOWNLOAD_DIR}`);
}
if (!fs.existsSync(FILE_TO_SEND)) {
    fs.writeFileSync(FILE_TO_SEND, 'This is the file content to be downloaded.');
    console.log(`Created sample file: ${FILE_TO_SEND}`);
}


const wss = new WebSocket.Server({ port: PORT });

console.log(`WebSocket server started on port ${PORT}`);

wss.on('connection', (ws) => {
    console.log('Client connected, preparing to send file...');

    if (!fs.existsSync(FILE_TO_SEND)) {
        console.error(`Error: File not found at ${FILE_TO_SEND}`);
        ws.send(`Server error: File ${path.basename(FILE_TO_SEND)} not found.`);
        ws.close(1011, `File not found on server`);
        return;
    }

    const readStream = fs.createReadStream(FILE_TO_SEND);
    const duplex = WebSocket.createWebSocketStream(ws, { encoding: 'binary' });

    console.log(`Sending file: ${path.basename(FILE_TO_SEND)}...`);

    readStream.pipe(duplex);

    readStream.on('end', () => {
        console.log('File sending finished from server side.');
    });

     readStream.on('error', (err) => {
        console.error('ReadStream error:', err);
        try { ws.send(`Server error reading file: ${err.message}`); } catch (sendErr) {}
        duplex.destroy(err);
    });

    duplex.on('error', (err) => {
        console.error('WebSocket stream error:', err);
        readStream.destroy(err);
    });

    ws.on('close', () => {
        console.log('Client disconnected');
        readStream.destroy();
    });

    ws.on('error', (err) => {
        console.error('WebSocket connection error:', err);
        readStream.destroy();
    });
});

wss.on('error', (err) => {
    console.error('WebSocket server error:', err);
});