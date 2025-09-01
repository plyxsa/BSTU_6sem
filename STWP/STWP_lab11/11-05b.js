const rpcWSC = require('rpc-websockets').Client;
const async = require('async');

const ws = new rpcWSC('ws://localhost:4000');

function runParallelRPC() {
    console.log('--- Running RPC calls in parallel ---');

    ws.login({ user: 'admin', pass: 'password123' })
        .then(loginSuccess => {
            if (!loginSuccess) {
                console.error('Login failed! Protected methods will fail.');
                 throw new Error('Login Required for Protected Calls');
            }
            console.log('Login successful.');
            return makeParallelCalls();
        })
        .catch(error => {
            console.error('Error during login or parallel calls:', error.message || error);
            ws.close();
        });
}

function makeParallelCalls() {
    async.parallel({
        sq1: (cb) => ws.call('square', [3]).then(r => cb(null, r)).catch(e => cb(e)),
        sq2: (cb) => ws.call('square', [5, 4]).then(r => cb(null, r)).catch(e => cb(e)),
        sum1: (cb) => ws.call('sum', [2]).then(r => cb(null, r)).catch(e => cb(e)),
        sum2: (cb) => ws.call('sum', [2, 4, 6, 8, 10]).then(r => cb(null, r)).catch(e => cb(e)),
        mul1: (cb) => ws.call('mul', [3]).then(r => cb(null, r)).catch(e => cb(e)),
        mul2: (cb) => ws.call('mul', [3, 5, 7, 9, 11, 13]).then(r => cb(null, r)).catch(e => cb(e)),
        // Protected calls
        fib1: (cb) => ws.call('fib', [1]).then(r => cb(null, r)).catch(e => cb(e)),
        fib2: (cb) => ws.call('fib', [2]).then(r => cb(null, r)).catch(e => cb(e)),
        fib7: (cb) => ws.call('fib', [7]).then(r => cb(null, r)).catch(e => cb(e)),
        fact0: (cb) => ws.call('fact', [0]).then(r => cb(null, r)).catch(e => cb(e)),
        fact5: (cb) => ws.call('fact', [5]).then(r => cb(null, r)).catch(e => cb(e)),
        fact10: (cb) => ws.call('fact', [10]).then(r => cb(null, r)).catch(e => cb(e)),
    }, (err, results) => {
        if (err) {
            console.error('One or more parallel RPC calls failed:', err.message || err);
             if (err.code) console.error(`Error code: ${err.code}`);
        } else {
            console.log('All parallel RPC calls completed:');
            console.log('Results:', results);
        }
        ws.close();
    });
}


ws.on('open', () => {
    console.log('RPC Client connected.');
    runParallelRPC();
});

ws.on('error', (error) => console.error('RPC Client WebSocket error:', error.message));
ws.on('close', () => console.log('RPC Client disconnected.'));