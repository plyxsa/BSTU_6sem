// 09-07.js: POST-запрос с большим файлом MyFile.png (стриминг multipart/form-data)
const http = require('http');
const fs = require('fs');
const path = require('path');

const boundary = '--------------------------' + Date.now().toString(16);
const filePath = path.join(__dirname, 'MyFile.png'); // Убедитесь, что файл существует и > 0.5 MB

// Проверяем существование файла перед отправкой
if (!fs.existsSync(filePath)) {
    console.error(`Ошибка: Файл не найден - ${filePath}`);
    process.exit(1);
}

const options = {
    hostname: 'localhost',
    port: 3000,
    path: '/upload', // Эндпоинт из ЛР08, задание 14
    method: 'POST',
    headers: {
        'Content-Type': `multipart/form-data; boundary=${boundary}`,
    }
};

console.log('--- Отправка запроса (09-07) ---');
console.log(`Метод: ${options.method}`);
console.log(`Хост: ${options.hostname}:${options.port}`);
console.log(`Путь: ${options.path}`);
console.log(`Файл (стриминг): ${filePath}`);

const req = http.request(options, (res) => {
    console.log('\n--- Получен ответ (09-07) ---');
    console.log(`Статус ответа: ${res.statusCode} (${res.statusMessage})`);

    let responseBody = '';
    res.setEncoding('utf8');
    res.on('data', chunk => responseBody += chunk);
    res.on('end', () => {
        console.log('\n--- Тело ответа ---');
        console.log(responseBody);
        console.log('--- Конец тела ответа ---');
    });
});

req.on('error', (e) => console.error(`Ошибка при запросе: ${e.message}`));

// Часть 1: текстовое поле
req.write(`--${boundary}\r\n`);
req.write(`Content-Disposition: form-data; name="comment"\r\n\r\n`);
req.write(`Это большой файл MyFile.png\r\n`);

// Часть 2: заголовки файла
req.write(`--${boundary}\r\n`);
req.write(`Content-Disposition: form-data; name="file_large"; filename="${path.basename(filePath)}"\r\n`);
req.write(`Content-Type: image/png\r\n\r\n`);

// Часть 3: стриминг файла
const fileStream = fs.createReadStream(filePath);

fileStream.on('data', (chunk) => {
    req.write(chunk);
    process.stdout.write('.');
});

fileStream.on('end', () => {
    console.log('\nФайл полностью отправлен.');
    // Часть 4: завершающий boundary
    req.end(`\r\n--${boundary}--\r\n`); // Завершаем запрос ПОСЛЕ отправки файла
});

fileStream.on('error', (err) => {
    console.error(`\nОшибка чтения файла: ${err.message}`);
    req.destroy(err);
});