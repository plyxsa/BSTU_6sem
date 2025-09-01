const http = require('http');
const createStaticFileHandler = require('./m07-01');

const PORT = 3000;
const STATIC_DIR = 'static';

const requestHandler = createStaticFileHandler(STATIC_DIR);

const server = http.createServer(requestHandler);

server.listen(PORT, (err) => {
    if (err) {
        console.error('Error starting server:', err);
        return;
    }
    console.log(`Server 07-01 running at http://localhost:${PORT}/`);
    console.log(`Serving static files from directory: '${STATIC_DIR}'`);
});

server.on('error', (error) => {
    if (error.code === 'EADDRINUSE') {
        console.error(`Error: Port ${PORT} is already in use.`);
    } else {
        console.error('Server error:', error);
    }
    process.exit(1);
});