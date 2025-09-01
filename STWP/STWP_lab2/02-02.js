const http = require('http');
const fs = require('fs');

http.createServer((request, response) => {
    if (request.url === '/png') {
        fs.readFile('./STWP_lab2/pic.png', (err, data) => {
            if (err) {
                response.writeHead(404);
                response.end('File not found');
            } else {
                response.writeHead(200, {'Content-Type': 'image/png'});
                response.end(data);
            }
        });
    } else {
        response.writeHead(404);
        response.end('Not Found');
    }
}).listen(5000, () => {
    console.log('Server running at http://localhost:5000/png');
});