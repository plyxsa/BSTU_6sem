const http = require('http');
const readline = require('readline');

let state = 'norm';

const server = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end(`<h1>${state}</h1>`);
});

server.listen(5000, () => {
    console.log('Server running at http://localhost:5000/');
});

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.setPrompt(`${state}> `);
rl.prompt();

rl.on('line', (input) => {
    if (['norm', 'stop', 'test', 'idle'].includes(input)) {
        console.log(`${state}-->${input}`);
        state = input;
    } else if (input === 'exit') {
        console.log('Exiting...');
        server.close();
        rl.close();
        return;
    } else {
        console.log(`Invalid input: ${input}`);
    }

    rl.setPrompt(`${state}> `);
    rl.prompt();
});
