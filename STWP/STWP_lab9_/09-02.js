// 09-02.js: GET-запрос с числовыми параметрами x и y
const http = require('http');
const querystring = require('querystring');

const params = querystring.stringify({ x: 5, y: 10 });
const pathWithParams = `/parameter?${params}`;

const options = {
    hostname: 'localhost',
    port: 3000,
    path: pathWithParams,
    method: 'GET'
};

console.log('--- Отправка запроса (09-02) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-02) ---');
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

req.end();