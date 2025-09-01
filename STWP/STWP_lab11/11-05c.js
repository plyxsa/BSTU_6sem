const rpcWSC = require('rpc-websockets').Client;
const ws = new rpcWSC('ws://localhost:4000');

async function calculateExpression() {
    console.log('--- Calculating Expression ---');
    console.log('Expression: sum(square(3), square(5,4), mul(3,5,7,9,11,13)) + fib(7) * mul(2,4,6)');

    try {
        // Шаг 1: Вычисляем аргументы для первого sum параллельно
        console.log('Calculating arguments for sum...');
        const [sq3, sq54, mul_large] = await Promise.all([
            ws.call('square', [3]),
            ws.call('square', [5, 4]),
            ws.call('mul', [3, 5, 7, 9, 11, 13])
        ]);
        console.log(`square(3) = ${sq3}`);
        console.log(`square(5,4) = ${sq54}`);
        console.log(`mul(3,5,7,9,11,13) = ${mul_large}`);

        // Шаг 2: Вычисляем первый sum
        console.log('Calculating first sum...');
        const sum1_result = await ws.call('sum', [sq3, sq54, mul_large]);
        console.log(`sum(...) = ${sum1_result}`);

        // Шаг 3: Вычисляем fib(7) и mul(2,4,6) параллельно (fib требует логина)
        console.log('Calculating fib(7) and mul(2,4,6)...');
         console.log('Attempting login for fib(7)...');
        const loginSuccess = await ws.login({ user: 'admin', pass: 'password123' });
        if (!loginSuccess) {
            throw new Error('Login failed! Cannot call fib(7).');
        }
        console.log('Login successful.');

        const [fib7_array, mul_small] = await Promise.all([
            ws.call('fib', [7]),
            ws.call('mul', [2, 4, 6])
        ]);
        console.log(`fib(7) = [${fib7_array.join(', ')}]`);
        console.log(`mul(2,4,6) = ${mul_small}`);

        // Шаг 3.1: Извлекаем нужное значение из fib(7)
        if (!Array.isArray(fib7_array) || fib7_array.length < 7) {
             throw new Error(`fib(7) did not return a valid array of length 7: ${fib7_array}`);
        }
        const fib7_value = fib7_array[6]; 
        console.log(`Using value from fib(7): ${fib7_value}`);


        // Шаг 4: Вычисляем произведение fib(7) * mul(2,4,6)
        console.log('Calculating product term...');
        const product_term = fib7_value * mul_small;
        console.log(`Product term = ${product_term}`);

        // Шаг 5: Вычисляем финальную сумму
        console.log('Calculating final sum...');
        const finalResult = sum1_result + product_term;
        console.log('-------------------------------------');
        console.log(`Final Result = ${sum1_result} + ${product_term} = ${finalResult}`);
        console.log('-------------------------------------');

    } catch (error) {
        console.error('RPC calculation failed:', error.message || error);
         if (error.code) console.error(`Error code: ${error.code}`);
    } finally {
        ws.close();
    }
}


ws.on('open', () => {
    console.log('RPC Client connected.');
    calculateExpression();
});

ws.on('error', (error) => console.error('RPC Client WebSocket error:', error.message));
ws.on('close', () => console.log('RPC Client disconnected.'));