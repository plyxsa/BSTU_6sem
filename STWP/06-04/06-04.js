const m0603 = require('m0603-pol-lop').default;

async function main() {
  try {
    await m0603.send('Это сообщение от приложения 06-04!');
    console.log('Письмо успешно отправлено из приложения 06-04!');
  } catch (error) {
    console.error('Ошибка в приложении 06-04:', error);
  }
}

main();