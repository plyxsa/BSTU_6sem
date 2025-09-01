const http = require('http');
const url = require('url');
const fs = require('fs');
const data = require('./DBmodule.js'); 

const db = new data.DB();

db.on('GET', (req, res) => {
    console.log('DB.GET');
    res.writeHead(200, {'Content-Type': 'application/json; charset=utf-8'});
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
            res.writeHead(409, {'Content-Type': 'text/plain; charset=utf-8'});
            res.end('Запись с таким ID уже существует');
            return;
        }

        db.insert(record);
        res.writeHead(201, {'Content-Type': 'application/json; charset=utf-8'});
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
            res.writeHead(200, {'Content-Type': 'application/json; charset=utf-8'});
            res.end(JSON.stringify(updated));
        } else {
            res.writeHead(404, {'Content-Type': 'text/plain; charset=utf-8'});
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
        res.writeHead(200, {'Content-Type': 'application/json; charset=utf-8'});
        res.end(JSON.stringify(deleted));
    } else {
        res.writeHead(404, {'Content-Type': 'text/plain; charset=utf-8'});
        res.end('Record not found');
    }
});

http.createServer((req, res) => {
    let pathname = url.parse(req.url).pathname;

    if (pathname === '/') {
        let html = fs.readFileSync('./04-02.html');
        res.writeHead(200, {'Content-Type': 'text/html; charset=utf-8'});
        res.end(html);
    } else if (pathname === '/api/db') {
        db.emit(req.method, req, res);
    } else {
        res.writeHead(404, {'Content-Type': 'text/plain; charset=utf-8'});
        res.end('Not Found');
    }
}).listen(3000, () => console.log('Server running at http://localhost:3000'));
