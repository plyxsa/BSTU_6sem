const fs = require('fs').promises; 
const path = require('path');
const url = require('url');

// Карта MIME-типов
const MIME_TYPES = {
    'html': 'text/html',
    'css': 'text/css',
    'js': 'text/javascript',
    'png': 'image/png',
    'docx': 'application/msword',
    'json': 'application/json',
    'xml': 'application/xml',
    'mp4': 'video/mp4',
    'default': 'application/octet-stream'
};

const TEXT_BASED_TYPES = new Set([
    'text/html',
    'text/css',
    'text/javascript',
    'application/json',
    'application/xml'
]);

// Функция для создания обработчика запросов, параметризованная именем статической директории
function createStaticFileHandler(staticDirName) {

    return async (req, res) => {
        console.log(`${req.method} ${req.url}`);

        if (req.method !== 'GET') {
            res.writeHead(405, { 'Content-Type': 'text/plain; charset=utf-8', 'Allow': 'GET' }); // Добавим charset и сюда
            res.end('Метод не разрешен (405 Method Not Allowed)');
            console.log(`-> 405 Метод не разрешен`);
            return;
        }

        try {
            let requestedPath = url.parse(req.url).pathname;
            let relativePath = requestedPath.substring(1);

            if (relativePath === '') {
                relativePath = 'index.html';
            }

            relativePath = decodeURIComponent(relativePath);

            const baseDir = path.resolve(process.cwd(), staticDirName);
            const fullPath = path.join(baseDir, relativePath);

            if (!fullPath.startsWith(baseDir)) {
                 console.error(`Попытка обхода каталога: ${requestedPath}`);
                 throw new Error('Forbidden');
            }

            const stats = await fs.stat(fullPath);

            if (!stats.isFile()) {
                throw new Error('Not a file');
            }

            // Определяем MIME-тип по расширению
            const extension = path.extname(fullPath).substring(1).toLowerCase();
            let contentType = MIME_TYPES[extension] || MIME_TYPES['default'];

             // Проверяем, разрешено ли расширение явно из таблицы
             if (!MIME_TYPES[extension]) {
                 console.log(`-> 404 Не найдено (Неподдерживаемое расширение: ${extension})`);
                 res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
                 res.end('404 Не найдено: Неподдерживаемый тип файла');
                 return;
             }

            if (TEXT_BASED_TYPES.has(contentType)) {
                contentType += '; charset=utf-8';
            }

            const fileContent = await fs.readFile(fullPath);

            res.writeHead(200, { 'Content-Type': contentType });
            res.end(fileContent);
            console.log(`-> 200 OK (${contentType})`);

        } catch (error) {
            console.error(`Ошибка при обработке ${req.url}:`, error.message);

            const errorContentType = 'text/plain; charset=utf-8';
            if (error.code === 'ENOENT' || error.message === 'Not a file' || error.message === 'Forbidden') {
                res.writeHead(404, { 'Content-Type': errorContentType });
                res.end('404 Не найдено (Not Found)');
                console.log(`-> 404 Не найдено`);
            } else {
                res.writeHead(500, { 'Content-Type': errorContentType });
                res.end('500 Внутренняя ошибка сервера (Internal Server Error)');
                console.log(`-> 500 Внутренняя ошибка сервера`);
            }
        }
    };
}

module.exports = createStaticFileHandler;