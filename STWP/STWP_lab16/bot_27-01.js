const https = require('https');

const BOT_TOKEN = '';
let TICK = 0;
let PARAMS = { limit: 10, timeout: 60, offset: 0 };

// Функция для формирования опций HTTPS запроса
function getoptions(method) {
    return {
        host: 'api.telegram.org',
        path: `/bot${BOT_TOKEN}/${method}`,
        port: 443,
        method: 'POST',
        headers: { 'content-type': 'application/json', 'accept': 'application/json' }
    };
}

// Функция для выполнения запроса getUpdates
function reqX(parms, resolve, reject) {
    let rc = { next_parms: parms, tick: TICK, result: [] };
    const req = https.request(getoptions('getUpdates'), (res) => {
        let data = '';
        TICK++;
        res.on('data', (chunk) => { data += chunk.toString('utf8'); });
        res.on('end', () => {
            try {
                const teleg = JSON.parse(data);
                if (teleg && teleg.ok) {
                    if (teleg.result.length > 0) {
                        rc.next_parms.offset = teleg.result[teleg.result.length - 1].update_id + 1;
                    }
                    rc.result = teleg.result;
                    resolve(rc);
                } else {
                    console.error('Telegram API error (getUpdates):', teleg.description || 'Unknown error');
                    reject(`Error 1: API not ok or bad response structure. Description: ${teleg.description}`);
                }
            } catch (e) {
                console.error('Error parsing Telegram response (getUpdates):', e.message, "Data:", data);
                reject(`Error parsing JSON: ${e.message}`);
            }
        });
    });

    req.on('error', (e) => {
        console.error('http.request error (getUpdates):', e.message);
        reject(`Error 2: HTTP request failed - ${e.message}`);
    });

    req.write(JSON.stringify(parms));
    req.end();
}

function TlgGet(parms) {
    return new Promise((resolve, reject) => {
        reqX(parms, resolve, reject);
    });
}

// Функция для форматирования ответного сообщения (echo)
async function formatEchoResponse(incomingText, tick) {
    return `echo: ${incomingText}`;
}

// Функция для отправки сообщений
function tlgout(p_rc_from_TlgGet, p_formatResponseFunc, resolve, reject) {
    if (p_rc_from_TlgGet.result.length > 0) {
        const promises = p_rc_from_TlgGet.result.map(el => {
            if (el.message && el.message.text) {
                return p_formatResponseFunc(el.message.text, TICK).then((responseText) => {
                    return new Promise((msgResolve, msgReject) => {
                        const messageData = {
                            chat_id: el.message.chat.id,
                            parse_mode: 'HTML',
                            text: responseText
                        };
                        const req = https.request(getoptions('sendMessage'), (res) => {
                            let responseData = '';
                            res.on('data', (chunk) => responseData += chunk);
                            res.on('end', () => {
                                console.log(TICK, (new Date()).toISOString(), 'response sent for tick', p_rc_from_TlgGet.tick, 'to chat_id:', el.message.chat.id, 'original:', el.message.text);
                                msgResolve('message sent');
                            });
                        });
                        req.on('error', (err) => {
                            console.error('Error 3 (sendMessage):', err.message);
                            msgReject(`Error 3: sendMessage failed - ${err.message}`);
                        });
                        req.write(JSON.stringify(messageData));
                        req.end();
                    });
                });
            }
            return Promise.resolve('no text in message or not a message update');
        });
        Promise.all(promises)
            .then(() => resolve('all responses processed'))
            .catch(err => reject(err));
    } else {
        console.log(TICK, (new Date()).toISOString(), 'timeout tick (no new messages)', p_rc_from_TlgGet.next_parms);
        resolve('timeout tick');
    }
}

function TlgOut(p_rc_from_TlgGet, p_formatResponseFunc) {
    return new Promise((resolve, reject) => {
        tlgout(p_rc_from_TlgGet, p_formatResponseFunc, resolve, reject);
    });
}

// Функция задержки
function TlgWait(clock, pt) {
    let t = pt || 5000;
    return new Promise((resolve) => {
        setTimeout((c) => {
            resolve(parseInt(((new Date()).getTime() - c) / 1000).toFixed(0));
        }, t, clock);
    });
}

// Основной цикл запросов обновлений
(async () => {
    if (BOT_TOKEN === 'null') {
        console.error("Пожалуйста, установите ваш BOT_TOKEN в коде!");
        return;
    }
    console.log("Бот запускается с параметрами:", PARAMS);
    try {
        let clock = (new Date()).getTime();
        for (let i = 0; ; i++) {
            console.log(`\n[${i}] Ожидание обновлений... offset: ${PARAMS.offset}`);
            const rc1_updates = await TlgGet(PARAMS);
            
            // Отправляем ответ
            const rc2_send_status = await TlgOut(rc1_updates, formatEchoResponse);
            
            const rc3_delay_info = await TlgWait(clock, 1000); // Уменьшим задержку до 1 секунды
            console.log(`[${i}] Итерация завершена. clock = ${rc3_delay_info}s. Статус отправки: ${rc2_send_status}`);
            clock = (new Date()).getTime();
        }
    } catch (err) {
        console.error("Критическая ошибка в основном цикле:", err);
    }
})();

console.log("Echo Bot запущен...");