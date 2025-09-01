const http = require('http');
const url = require('url');
const fs = require('fs');
const readline = require('readline');
const data = require('./DBmodule.js');

const db = new data.DB();

let shutdownTimer = null;
let commitInterval = null;
let statsCollection = null;
let stats = {
    start: null,
    end: null,
    requests: 0,
    commits: 0
};

// Обработчики событий БД
db.on('GET', (req, res) => {
    console.log('DB.GET');
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    res.end(JSON.stringify(db.select()));
});

db.on('POST', (req, res) => {
    console.log('DB.POST');
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
        let record = JSON.parse(body);

        let existing = db.select().find(r => r.id === record.id);
        if (existing) {
            res.writeHead(409, { 'Content-Type': 'text/plain; charset=utf-8' });
            res.end('Запись с таким ID уже существует');
            return;
        }

        db.insert(record);
        res.writeHead(201, { 'Content-Type': 'application/json; charset=utf-8' });
        res.end(JSON.stringify(record));
    });
});

db.on('PUT', (req, res) => {
    console.log('DB.PUT');
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
        let record = JSON.parse(body);
        let updated = db.update(record);
        if (updated) {
            res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
            res.end(JSON.stringify(updated));
        } else {
            res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
            res.end('Record not found');
        }
    });
});

db.on('DELETE', (req, res) => {
    console.log('DB.DELETE');
    let query = url.parse(req.url, true).query;
    let id = parseInt(query.id);
    let deleted = db.delete(id);
    if (deleted) {
        res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
        res.end(JSON.stringify(deleted));
    } else {
        res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
        res.end('Record not found');
    }
});

// Обработчик GET-запроса на /api/ss
http.createServer((req, res) => {
    let pathname = url.parse(req.url).pathname;

    if (pathname === '/') {
        let html = fs.readFileSync('./05-01.html');
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end(html);
    } else if (pathname === '/api/db') {
        db.emit(req.method, req, res);
    } else if (pathname === '/api/ss') {
        res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
        res.end(JSON.stringify(stats));
    } else {
        res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
        res.end('Not Found');
    }
}).listen(3000, () => console.log('Server running at http://localhost:3000'));

// Чтение команд из консоли
const rl = readline.createInterface({ input: process.stdin, output: process.stdout });

rl.on('line', (input) => {
    let args = input.trim().split(/\s+/);
    let command = args[0];
    let param = args.length > 1 ? parseInt(args[1]) : null;

    switch (command) {
        case 'sd':
            if (shutdownTimer) {
                clearTimeout(shutdownTimer);
                console.log('Остановка сервера отменена.');
            }
            if (param) {
                shutdownTimer = setTimeout(() => {
                    console.log('Сервер остановлен.');
                    process.exit(0);
                }, param * 1000);
                console.log(`Сервер остановится через ${param} секунд.`);
            }
            break;

        case 'sc':
            if (commitInterval) {
                clearInterval(commitInterval);
                console.log('Периодическая фиксация отключена.');
            }
            if (param) {
                commitInterval = setInterval(() => {
                    db.commit();
                }, param * 1000);
                commitInterval.unref();
                console.log(`Фиксация состояния БД каждые ${param} секунд включена.`);
            }
            break;

        case 'ss':
            if (statsCollection) {
                clearTimeout(statsCollection);
                console.log('Сбор статистики остановлен.');
                stats.end = new Date().toISOString();
            }
            if (param) {
                stats = {
                    start: new Date().toISOString(),
                    end: null,
                    requests: db.requestsCount,
                    commits: db.commitsCount
                };
                statsCollection = setTimeout(() => {
                    stats.end = new Date().toISOString();
                    console.log('Сбор статистики завершен.');
                }, param * 1000);
                statsCollection.unref();
                console.log(`Сбор статистики запущен на ${param} секунд.`);
            }
            break;

        default:
            console.log('Неизвестная команда.');
    }
});
