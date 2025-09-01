// WS-сервер предназначен для приема по ws-каналу файлов.
// Принятый по ws-каналу файл переписывается в директорий upload.

const WebSocket = require('ws');
const fs = require('fs');
const path = require('path');

const PORT = 4000;
const UPLOAD_DIR = path.join(__dirname, 'upload');

if (!fs.existsSync(UPLOAD_DIR)) {
    fs.mkdirSync(UPLOAD_DIR);
    console.log(`Created directory: ${UPLOAD_DIR}`);
}

const wss = new WebSocket.Server({ port: PORT });
let fileCounter = 0;

console.log(`WebSocket server started on port ${PORT}`);

wss.on('connection', (ws) => {
    console.log('Client connected');
    fileCounter++;
    const fileName = `uploaded_file_${fileCounter}.dat`;
    const filePath = path.join(UPLOAD_DIR, fileName);
    console.log(`Preparing to receive file: ${fileName}`);

    const duplex = WebSocket.createWebSocketStream(ws, { encoding: 'binary' });
    const writeStream = fs.createWriteStream(filePath);

    duplex.pipe(writeStream);

    writeStream.on('finish', () => {
        console.log(`File ${fileName} received successfully.`);
        ws.send(`Server received ${fileName}`); 
    });

    writeStream.on('error', (err) => {
        console.error('WriteStream error:', err);
        ws.send(`Server error receiving file: ${err.message}`);
        duplex.destroy(err);
    });

    duplex.on('error', (err) => {
        console.error('WebSocket stream error:', err);
        writeStream.end();
        fs.unlink(filePath, (unlinkErr) => {
             if (unlinkErr) console.error(`Error deleting incomplete file ${fileName}:`, unlinkErr);
             else console.log(`Deleted incomplete file ${fileName}`);
        });
    });

    ws.on('close', () => {
        console.log('Client disconnected');
        if (!writeStream.closed) {
             writeStream.end();
             console.log(`WriteStream for ${fileName} closed due to client disconnect.`);
        }
    });

    ws.on('error', (err) => {
        console.error('WebSocket connection error:', err);
        if (!writeStream.closed) {
            writeStream.end();
        }
    });
});

wss.on('error', (err) => {
    console.error('WebSocket server error:', err);
});