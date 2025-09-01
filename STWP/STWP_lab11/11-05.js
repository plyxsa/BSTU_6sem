//Сервер обеспечивает RPC-интерфейс

const rpcWSS = require('rpc-websockets').Server;

const PORT = 4000;
const server = new rpcWSS({ port: PORT, host: 'localhost' });

console.log(`RPC WebSocket server started on port ${PORT}`);

// square: площадь круга (1 параметр) или прямоугольника (2 параметра)
server.register('square', (params) => {
    if (!Array.isArray(params)) throw new Error('Parameters must be in an array.');
    const args = params;

    if (args.length === 1 && typeof args[0] === 'number') {
        const r = args[0];
        console.log(`RPC: square(radius=${r}) called`);
        return Math.PI * r * r;
    } else if (args.length === 2 && typeof args[0] === 'number' && typeof args[1] === 'number') {
        const a = args[0];
        const b = args[1];
        console.log(`RPC: square(a=${a}, b=${b}) called`);
        return a * b;
    } else {
        throw new Error('Invalid parameters for square. Use [r] for circle or [a, b] for rectangle.');
    }
}).public();

// sum: сумма переменного числа параметров
server.register('sum', (params) => {
    if (!Array.isArray(params)) throw new Error('Parameters must be in an array.');
    console.log(`RPC: sum(${params}) called`);
    if (params.length === 0) return 0;
    return params.reduce((acc, val) => {
        if (typeof val !== 'number') throw new Error('All parameters for sum must be numbers.');
        return acc + val;
    }, 0);
}).public();

// mul: произведение переменного числа параметров
server.register('mul', (params) => {
    if (!Array.isArray(params)) throw new Error('Parameters must be in an array.');
     console.log(`RPC: mul(${params}) called`);
    if (params.length === 0) return 1;
    return params.reduce((acc, val) => {
        if (typeof val !== 'number') throw new Error('All parameters for mul must be numbers.');
        return acc * val;
    }, 1);
}).public();


// установим фиктивную аутентификацию для демонстрации protected
server.setAuth((credentials) => {
    console.log('Authentication attempt:', credentials);
    return credentials && credentials.user === 'admin' && credentials.pass === 'password123';
});

// fib: n чисел Фибоначчи (защищенный метод)
server.register('fib', (params) => {
    if (!Array.isArray(params) || params.length !== 1 || typeof params[0] !== 'number' || params[0] < 0 || !Number.isInteger(params[0])) {
        throw new Error('Invalid parameter for fib. Use [n] where n is a non-negative integer.');
    }
    const n = params[0];
    console.log(`RPC (protected): fib(n=${n}) called`);

    if (n === 0) return [];
    if (n === 1) return [0];

    const sequence = [0, 1];
    while (sequence.length < n) {
        sequence.push(sequence[sequence.length - 1] + sequence[sequence.length - 2]);
    }
    return sequence.slice(0, n);
}).protected();

// fact: факториал числа n (защищенный метод)
server.register('fact', (params) => {
     if (!Array.isArray(params) || params.length !== 1 || typeof params[0] !== 'number' || params[0] < 0 || !Number.isInteger(params[0])) {
        throw new Error('Invalid parameter for fact. Use [n] where n is a non-negative integer.');
    }
    const n = params[0];
    console.log(`RPC (protected): fact(n=${n}) called`);

    if (n === 0 || n === 1) return 1;
    let result = 1;
    for (let i = 2; i <= n; i++) {
        result *= i;
    }
    return result;
}).protected();


server.on('listening', () => {
    console.log('RPC Server is listening');
});

server.on('error', (error) => {
    console.error('RPC Server error:', error);
});

server.on('connection', (socket) => {
    console.log('Client connected via RPC');
    socket.on('close', () => {
        console.log('Client disconnected from RPC');
    });
});