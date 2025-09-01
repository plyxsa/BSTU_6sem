const http = require('http');
const fs = require('fs');
const url = require('url');
const open = require('open');

async function openTabs() {
    await open('http://localhost:3000/');
    await open('http://localhost:3000/');
    await open('http://localhost:3000/');
}

function factorial(n) {
    return n === 0 ? 1 : n * factorial(n - 1);
}

http.createServer((req, res) => {
    const parsedUrl = url.parse(req.url, true);

    if (parsedUrl.pathname === '/') {
        fs.readFile('./index.html', (err, html) => {
            if (err) {
                res.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' });
                res.end('Ошибка при чтении файла');
                return;
            }

            res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
            res.end(html);
        });
    } else if (parsedUrl.pathname === '/fact' && parsedUrl.query.k) {
        const k = parseInt(parsedUrl.query.k);
        if (isNaN(k) || k < 0) {
            res.writeHead(400, { 'Content-Type': 'application/json' });
            res.end(JSON.stringify({ error: 'Invalid k' }));
            return;
        }
        const fact = factorial(k);
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ k, fact }));
    } else {
        res.writeHead(404, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ error: 'Not found' }));
    }
}).listen(3000, () => {
    console.log('Сервер запущен по адресу http://localhost:3000/');
});

openTabs();