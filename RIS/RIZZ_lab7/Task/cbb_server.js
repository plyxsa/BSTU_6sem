const dgram = require('dgram');
const fs = require('fs');

const CONFIG_PATH = './config.json';
let config;
try {
    config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
} catch (err) {
    console.error(`[КРИТИЧЕСКАЯ ОШИБКА] Не удалось загрузить файл конфигурации ${CONFIG_PATH}:`, err);
    process.exit(1);
}

// Идентификация
const myIp = process.argv[2];
if (!myIp) {
    console.error(`[КРИТИЧЕСКАЯ ОШИБКА] Пожалуйста, укажите IP-адрес сервера в качестве аргумента командной строки.`);
    console.error(`Пример: node cbb_server.js 192.168.1.101`);
    process.exit(1);
}

const myServerInfo = config.cbb_servers.find(s => s.ip === myIp);
if (!myServerInfo) {
    console.error(`[КРИТИЧЕСКАЯ ОШИБКА] IP-адрес ${myIp} не найден в config.cbb_servers.`);
    process.exit(1);
}
const myPort = myServerInfo.port;
const allServers = config.cbb_servers;
const proxyInfo = { ip: config.proxy_listen_ip, port: config.proxy_listen_port };

// Состояние
let state = {
    isCoordinator: false,
    currentCoordinator: null,
    electionInProgress: false,
    healthCheckTimer: null,
    electionTimeoutTimer: null,
    coordinatorPongTimer: null,
    consecutiveHealthFailures: 0,
};

// UDP Сокет
const socket = dgram.createSocket('udp4');

// Логирование
function log(message) {
    const now = new Date();
    console.log(`[${now.toISOString()}] [${myIp}:${myPort}] ${message}`);
}

// Вспомогательные функции
function send(targetIp, targetPort, message) {
    const buffer = Buffer.from(message);
    socket.send(buffer, 0, buffer.length, targetPort, targetIp, (err) => {
        if (err) {
            log(`Ошибка отправки сообщения "${message}" на ${targetIp}:${targetPort}: ${err.message}`);
        } else {
        }
    });
}

// Новая функция для уведомления прокси через временный сокет
function notifyProxy(message) {
    const proxyTargetIp = (config.proxy_listen_ip === '0.0.0.0' || config.proxy_listen_ip === '::') ? '127.0.0.1' : config.proxy_listen_ip;
    log(`Уведомление Прокси на ${proxyTargetIp}:${proxyInfo.port} о Координаторе`);

    const tempSocket = dgram.createSocket('udp4');
    const buffer = Buffer.from(message);
    tempSocket.send(buffer, 0, buffer.length, proxyInfo.port, proxyTargetIp, (err) => {
        if (err) {
            log(`Ошибка отправки сообщения "${message}" на ${proxyTargetIp}:${proxyInfo.port} через временный сокет: ${err.message}`);
        } else {
             log(`Сообщение COORDINATOR успешно отправлено прокси через временный сокет.`);
        }
        tempSocket.close();
    });
}


function broadcast(message, includeProxy = false) {
    log(`Широковещательная рассылка: "${message}" (Включая прокси: ${includeProxy})`);
    allServers.forEach(server => {
        if (server.ip !== myIp) {
            send(server.ip, server.port, message);
        }
    });
    if (includeProxy && message.startsWith('COORDINATOR:')) {
        notifyProxy(message);
    }
}

function getTimeString() {
    const now = new Date();
    const day = String(now.getDate()).padStart(2, '0');
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const year = now.getFullYear();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    return `${day}${month}${year}:${hours}:${minutes}:${seconds}`;
}

// Алгоритм Забияки и Логика Высокой Доступности

function findHighestIpServer() {
    const ipToNum = ip => ip.split('.').reduce((res, octet) => (res << 8) | parseInt(octet), 0);
    return allServers.reduce((highest, current) => {
        return ipToNum(current.ip) > ipToNum(highest.ip) ? current : highest;
    }, allServers[0]);
}

function becomeCoordinator() {
    log('>>> Становлюсь Координатором <<<');
    state.isCoordinator = true;
    state.currentCoordinator = { ip: myIp, port: myPort };
    state.electionInProgress = false;
    stopHealthChecks();
    clearTimeout(state.electionTimeoutTimer);
    state.electionTimeoutTimer = null;
    broadcast(`COORDINATOR:${myIp}`, true);
}

function startElection() {
    if (state.electionInProgress) {
        log('Выборы уже идут, пропуск.');
        return;
    }
    log('Начинаю Выборы...');
    state.electionInProgress = true;
    state.isCoordinator = false;
    state.currentCoordinator = null;
    stopHealthChecks(); 

    // Ищем серверы с бОльшим IP-адресом
    const ipToNum = ip => ip.split('.').reduce((res, octet) => (res << 8) | parseInt(octet), 0);
    const higherServers = allServers.filter(server => ipToNum(server.ip) > ipToNum(myIp));

    if (higherServers.length === 0) {
        log('Не найдено серверов с бОльшим IP. Объявляю себя координатором.');
        becomeCoordinator();
    } else {
        // Отправляем сообщение ELECTION серверам с бОльшим IP
        log(`Отправляю ELECTION ${higherServers.length} серверам с бОльшим IP.`);
        higherServers.forEach(server => {
            send(server.ip, server.port, 'ELECTION');
        });

        clearTimeout(state.electionTimeoutTimer);
        state.electionTimeoutTimer = setTimeout(() => {
            if (state.electionInProgress) {
                log('Таймаут выборов: Не получен OK от серверов с бОльшим IP. Объявляю себя координатором.');
                becomeCoordinator();
            }
        }, config.election_timeout_ms);
    }
}

