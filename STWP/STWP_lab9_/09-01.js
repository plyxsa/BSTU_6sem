// 09-01.js: GET-запрос, вывод статуса, IP/порта сервера, тела ответа
const http = require('http');

const options = {
    hostname: 'localhost',
    port: 3000,
    path: '/headers',
    method: 'GET'
};

console.log('--- Отправка запроса (09-01) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-01) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`); // Статус и сообщение
    console.log(`IP-адрес удаленного сервера: ${res.socket.remoteAddress}`); // IP сервера
    console.log(`Порт удаленного сервера: ${res.socket.remotePort}`);     // Порт сервера

    let responseBody = '';
    res.setEncoding('utf8');

    res.on('data', (chunk) => {
        responseBody += chunk;
    });

    res.on('end', () => {
        console.log('\n--- Тело ответа ---');
        console.log(responseBody);
        console.log('--- Конец тела ответа ---');
    });
});

req.on('error', (e) => {
    console.error(`Ошибка при запросе: ${e.message}`);
});

req.end();