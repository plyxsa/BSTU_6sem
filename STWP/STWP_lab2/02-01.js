const http = require('http');
const fs = require('fs');

http.createServer((request, response) => {
    fs.readFile('./STWP_lab2/index.html', (err, html) => {
        if (err) {
            response.writeHead(500, {'Content-Type': 'text/plain; charset=utf-8'});
            response.end('Ошибка при чтении файла');
            return;
        }

        response.writeHead(200, {'Content-Type': 'text/html; charset=utf-8'});
        response.end(html);
    });
}).listen(5000, () => {
    console.log('Сервер запущен по адресу http://localhost:5000/');
});
