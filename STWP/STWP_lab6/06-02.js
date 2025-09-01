const http = require('http');
const fs = require('fs');
const url = require('url');
const querystring = require('querystring');
const m0603 = require('./m0603');

const port = 3000;

const server = http.createServer((req, res) => {
  const parsedUrl = url.parse(req.url);
  const pathname = parsedUrl.pathname;

  if (pathname === '/') {
    fs.readFile('index.html', (err, data) => {
      if (err) {
        res.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' });
        res.end('Ошибка загрузки файла');
        return;
      }
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      res.end(data);
    });
  } else if (pathname === '/send-email' && req.method === 'POST') {
    let body = '';
    req.on('data', chunk => {
      body += chunk.toString();
    });
    req.on('end', async () => {
      const formData = querystring.parse(body);
      const senderEmail = formData.senderEmail;
      const recipientEmail = formData.recipientEmail;
      const message = formData.message;

      try {
        await m0603.send(`Отправитель: ${senderEmail}<br>Получатель: ${recipientEmail}<br>Сообщение: ${message}`);
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end('<h1>Письмо успешно отправлено!</h1>');
      } catch (error) {
        console.error('Ошибка при отправке письма:', error);
        res.writeHead(500, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end('<h1>Ошибка при отправке письма.</h1>');
      }
    });
  } else {
    res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end('Страница не найдена');
  }
});

server.listen(port, () => {
  console.log(`Сервер запущен на порту ${port}`);
});