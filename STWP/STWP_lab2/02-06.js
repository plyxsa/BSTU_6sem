const http = require('http');
const fs = require('fs');

http.createServer((request, response) => {
    if (request.url === '/jquery' && request.method === 'GET') {
        fs.readFile('./STWP_lab2/jquery.html', (err, data) => {
            if (err) {
                response.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' });
                response.end('Ошибка сервера');
            } else {
                response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
                response.end(data);
            }
        });

    } else if (request.url === '/api/name' && request.method === 'GET') {
        response.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
        response.end('Лопатнюк Полина Вечеславовна');
        
    } else {
        response.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
        response.end('Not Found');
    }
}).listen(5000, () => {
    console.log('Server running at http://localhost:5000/jquery');
});
