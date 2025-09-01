const rpcWSS = require('rpc-websockets').Server;

const PORT = 4000;
const server = new rpcWSS({ port: PORT, host: 'localhost' });

console.log(`Pub/Sub WebSocket server started on port ${PORT}`);
console.log('Enter A, B, or C in the console to generate events...');

// Объявляем события, на которые могут подписываться клиенты
server.event('A');
server.event('B');
server.event('C');

server.on('listening', () => {
    console.log('Pub/Sub Server is listening');
});

server.on('error', (error) => {
    console.error('Pub/Sub Server error:', error);
});

server.on('connection', (socket) => {
    console.log('Client connected to Pub/Sub');
    socket.on('close', () => {
        console.log('Client disconnected from Pub/Sub');
    });
});

process.stdin.setEncoding('utf8');
process.stdin.on('readable', () => {
    let chunk;
    while ((chunk = process.stdin.read()) !== null) {
        const input = chunk.toString().trim().toUpperCase(); 

        if (input === 'A' || input === 'B' || input === 'C') {
            const eventName = input;
            const payload = {
                event: eventName,
                message: `Server generated event ${eventName}`,
                timestamp: new Date().toISOString()
            };
            console.log(`Emitting event '${eventName}' with payload:`, payload);
            server.emit(eventName, payload);
        } else {
            console.log(`Ignoring input: "${chunk.trim()}"`);
        }
    }
});

process.stdin.on('end', () => {
    console.log('Stdin stream ended.');
});

process.stdin.resume();