import { createTransport } from 'nodemailer';
import dotenv from 'dotenv';
dotenv.config();

const transporter = createTransport({
  service: 'gmail',
  auth: {
    user: process.env.EMAIL_USER,
    pass: process.env.EMAIL_PASS
  }
});

const recipientEmail = process.env.RECIPIENT_EMAIL;

async function send(messageText) {
  const mailOptions = {
    from: process.env.EMAIL_USER,
    to: recipientEmail,
    subject: 'Новое сообщение!',
    html: messageText
  };

  try {
    await transporter.sendMail(mailOptions);
    console.log('Письмо успешно отправлено!');
  } catch (error) {
    console.error('Ошибка при отправке письма:', error);
    throw error;
  }
}

export default { send };