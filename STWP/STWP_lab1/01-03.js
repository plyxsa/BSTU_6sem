const http = require('http');

const formatHeaders = (request) => {
    let headersContent = '';
    for (let key in request.headers) {
        headersContent += `<h3>${key}: ${request.headers[key]}</h3>`;
    }
    return headersContent;
};

http.createServer((request, response) => {
    let body = '';
    request.on('data', (chunk) => {
        body += chunk;
        console.log('Data received:', body);
    });

    request.on('end', () => {
        response.writeHead(200, {'Content-Type': 'text/html; charset=utf-8'});
        response.end(`
            <!DOCTYPE html>
            <html>
                <head></head>
                <body>
                    <h1>Структура запроса</h1>
                    <h2>Метод: ${request.method}</h2>
                    <h2>URI: ${request.url}</h2>
                    <h2>Версия: ${request.httpVersion}</h2>
                    <h2>Заголовки:</h2>
                    ${formatHeaders(request)}
                    <h2>Тело: ${body}</h2>
                </body>
            </html>
        `);
    });
}).listen(3000, () => {
    console.log('Server running at http://localhost:3000/');
});