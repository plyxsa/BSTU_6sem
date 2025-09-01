const WebSocket = require('ws');

const WS_URL = 'ws://localhost:4000';

let socket = null;
let clientIntervalId = null;
let timeoutId = null;
let clientN = 0;

function logMessage(message) {
    console.log(`[Client 10-02] ${message}`);
}

function startWebSocketConnection() {
    if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) {
        logMessage('WebSocket connection already open or connecting.');
        return;
    }

    logMessage('Attempting to connect to ' + WS_URL);
    socket = new WebSocket(WS_URL);
    clientN = 0;

    socket.on('open', () => {
        logMessage('WebSocket connection opened.');

        clientIntervalId = setInterval(() => {
            clientN++;
            const message = `10-02-client: ${clientN}`;
            if (socket.readyState === WebSocket.OPEN) {
                logMessage(`Sending: ${message}`);
                socket.send(message);
            } else {
                logMessage('Socket not open, cannot send.');
                stopClientTasks();
            }
        }, 3000);

        timeoutId = setTimeout(() => {
            logMessage('25 second timeout reached. Closing connection.');
            stopClientTasks();
        }, 25000);
    });

    // Simulate Req 7: Display received messages
    socket.on('message', (data) => {
        logMessage(`Received: ${data.toString()}`);
    });

    socket.on('close', (code, reason) => {
        const reasonString = reason ? reason.toString() : 'N/A';
        logMessage(`WebSocket connection closed. Code: ${code}, Reason: ${reasonString}`);
        clearTimers();
        socket = null;
    });

    socket.on('error', (error) => {
        logMessage('WebSocket error: ' + error.message);
        console.error('[Client 10-02] WebSocket Error:', error);
        clearTimers();
        socket = null;
    });
}

function clearTimers() {
    if (clientIntervalId) {
        clearInterval(clientIntervalId);
        clientIntervalId = null;
        logMessage('Client message interval cleared.');
    }
    if (timeoutId) {
        clearTimeout(timeoutId);
        timeoutId = null;
        logMessage('Client timeout cleared.');
    }
}

function stopClientTasks() {
    clearTimers();
    if (socket && socket.readyState === WebSocket.OPEN) {
        socket.close(1000, "Client 10-02 initiated close after timeout");
    }
}

startWebSocketConnection();