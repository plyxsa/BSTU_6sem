const http = require('http');

http.createServer((request, response) => {
    if (request.url === '/api/name' && request.method === 'GET') {
        response.writeHead(200, {'Content-Type': 'text/plain; charset=utf-8'});

        const fullName = 'Лопатнюк Полина Вечеславовна';

        response.end(fullName);
    } else {
        response.writeHead(404, {'Content-Type': 'text/plain; charset=utf-8'});
        response.end('Not Found');
    }
}).listen(5000, () => {
    console.log('Server running at http://localhost:5000/api/name');
});