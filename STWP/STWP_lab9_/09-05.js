// 09-05.js: POST-запрос с данными в формате XML
const http = require('http');
const xmlbuilder = require('xmlbuilder');

// Структура данных из Задания 11 Лабораторной работы 8 (пункт 26)
const requestXml = xmlbuilder.create('request')
    .att('id', '99')
    .ele('x', { 'value': '3' }).up()
    .ele('x', { 'value': '4' }).up()
    .ele('m', { 'value': 'c' }).up()
    .ele('m', { 'value': 'd' }).up()
    .end({ pretty: false });

const options = {
    hostname: 'localhost',
    port: 3000,
    path: '/xml', // Эндпоинт из ЛР08, задание 11
    method: 'POST',
    headers: {
        'Content-Type': 'application/xml',
        'Accept': 'application/xml',
        'Content-Length': Buffer.byteLength(requestXml)
    }
};

console.log('--- Отправка запроса (09-05) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);
console.log('Отправляемые XML данные:', requestXml);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-05) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`);

    let responseBody = '';
    res.setEncoding('utf8');

    res.on('data', (chunk) => {
        responseBody += chunk;
    });

    res.on('end', () => {
        console.log('\n--- Тело ответа (XML) ---');
        console.log(responseBody);
        console.log('--- Конец тела ответа ---');
    });
});

req.on('error', (e) => {
    console.error(`Ошибка при запросе: ${e.message}`);
});

req.write(requestXml);
req.end();