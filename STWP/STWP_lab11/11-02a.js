const WebSocket = require('ws');
const fs = require('fs');
const path = require('path');

const SERVER_URL = 'ws://localhost:4000';
const SAVE_AS = path.join(__dirname, 'downloaded_copy.txt');

const ws = new WebSocket(SERVER_URL);
let writeStream;

ws.on('open', () => {
    console.log('Connected to server.');
    console.log(`Preparing to receive file and save as ${path.basename(SAVE_AS)}`);

    writeStream = fs.createWriteStream(SAVE_AS);
    const duplex = WebSocket.createWebSocketStream(ws, { encoding: 'binary' });

    duplex.pipe(writeStream);

    writeStream.on('finish', () => {
        console.log(`File ${path.basename(SAVE_AS)} received successfully.`);
        ws.close();
    });

    writeStream.on('error', (err) => {
        console.error('WriteStream error:', err);
        duplex.destroy(err);
        ws.close();
    });

    duplex.on('error', (err) => {
        console.error('WebSocket stream error:', err);
        if (writeStream && !writeStream.closed) {
            writeStream.end();
        }
        fs.unlink(SAVE_AS, (unlinkErr) => {
             if (unlinkErr && unlinkErr.code !== 'ENOENT') console.error(`Error deleting incomplete file ${path.basename(SAVE_AS)}:`, unlinkErr);
             else console.log(`Deleted incomplete file ${path.basename(SAVE_AS)} (or it wasn't created).`);
        });
        ws.close();
    });
});

ws.on('message', (message) => {
    if (!writeStream) {
      console.log(`Received text message from server: ${message}`);
    }
});


ws.on('close', (code, reason) => {
    console.log(`Disconnected from server. Code: ${code}, Reason: ${reason ? reason.toString() : 'No reason given'}`);
    if (writeStream && !writeStream.closed) {
        writeStream.end();
        console.log('WriteStream closed due to connection closure.');
    }
});

ws.on('error', (err) => {
    console.error('WebSocket connection error:', err.message);
    if (writeStream && !writeStream.closed) {
        writeStream.end();
    }
});