function handleCoordinatorFailure() {
    log(`Координатор ${state.currentCoordinator?.ip || 'неизвестен'} не прошел проверку доступности. Начинаю выборы.`);
    state.currentCoordinator = null;
    state.consecutiveHealthFailures = 0;
    stopHealthChecks();
    startElection();
}

function startHealthChecks() {
    stopHealthChecks();
    if (state.isCoordinator || !state.currentCoordinator) {
        return;
    }

    log(`Запускаю проверку доступности для координатора ${state.currentCoordinator.ip}:${state.currentCoordinator.port}`);
    state.healthCheckTimer = setInterval(() => {
        if (state.isCoordinator || !state.currentCoordinator || state.electionInProgress) {
            stopHealthChecks();
            return;
        }

        log(`Отправляю PING координатору ${state.currentCoordinator.ip}`);
        send(state.currentCoordinator.ip, state.currentCoordinator.port, 'PING');

        clearTimeout(state.coordinatorPongTimer);
        state.coordinatorPongTimer = setTimeout(() => {
            state.consecutiveHealthFailures++;
            log(`Таймаут PONG от координатора. Сбоев: ${state.consecutiveHealthFailures}/${config.health_check_failures_allowed}`);
            if (state.consecutiveHealthFailures >= config.health_check_failures_allowed) {
                handleCoordinatorFailure();
            }
        }, config.coordinator_timeout_ms);

    }, config.health_check_interval_ms);
}

function stopHealthChecks() {
    if (state.healthCheckTimer) {
        clearInterval(state.healthCheckTimer);
        state.healthCheckTimer = null;
        log('Остановлены проверки доступности.');
    }
    clearTimeout(state.coordinatorPongTimer);
    state.coordinatorPongTimer = null;
    state.consecutiveHealthFailures = 0;
}

// Обработка сообщений
socket.on('message', (msg, rinfo) => {
    const message = msg.toString();
    log(`Получено "${message}" от ${rinfo.address}:${rinfo.port}`);

    const senderIp = rinfo.address;
    const senderPort = rinfo.port;

    switch (message) {
        case 'GET_TIME':
            if (state.isCoordinator) {
                const timeStr = getTimeString();
                log(`Отвечаю временем: ${timeStr}`);
                send(senderIp, senderPort, timeStr);
            } else {
                log('Получен GET_TIME, но я не координатор. Игнорирую.');
            }
            break;

        case 'PING':
            if (state.isCoordinator) {
                log('Отвечаю PONG');
                send(senderIp, senderPort, 'PONG');
            }
            break;

        case 'PONG':
            if (state.currentCoordinator && senderIp === state.currentCoordinator.ip) {
                log('Получен PONG от координатора. Сброс счетчика сбоев.');
                clearTimeout(state.coordinatorPongTimer);
                state.coordinatorPongTimer = null;
                state.consecutiveHealthFailures = 0;
            }
            break;

        case 'ELECTION':
            log(`Получено ELECTION от ${senderIp}. Отправляю OK и начинаю свои выборы.`);
            send(senderIp, senderPort, 'OK');
            if (!state.electionInProgress) {
                startElection();
            }
            break;

        case 'OK':
            if (state.electionInProgress) {
                log(`Получен OK от ${senderIp}. Сервер с бОльшим IP берет управление. Прекращаю ожидание победы.`);
                state.electionInProgress = false;
                clearTimeout(state.electionTimeoutTimer);
                state.electionTimeoutTimer = null;
            }
            break;

        default:
            if (message.startsWith('COORDINATOR:')) {
                const newCoordIp = message.split(':')[1];
                if (newCoordIp === myIp) {
                     if (!state.isCoordinator) {
                         log(`Получено сообщение COORDINATOR для себя, но не ожидал. Переутверждаю роль.`);
                         becomeCoordinator();
                     }
                     return;
                }

                const newCoordInfo = allServers.find(s => s.ip === newCoordIp);
                if (newCoordInfo) {
                    log(`Объявлен новый координатор: ${newCoordIp}. Обновляю состояние.`);
                    state.isCoordinator = false;
                    state.currentCoordinator = { ip: newCoordInfo.ip, port: newCoordInfo.port };
                    state.electionInProgress = false;
                    clearTimeout(state.electionTimeoutTimer);
                    state.electionTimeoutTimer = null;
                    stopHealthChecks();
                    startHealthChecks();
                } else {
                    log(`Получено сообщение COORDINATOR для неизвестного IP: ${newCoordIp}. Игнорирую.`);
                }
            } else {
                log(`Получен неизвестный тип сообщения: "${message}". Игнорирую.`);
            }
            break;
    }
});

// Привязка сокета и запуск
socket.on('listening', () => {
    const address = socket.address();
    log(`UDP Сервер слушает на ${address.address}:${address.port}`);

    const highestServer = findHighestIpServer();
    log(`Сервер с наибольшим IP в конфиге: ${highestServer.ip}`);

    if (myIp === highestServer.ip) {
        log('У меня наибольший IP. Беру роль координатора изначально.');
        becomeCoordinator();
    } else {
        log(`Устанавливаю начального координатора на ${highestServer.ip}`);
        state.currentCoordinator = { ip: highestServer.ip, port: highestServer.port };
        startHealthChecks();
    }
});

socket.on('error', (err) => {
    log(`Ошибка сокета:\n${err.stack}`);
    socket.close();
    process.exit(1);
});

socket.bind(myPort, myIp);

log(`Сервер CBB запускается с IP ${myIp} на порту ${myPort}...`);

process.on('SIGINT', () => {
    log('Завершение работы...');
    stopHealthChecks();
    clearTimeout(state.electionTimeoutTimer);
    clearTimeout(state.coordinatorPongTimer);
    socket.close(() => {
        log('Сокет закрыт.');
        process.exit(0);
    });
});