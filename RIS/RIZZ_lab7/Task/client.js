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

const proxyIp = (config.proxy_listen_ip === '0.0.0.0' || config.proxy_listen_ip === '::') ? '127.0.0.1' : config.proxy_listen_ip;
const proxyPort = config.proxy_listen_port;
const requestMessage = 'GET_TIME';
const RESPONSE_TIMEOUT = 3000;

const client = dgram.createSocket('udp4');
let responseReceived = false;

console.log(`Отправка "${requestMessage}" на прокси по адресу ${proxyIp}:${proxyPort}`);
const buffer = Buffer.from(requestMessage);

client.on('message', (msg, rinfo) => {
    console.log(`Получен ответ от ${rinfo.address}:${rinfo.port}: ${msg.toString()}`);
    responseReceived = true;
    client.close();
});

client.on('error', (err) => {
    console.error(`Ошибка сокета клиента:\n${err.stack}`);
    client.close();
    process.exit(1);
});

client.send(buffer, 0, buffer.length, proxyPort, proxyIp, (err) => {
    if (err) {
        console.error(`Ошибка отправки сообщения: ${err.message}`);
        client.close();
    } else {
        setTimeout(() => {
            if (!responseReceived) {
                console.error('Таймаут: Ответ от прокси не получен.');
                client.close();
            }
        }, RESPONSE_TIMEOUT);
    }
});