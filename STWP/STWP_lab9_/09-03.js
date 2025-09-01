// 09-03.js: POST-запрос с параметрами x, y, s (form-urlencoded)
const http = require('http');
const querystring = require('querystring');

const postData = querystring.stringify({
    x: 3,
    y: 4,
    s: 'Привет, сервер!'
});

const options = {
    hostname: 'localhost',
    port: 3000,
    path: '/formparameter',
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Content-Length': Buffer.byteLength(postData)
    }
};

console.log('--- Отправка запроса (09-03) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);
console.log('Отправляемые данные:', postData);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-03) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`);

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

req.write(postData);
req.end();