const WebSocket = require('ws');
const readline = require('readline');

const WS_URL = 'ws://localhost:4001';

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    prompt: 'Send> '
});

let clientName = `User_${Math.random().toString(36).substring(2, 7)}`;
console.log(`*** Starting client as: ${clientName} ***`);
console.log(`*** Type messages and press Enter to broadcast ***`);
console.log(`*** Press Ctrl+C to exit ***`);


function connect() {
    const ws = new WebSocket(WS_URL);

    ws.on('open', () => {
        console.log(`[${clientName}] Connected to broadcast server.`);
        ws.send(`${clientName} has joined the chat.`);
        rl.prompt();
    });

    ws.on('message', (data) => {
        readline.cursorTo(process.stdout, 0);
        readline.clearLine(process.stdout, 0);
        console.log(data.toString());
        rl.prompt(true);
    });

    ws.on('close', (code, reason) => {
        const reasonString = reason ? reason.toString() : 'N/A';
        console.log(`\n[${clientName}] Disconnected from server. Code: ${code}, Reason: ${reasonString}`);
        rl.close();
        process.exit(0);
    });

    ws.on('error', (error) => {
        console.error(`\n[${clientName}] WebSocket error:`, error.message);
        rl.close();
        process.exit(1);
    });

    rl.on('line', (line) => {
        const message = line.trim();
        if (message) {
            if (ws.readyState === WebSocket.OPEN) {
                ws.send(message);
            } else {
                console.log('Not connected. Cannot send message.');
            }
        }
        rl.prompt();
    });

    rl.on('close', () => {
         if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
             console.log('\nExiting...');
             ws.close(1000, `${clientName} is leaving.`);
         }
         process.exit(0);
    });

    return ws;
}

connect();