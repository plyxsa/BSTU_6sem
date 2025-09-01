const http = require('http');
const url = require('url');

function factorial(n) {
    return n === 0 ? 1 : n * factorial(n - 1);
}

const server = http.createServer((req, res) => {
    const parsedUrl = url.parse(req.url, true);
    
    if (parsedUrl.pathname === '/fact' && parsedUrl.query.k) {
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
});

server.listen(3000, () => {
    console.log('Server running at http://localhost:3000/');
});
