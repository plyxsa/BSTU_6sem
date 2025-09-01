// Приложение может принимать три типа уведомлений: A, B, C. 
// При получении уведомления, сервер выводит соответствующее сообщение на консоль.
const rpcWSS = require('rpc-websockets').Server;

const PORT = 4000;
const server = new rpcWSS({ port: PORT, host: 'localhost' });

console.log(`Notification WebSocket server started on port ${PORT}`);

// Регистрируем методы, которые будут вызываться через notify клиента
// Params будут содержать данные, переданные клиентом в notify (если есть)
server.register('A', (params) => {
    console.log(`Notification 'A' received.`);
    if (params && Object.keys(params).length > 0) {
         console.log('Payload:', params);
    }
}).public(); // Делаем публичными, т.к. нет аутентификации

server.register('B', (params) => {
    console.log(`Notification 'B' received.`);
     if (params && Object.keys(params).length > 0) {
         console.log('Payload:', params);
    }
}).public();

server.register('C', (params) => {
    console.log(`Notification 'C' received.`);
     if (params && Object.keys(params).length > 0) {
         console.log('Payload:', params);
    }
}).public();

server.on('listening', () => {
    console.log('Notification Server is listening');
});

server.on('error', (error) => {
    console.error('Notification Server error:', error);
});

server.on('connection', (socket) => {
    console.log('Client connected for notifications');
    socket.on('close', () => {
        console.log('Client disconnected from notifications');
    });
});