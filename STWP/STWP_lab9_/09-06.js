// 09-06.js: POST-запрос с вложенным файлом MyFile.txt (multipart/form-data)
const http = require('http');
const fs = require('fs');
const path = require('path');

const boundary = '--------------------------' + Date.now().toString(16);
const filePath = path.join(__dirname, 'MyFile.txt'); // Убедитесь, что файл существует

try {
    let body = '';
    // Часть 1: текстовое поле (если нужно)
    body += `--${boundary}\r\n`;
    body += `Content-Disposition: form-data; name="comment"\r\n\r\n`;
    body += `Это файл MyFile.txt\r\n`;

    // Часть 2: файл
    body += `--${boundary}\r\n`;
    body += `Content-Disposition: form-data; name="file_upload"; filename="${path.basename(filePath)}"\r\n`;
    body += `Content-Type: text/plain\r\n\r\n`;

    const fileContent = fs.readFileSync(filePath);

    const bodyBuffer = Buffer.concat([
         Buffer.from(body, 'utf-8'), // Предыдущая часть тела
         fileContent,                // Содержимое файла
         Buffer.from(`\r\n--${boundary}--\r\n`, 'utf-8')
    ]);


    const options = {
        hostname: 'localhost',
        port: 3000,
        path: '/upload', // Эндпоинт из ЛР08, задание 14
        method: 'POST',
        headers: {
            'Content-Type': `multipart/form-data; boundary=${boundary}`,
            'Content-Length': bodyBuffer.length
        }
    };

    console.log('--- Отправка запроса (09-06) ---');
    console.log(`Метод: ${options.method}`);
    console.log(`Хост: ${options.hostname}:${options.port}`);
    console.log(`Путь: ${options.path}`);
    console.log(`Файл: ${filePath}`);

    const req = http.request(options, (res) => {
        console.log('\n--- Получен ответ (09-06) ---');
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

    // Отправляем собранное тело
    req.write(bodyBuffer);
    req.end();

} catch (err) {
    console.error(`Ошибка чтения файла ${filePath} или сборки запроса: ${err.message}`);
}