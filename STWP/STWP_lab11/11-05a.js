const rpcWSC = require('rpc-websockets').Client;
const ws = new rpcWSC('ws://localhost:4000');

async function runSequentialRPC() {
    console.log('--- Running RPC calls sequentially ---');
    try {
        // Public calls
        console.log('Calling square(3)...');
        let res = await ws.call('square', [3]);
        console.log('square(3) result:', res);

        console.log('Calling square(5, 4)...');
        res = await ws.call('square', [5, 4]);
        console.log('square(5, 4) result:', res);

        console.log('Calling sum(2)...');
        res = await ws.call('sum', [2]);
        console.log('sum(2) result:', res);

        console.log('Calling sum(2, 4, 6, 8, 10)...');
        res = await ws.call('sum', [2, 4, 6, 8, 10]);
        console.log('sum(2, 4, 6, 8, 10) result:', res);

        console.log('Calling mul(3)...');
        res = await ws.call('mul', [3]);
        console.log('mul(3) result:', res);

        console.log('Calling mul(3, 5, 7, 9, 11, 13)...');
        res = await ws.call('mul', [3, 5, 7, 9, 11, 13]);
        console.log('mul(3, 5, 7, 9, 11, 13) result:', res);

        // Protected calls
        console.log('Attempting login...');
        const loginSuccess = await ws.login({ user: 'admin', pass: 'password123' });
        if (!loginSuccess) {
            console.error('Login failed! Cannot call protected methods.');
        } else {
            console.log('Login successful.');

            console.log('Calling fib(1)...');
            res = await ws.call('fib', [1]);
            console.log('fib(1) result:', res);

            console.log('Calling fib(2)...');
            res = await ws.call('fib', [2]);
            console.log('fib(2) result:', res);

            console.log('Calling fib(7)...');
            res = await ws.call('fib', [7]);
            console.log('fib(7) result:', res);

            console.log('Calling fact(0)...');
            res = await ws.call('fact', [0]);
            console.log('fact(0) result:', res);

            console.log('Calling fact(5)...');
            res = await ws.call('fact', [5]);
            console.log('fact(5) result:', res);

            console.log('Calling fact(10)...');
            res = await ws.call('fact', [10]);
            console.log('fact(10) result:', res);
        }

    } catch (error) {
        console.error('RPC call failed:', error.message || error);
        if (error.code) console.error(`Error code: ${error.code}`);
    } finally {
        ws.close();
    }
}

ws.on('open', () => {
    console.log('RPC Client connected.');
    runSequentialRPC();
});

ws.on('error', (error) => console.error('RPC Client WebSocket error:', error.message));
ws.on('close', () => console.log('RPC Client disconnected.'));