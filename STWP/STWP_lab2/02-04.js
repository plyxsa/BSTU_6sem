const http = require('http')
const fs = require('fs')

http.createServer(function (request, response) {
    if (request.url === '/xmlhttprequest') {
        let html = fs.readFileSync('./STWP_lab2/xmlhttprequest.html')
        response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' })
        response.end(html)

    } else if (request.method === 'GET' && request.url === '/api/name') {
        response.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
        response.end('Лопатнюк Полина Вечеславовна'); 
        
    } else {
        response.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
        response.end('The page is not found');
    }

}).listen(5000, () => {
    console.log('Server running at http://localhost:5000/xmlhttprequest');
});