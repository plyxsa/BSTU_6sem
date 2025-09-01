// 11-07a.js - WebSocket Notification Client
const rpcWSC = require('rpc-websockets').Client;
const ws = new rpcWSC('ws://localhost:4000');

ws.on('open', () => {
    console.log('Notification Client connected.');
    console.log('Enter A, B, or C to send a notification to the server...');

    // Настройка чтения из стандартного ввода (консоли клиента)
    process.stdin.setEncoding('utf8');
    process.stdin.on('readable', () => {
        let chunk;
        while ((chunk = process.stdin.read()) !== null) {
            const input = chunk.toString().trim().toUpperCase();

            if (input === 'A' || input === 'B' || input === 'C') {
                const notificationType = input;
                const payload = { // Необязательные данные
                    sender: 'Client-11-07a',
                    time: new Date().toLocaleTimeString()
                };
                console.log(`Sending notification '${notificationType}' with payload:`, payload);
                // Отправляем уведомление (fire-and-forget)
                ws.notify(notificationType, payload);
            } else {
                // console.log(`Ignoring input: "${chunk.trim()}"`);
            }
        }
    });

    process.stdin.on('end', () => {
        console.log('Stdin stream ended. Disconnecting.');
        ws.close();
    });

    // Предотвращаем завершение программы сразу
    process.stdin.resume();
});

ws.on('error', (error) => console.error('Notification Client WebSocket error:', error.message));
ws.on('close', (code) => console.log(`Notification Client disconnected. Code: ${code}`));