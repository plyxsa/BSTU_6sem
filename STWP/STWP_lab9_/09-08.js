// 09-08.js: GET-запрос для получения ответа с вложенным файлом
const http = require('http');
const fs = require('fs');
const path = require('path');

const serverFileName = 'downloadable.txt'; // Имя файла на сервере (в папке static)
const localFileName = 'downloaded_file.txt'; // Имя, под которым сохраним локально
const localFilePath = path.join(__dirname, localFileName);

const options = {
    hostname: 'localhost',
    port: 3000,
    path: `/files/${serverFileName}`, // Эндпоинт из ЛР08, задание 13
    method: 'GET'
};

console.log('--- Отправка запроса (09-08) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);
console.log(`Сохраняем в: ${localFilePath}`);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-08) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`);

    if (res.statusCode === 200) {
        // Создаем поток для записи в локальный файл
        const fileStream = fs.createWriteStream(localFilePath);

        // Перенаправляем поток ответа (res) в поток файла (fileStream)
        res.pipe(fileStream);

        fileStream.on('finish', () => {
            console.log(`Файл успешно скачан и сохранен как ${localFileName}`);
        });

        fileStream.on('error', (err) => {
            console.error(`Ошибка записи файла ${localFileName}: ${err.message}`);
        });
    } else {
        let errorBody = '';
        res.setEncoding('utf8');
        res.on('data', chunk => errorBody += chunk);
        res.on('end', () => {
            console.error(`Ошибка на сервере (${res.statusCode}): ${errorBody}`);
        });
    }
});

req.on('error', (e) => {
    console.error(`Ошибка при запросе: ${e.message}`);
});

req.end();