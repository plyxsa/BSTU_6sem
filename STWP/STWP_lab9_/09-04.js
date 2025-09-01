// 09-04.js: POST-запрос с данными в формате JSON
const http = require('http');

// Структура данных из Задания 10 Лабораторной работы 8 (пункт 22)
const jsonData = JSON.stringify({
    "_comment": "Запрос. Лабораторная работа 8/10 --> 9/04",
    "x": 11,
    "y": 22,
    "s": "Строка запроса",
    "m": ["a", "b"],
    "o": { "surname": "Петров", "name": "Петр" }
});

const options = {
    hostname: 'localhost',
    port: 3000,
    path: '/json',
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Content-Length': Buffer.byteLength(jsonData)
    }
};

console.log('--- Отправка запроса (09-04) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);
console.log('Отправляемые JSON данные:', jsonData);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-04) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`);

    let responseBody = '';
    res.setEncoding('utf8');

    res.on('data', (chunk) => {
        responseBody += chunk;
    });

    res.on('end', () => {
        console.log('\n--- Тело ответа ---');
        try {
            const parsedResponse = JSON.parse(responseBody);
            console.log(JSON.stringify(parsedResponse, null, 2));
        } catch (e) {
            console.log("Не удалось распарсить ответ как JSON:");
            console.log(responseBody);
        }
        console.log('--- Конец тела ответа ---');
    });
});

req.on('error', (e) => {
    console.error(`Ошибка при запросе: ${e.message}`);
});

req.write(jsonData);
req.end();