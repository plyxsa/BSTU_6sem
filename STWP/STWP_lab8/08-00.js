const http = require('http');
const url = require('url');
const fs = require('fs').promises;
const fsSync = require('fs'); // Для синхронного чтения HTML и проверки существования папки
const path = require('path');
const querystring = require('querystring');
const { parseString } = require('xml2js');
const xmlbuilder = require('xmlbuilder');
const multiparty = require('multiparty');

const PORT = 3000;
const STATIC_DIR = path.join(__dirname, 'static');
const UPLOAD_DIR = path.join(__dirname, 'upload_files');

if (!fsSync.existsSync(UPLOAD_DIR)) {
    fsSync.mkdirSync(UPLOAD_DIR);
    console.log(`Upload directory created: ${UPLOAD_DIR}`);
}

const sendHtml = (res, filePath) => {
    try {
        const htmlContent = fsSync.readFileSync(filePath, 'utf-8');
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end(htmlContent);
    } catch (err) {
        console.error(`Error reading HTML file ${filePath}:`, err);
        sendError(res, 500, `Internal Server Error: Cannot read file ${path.basename(filePath)}`);
    }
};

const sendError = (res, statusCode, message) => {
    console.error(`Sending Error: ${statusCode} - ${message}`);
    res.writeHead(statusCode, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end(message);
};

const requestHandler = async (req, res) => {
    const parsedUrl = url.parse(req.url, true);
    const pathname = parsedUrl.pathname;
    const query = parsedUrl.query;
    const method = req.method;

    console.log(`Received: ${method} ${req.url}`);

    try {
        if (method === 'GET') {
            switch (pathname) {
                case '/':
                    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                    res.end('Сервер 08-00 запущен');
                    break;
                // Задание 01: /connection?set=set
                case '/connection':
                    if (query.set) {
                        const newTimeout = parseInt(query.set);
                        if (!isNaN(newTimeout) && newTimeout >= 0) {
                            server.keepAliveTimeout = newTimeout;
                            res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                            res.end(`Установлено новое значение KeepAliveTimeout = ${server.keepAliveTimeout}`);
                            console.log(`Set KeepAliveTimeout = ${server.keepAliveTimeout}`);
                        } else {
                            sendError(res, 400, 'Некорректное значение для параметра set (должно быть неотрицательное число)');
                        }
                    } else {
                        res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                        res.end(`Текущее значение KeepAliveTimeout = ${server.keepAliveTimeout}`);
                    }
                    break;

                // Задание 02: /headers
                case '/headers':
                    res.setHeader('X-My-Header', 'Custom header value from PSKP');
                    const requestHeaders = req.headers;
                    const responseHeaders = res.getHeaders();
                    let headersResponse = '<h2>Заголовки запроса:</h2><pre>';
                    for (const key in requestHeaders) {
                        headersResponse += `${key}: ${requestHeaders[key]}\n`;
                    }
                    headersResponse += '</pre><h2>Заголовки ответа:</h2><pre>';
                    for (const key in responseHeaders) {
                        headersResponse += `${key}: ${responseHeaders[key]}\n`;
                    }
                    headersResponse += '</pre>';
                    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
                    res.end(headersResponse);
                    break;

                // Задание 03: /parameter?x=x&&y=y
                case '/parameter':
                    const xParam = parseFloat(query.x);
                    const yParam = parseFloat(query.y);
                    if (!isNaN(xParam) && !isNaN(yParam)) {
                        const sum = xParam + yParam;
                        const diff = xParam - yParam;
                        const prod = xParam * yParam;
                        const quot = yParam !== 0 ? xParam / yParam : 'Деление на ноль!';
                        res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                        res.end(`x = ${xParam}, y = ${yParam}\nСумма: ${sum}\nРазность: ${diff}\nПроизведение: ${prod}\nЧастное: ${quot}`);
                    } else {
                        sendError(res, 400, 'Параметры x и y должны быть числами.');
                    }
                    break;

                // Задание 05: /close
                case '/close':
                    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                    res.end('Сервер будет остановлен через 10 секунд.');
                    console.log('Server shutdown initiated...');
                    setTimeout(() => {
                        server.close((err) => {
                            if (err) {
                                console.error('Error closing server:', err);
                                process.exit(1); // Выход с ошибкой
                            } else {
                                console.log('Server closed successfully.');
                                process.exit(0); // Успешный выход
                            }
                        });
                    }, 10000);
                    break;

                // Задание 06: /socket
                case '/socket':
                    const clientIp = req.socket.remoteAddress;
                    const clientPort = req.socket.remotePort;
                    const serverIp = req.socket.localAddress;
                    const serverPort = req.socket.localPort;
                    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                    res.end(`IP-адрес клиента: ${clientIp}\nПорт клиента: ${clientPort}\nIP-адрес сервера: ${serverIp}\nПорт сервера: ${serverPort}`);
                    break;

                // Задание 07: /req-data (демонстрация)
                case '/req-data':
                    let requestDataInfo = '';
                    let chunkCount = 0;
                    req.on('data', (chunk) => {
                        chunkCount++;
                        console.log(`Received chunk #${chunkCount}, size: ${chunk.length}`);
                        requestDataInfo += `Chunk ${chunkCount} received (${chunk.length} bytes)\n`;
                    });
                    req.on('end', () => {
                        console.log('Request data end.');
                        requestDataInfo += 'End of request data.\n';
                         res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                         res.end('Сервер готов принимать данные. Информация о чанках в консоли.\n' + requestDataInfo);
                    });
                    break;


                // Задание 08: /resp-status?code=c&mess=m
                case '/resp-status':
                    const code = parseInt(query.code);
                    const mess = query.mess || http.STATUS_CODES[code] || 'Unknown Status'; // Используем стандартное сообщение если mess не задан
                    if (!isNaN(code) && code >= 100 && code <= 599) {
                        res.writeHead(code, mess, { 'Content-Type': 'text/plain; charset=utf-8' });
                        res.end(`Статус: ${code}, Сообщение: ${mess}`);
                    } else {
                        sendError(res, 400, 'Некорректный или отсутствующий параметр code (должно быть число от 100 до 599).');
                    }
                    break;

                 // Задание 09: /formparameter (отображение формы)
                case '/formparameter':
                    sendHtml(res, path.join(__dirname, 'formparameter.html'));
                    break;

                // Задание 12: /files
                case '/files':
                    try {
                        const files = await fs.readdir(STATIC_DIR);
                        const fileCount = files.length;
                        res.setHeader('X-Static-Files-Count', fileCount.toString()); // Заголовок
                        res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                        res.end(`Количество файлов в директории static: ${fileCount}`);
                    } catch (err) {
                        console.error('Error reading static directory:', err);
                        sendError(res, 500, 'Ошибка чтения директории static.');
                    }
                    break;

                // Задание 14: /upload (отображение формы)
                case '/upload':
                    sendHtml(res, path.join(__dirname, 'upload.html'));
                    break;

                default:
                    // Задание 04: /parameter/x/y
                    const paramMatch = pathname.match(/^\/parameter\/([^/]+)\/([^/]+)$/);
                    if (paramMatch) {
                        const xPath = parseFloat(decodeURIComponent(paramMatch[1]));
                        const yPath = parseFloat(decodeURIComponent(paramMatch[2]));
                        if (!isNaN(xPath) && !isNaN(yPath)) {
                             const sum = xPath + yPath;
                             const diff = xPath - yPath;
                             const prod = xPath * yPath;
                             const quot = yPath !== 0 ? xPath / yPath : 'Деление на ноль!';
                             res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                             res.end(`x = ${xPath}, y = ${yPath}\nСумма: ${sum}\nРазность: ${diff}\nПроизведение: ${prod}\nЧастное: ${quot}`);
                        } else {
                             // Пункт 14 Задания 04: вывести URI
                            res.writeHead(400, { 'Content-Type': 'text/plain; charset=utf-8' });
                            res.end(`Ошибка: параметры x и y в пути (${pathname}) должны быть числами.`);
                        }
                    // Задание 13: /files/filename
                    } else if (pathname.startsWith('/files/')) {
                        const filename = decodeURIComponent(pathname.substring('/files/'.length));
                        // Предотвращение обхода каталога
                        if (filename.includes('..')) {
                            sendError(res, 400, 'Недопустимое имя файла.');
                            return;
                        }
                        const filePath = path.join(STATIC_DIR, filename);
                        try {
                            await fs.access(filePath); // Проверка существования
                            const fileStream = fsSync.createReadStream(filePath);
                             // Простой способ определить Content-Type (можно улучшить)
                            let contentType = 'application/octet-stream';
                            const ext = path.extname(filename).toLowerCase();
                            if (ext === '.txt') contentType = 'text/plain; charset=utf-8';
                            else if (ext === '.html') contentType = 'text/html; charset=utf-8';
                            else if (ext === '.css') contentType = 'text/css; charset=utf-8';
                            else if (ext === '.js') contentType = 'text/javascript; charset=utf-8';
                            else if (ext === '.png') contentType = 'image/png';
                            else if (ext === '.jpg' || ext === '.jpeg') contentType = 'image/jpeg';
                            else if (ext === '.json') contentType = 'application/json; charset=utf-8';
                            else if (ext === '.xml') contentType = 'application/xml; charset=utf-8';

                            res.writeHead(200, { 'Content-Type': contentType });
                            fileStream.pipe(res);
                            fileStream.on('error', (err) => {
                                console.error('Error streaming file:', err);
                                // Ответ уже мог быть частично отправлен, сложно отправить красивую ошибку
                                if (!res.headersSent) {
                                    sendError(res, 500, 'Ошибка чтения файла.');
                                } else {
                                    res.end(); // Просто завершаем, если заголовки отправлены
                                }
                            });
                        } catch (err) { // Ошибка от fs.access (файл не найден или нет доступа)
                            console.log(`File not found or access denied: ${filePath}`);
                            sendError(res, 404, `Файл '${filename}' не найден.`); // Задание 35
                        }
                    } else {
                        sendError(res, 404, 'Ресурс не найден (404 Not Found).');
                    }
                    break;
            }
        } else if (method === 'POST') {
            let body = '';
            req.on('data', chunk => body += chunk.toString());

            switch (pathname) {
                // Задание 07: /req-data (обработка для POST)
                case '/req-data':
                    let requestDataInfoPost = '';
                    let chunkCountPost = 0;
                    console.log(`Receiving data for POST ${pathname}...`);

                    req.on('data', (chunk) => {
                        chunkCountPost++;
                        body += chunk.toString(); // Собираем тело, если оно вдруг нужно целиком
                        console.log(`POST /req-data: Received chunk #${chunkCountPost}, size: ${chunk.length}`);
                        requestDataInfoPost += `Chunk ${chunkCountPost} received (${chunk.length} bytes)\n`;
                    });

                    req.on('end', () => {
                        console.log('POST /req-data: Request data end.');
                        requestDataInfoPost += 'End of request data.\n';
                        res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
                        res.end('POST /req-data: Данные получены. Информация о чанках в консоли.\nTotal body length: ' + body.length + '\n' + requestDataInfoPost);
                    });
                    break;
                // Задание 09: /formparameter (обработка данных)
                case '/formparameter':
                     req.on('end', () => {
                         const params = querystring.parse(body);
                         let responseText = '<h2>Полученные параметры формы:</h2><pre>';
                         for (const key in params) {
                             responseText += `${key}: ${params[key]}\n`;
                         }
                         responseText += '</pre>';
                         res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
                         res.end(responseText);
                     });
                    break;

                // Задание 10: /json
                case '/json':
                    req.on('end', () => {
                        if (req.headers['content-type'] !== 'application/json') {
                           sendError(res, 400, 'Требуется Content-Type: application/json');
                           return;
                        }
                        try {
                           const requestJson = JSON.parse(body);
                           // Проверка структуры запроса (примерная)
                            if (typeof requestJson.x !== 'number' || typeof requestJson.y !== 'number' ||
                                typeof requestJson.s !== 'string' || !Array.isArray(requestJson.m) ||
                                typeof requestJson.o !== 'object' || requestJson.o === null ||
                                typeof requestJson.o.surname !== 'string' || typeof requestJson.o.name !== 'string') {
                                throw new Error('Некорректная структура JSON запроса.');
                            }

                           // Формирование ответа
                           const responseJson = {
                               "_comment": "Ответ. Лабораторная работа 8/10",
                               "x_plus_y": requestJson.x + requestJson.y,
                               "Concatination_s_o": `${requestJson.s}: ${requestJson.o.surname}, ${requestJson.o.name}`,
                               "Length_m": requestJson.m.length
                           };
                           res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
                           res.end(JSON.stringify(responseJson, null, 2)); // Форматированный вывод
                        } catch (e) {
                            console.error("JSON parse error:", e.message);
                            sendError(res, 400, `Ошибка разбора JSON: ${e.message}`);
                        }
                    });
                    break;

                // Задание 11: /xml
                case '/xml':
                    req.on('end', () => {
                        // Проверяем Content-Type (может быть text/xml или application/xml)
                        const contentType = req.headers['content-type'];
                        if (!contentType || (!contentType.includes('application/xml') && !contentType.includes('text/xml'))) {
                            sendError(res, 400, 'Требуется Content-Type: application/xml или text/xml');
                            return;
                        }
                        parseString(body, (err, result) => {
                            if (err) {
                                console.error("XML parse error:", err.message);
                                sendError(res, 400, `Ошибка разбора XML: ${err.message}`);
                                return;
                            }
                            try {
                                // Обработка данных из XML
                                let sumX = 0;
                                let concatM = '';
                                if (result.request && result.request.x && Array.isArray(result.request.x)) {
                                    result.request.x.forEach(el => {
                                        if (el.$ && el.$.value) {
                                           const val = parseFloat(el.$.value);
                                           if (!isNaN(val)) sumX += val;
                                        }
                                    });
                                }
                                if (result.request && result.request.m && Array.isArray(result.request.m)) {
                                    result.request.m.forEach(el => {
                                        if (el.$ && el.$.value) concatM += el.$.value;
                                    });
                                }

                                // Создание ответного XML
                                const responseXml = xmlbuilder.create('response')
                                    .att('id', '33') // Примерный ID, как в задании
                                    .att('request', result.request.$.id || 'unknown') // ID из запроса
                                    .comment(' ответ на запрос ' + (result.request.$.id || 'unknown') + ' ')
                                    .ele('sum')
                                        .att('element', 'x')
                                        .att('result', sumX.toString())
                                    .up() // Возвращаемся на уровень <response>
                                    .ele('concat')
                                        .att('element', 'm')
                                        .att('result', concatM)
                                    .end({ pretty: true }); // Форматированный вывод

                                res.writeHead(200, { 'Content-Type': 'application/xml; charset=utf-8' });
                                res.end(responseXml);

                            } catch (e) {
                                console.error("XML processing error:", e.message);
                                sendError(res, 500, `Ошибка обработки XML данных: ${e.message}`);
                            }
                        });
                    });
                    break;

                 // Задание 14: /upload (обработка загрузки)
                case '/upload':
                    const form = new multiparty.Form({ uploadDir: UPLOAD_DIR });
                    let responseText = '<h2>Результат загрузки файла:</h2>';
                    let fileUploaded = false;

                    form.on('error', (err) => {
                        console.error('File upload error:', err.stack);
                        sendError(res, 500, `Ошибка загрузки файла: ${err.message}`);
                    });

                    form.on('close', () => {
                         console.log('File upload finished.');
                        if (!fileUploaded) { // Если файл не был загружен, сообщаем
                             responseText += '<p>Файл не был загружен.</p>';
                        }
                         res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
                         res.end(responseText);
                    });

                    form.on('field', (name, value) => {
                        console.log(`Received field: ${name} = ${value}`);
                        responseText += `<p>Поле <b>${name}</b>: ${value}</p>`;
                    });

                    form.on('file', (name, file) => {
                        fileUploaded = true;
                        console.log(`Received file: ${name}`);
                        console.log('  Original Filename:', file.originalFilename);
                        console.log('  Path on Server:', file.path);
                        console.log('  Size:', file.size);
                        responseText += `<p>Файл <b>${name}</b>:</p><ul>`;
                        responseText += `<li>Имя оригинала: ${file.originalFilename}</li>`;
                        responseText += `<li>Путь на сервере: ${file.path}</li>`;
                        responseText += `<li>Размер: ${file.size} байт</li></ul>`;
                         // Пункт 38: Сервер сохраняет файл - multiparty делает это автоматически в uploadDir
                    });

                    // Запускаем парсинг формы
                    form.parse(req);
                    break;

                default:
                    sendError(res, 404, 'Ресурс не найден для метода POST.');
                    break;
            }

        } else {
            sendError(res, 405, `Метод ${method} не поддерживается.`);
        }
    } catch (e) {
        console.error("Unhandled error:", e);
        if (!res.headersSent) {
            sendError(res, 500, "Внутренняя ошибка сервера.");
        } else {
             res.end();
        }
    }
};

const server = http.createServer(requestHandler);

server.on('connection', (socket) => {
});

server.on('error', (err) => {
    console.error('Server error:', err);
    if (err.code === 'EADDRINUSE') {
        console.error(`Error: Port ${PORT} is already in use.`);
    }
    process.exit(1);
});

server.on('clientError', (err, socket) => {
  console.error('Client error:', err);
  socket.end('HTTP/1.1 400 Bad Request\r\n\r\n');
});

server.listen(PORT, () => {
    console.log(`Server 08-00 running at http://localhost:${PORT}/`);
    console.log(`Static directory: ${STATIC_DIR}`);
    console.log(`Upload directory: ${UPLOAD_DIR}`);
    console.log(`Initial KeepAliveTimeout: ${server.keepAliveTimeout}`);
});