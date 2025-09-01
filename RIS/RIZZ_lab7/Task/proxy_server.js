const dgram = require('dgram');
const fs = require('fs');

// Конфигурация
const CONFIG_PATH = './config.json';
let config;
try {
    config = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
} catch (err) {
    console.error(`[КРИТИЧЕСКАЯ ОШИБКА] Не удалось загрузить файл конфигурации ${CONFIG_PATH}:`, err);
    process.exit(1);
}

const listenIp = config.proxy_listen_ip || '0.0.0.0'; // IP для прослушивания
const listenPort = config.proxy_listen_port; // Порт для прослушивания
const allCbbServers = config.cbb_servers; // Список всех серверов CBB

// Состояние
let currentCoordinator = null;

let pendingClientRequest = null;
const PENDING_REQUEST_TIMEOUT = 5000;

// UDP Сокет
const socket = dgram.createSocket('udp4');

// Логирование
function log(message) {
    const now = new Date();
    console.log(`[${now.toISOString()}] [ПРОКСИ ${listenIp}:${listenPort}] ${message}`);
}

// Вспомогательные функции
function send(targetIp, targetPort, message) {
    const buffer = Buffer.from(message);
    socket.send(buffer, 0, buffer.length, targetPort, targetIp, (err) => {
        if (err) {
            log(`Ошибка отправки сообщения "${message}" на ${targetIp}:${targetPort}: ${err.message}`);
        }
    });
}

// Обработка сообщений
socket.on('message', (msg, rinfo) => {
    const message = msg.toString();
    const senderIp = rinfo.address;
    const senderPort = rinfo.port;

    log(`Получено сообщение "${message}" от ${senderIp}:${senderPort}`);

    if (message.startsWith('COORDINATOR:')) {
        const newCoordIp = message.split(':')[1];
        const newCoordInfo = allCbbServers.find(s => s.ip === newCoordIp);

        if (newCoordInfo) {
            if (!currentCoordinator || currentCoordinator.ip !== newCoordInfo.ip) {
                log(`>>> Получено объявление нового координатора: ${newCoordIp}:${newCoordInfo.port} (от ${senderIp}:${senderPort}) <<<`);
                currentCoordinator = { ip: newCoordInfo.ip, port: newCoordInfo.port };
                pendingClientRequest = null;
            } else {
                log(`Получено повторное объявление координатора ${newCoordIp}. Игнорирую.`);
            }
        } else {
            log(`Предупреждение: Получено сообщение COORDINATOR для неизвестного IP ${newCoordIp} от ${senderIp}:${senderPort}. Игнорирую.`);
        }
        return;
    }

    // Дальнейшая обработка ТОЛЬКО если это НЕ сообщение COORDINATOR

    // Проверяем, является ли отправитель ТЕКУЩИМ координатором
    if (currentCoordinator && senderIp === currentCoordinator.ip && senderPort === currentCoordinator.port) {
        log(`Получен ответ от координатора ${senderIp}:${senderPort}`);
        if (pendingClientRequest) {
            log(`Перенаправление ответа "${message}" исходному клиенту ${pendingClientRequest.clientAddr}:${pendingClientRequest.clientPort}`);
            send(pendingClientRequest.clientAddr, pendingClientRequest.clientPort, message);
            pendingClientRequest = null; // Clear pending request
        } else {
            log(`Предупреждение: Получен ответ от координатора, но ожидающий запрос клиента не найден. Отбрасываю.`);
        }
        return;
    }

    // Если это не сообщение о координаторе и не ответ от координатора,
    // считаем, что это запрос от клиента

    if (!currentCoordinator) {
        log(`Получен запрос клиента от ${senderIp}:${senderPort}, но координатор неизвестен. Отправляю ошибку.`);
        send(senderIp, senderPort, 'ERROR: Сервис недоступен (Нет координатора)');
        return;
    }

    log(`Получен запрос клиента от ${senderIp}:${senderPort}. Перенаправление координатору ${currentCoordinator.ip}:${currentCoordinator.port}`);

    if (pendingClientRequest) {
        log(`Предупреждение: Перезапись предыдущего ожидающего запроса клиента от ${pendingClientRequest.clientAddr}`);
    }
    pendingClientRequest = { clientAddr: senderIp, clientPort: senderPort, timestamp: Date.now() };

    send(currentCoordinator.ip, currentCoordinator.port, message); // Перенаправляем исходное сообщение

    setTimeout(() => {
        if (pendingClientRequest && pendingClientRequest.clientAddr === senderIp && pendingClientRequest.clientPort === senderPort) {
             log(`Таймаут ожидания ответа координатора для клиента ${senderIp}:${senderPort}. Очистка ожидающего запроса.`);
             pendingClientRequest = null;
        }
    }, PENDING_REQUEST_TIMEOUT);

});

// Привязка сокета и запуск
socket.on('listening', () => {
    const address = socket.address();
    log(`Прокси-сервер UDP слушает на ${address.address}:${address.port}`);
    log('Ожидание сообщения COORDINATOR от серверов CBB...');
});

socket.on('error', (err) => {
    log(`Ошибка сокета:\n${err.stack}`);
    socket.close();
    process.exit(1);
});

socket.bind(listenPort, listenIp);

log(`Прокси-сервер запускается, слушает на ${listenIp}:${listenPort}...`);

process.on('SIGINT', () => {
    log('Завершение работы прокси...');
    socket.close(() => {
        log('Сокет закрыт.');
        process.exit(0);
    });
});