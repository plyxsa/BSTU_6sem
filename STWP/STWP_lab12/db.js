const { Sequelize } = require('sequelize');

const DB_NAME = 'LPV_DB';
const DB_USER = 'sa';
const DB_PASSWORD = '696969';
const DB_HOST = 'DESKTOP-VNKLCU9';

const sequelize = new Sequelize(DB_NAME, DB_USER, DB_PASSWORD, {
    host: DB_HOST,
    dialect: 'mssql',
    dialectOptions: {
        options: {
            encrypt: false,
            trustServerCertificate: true
        }
    },
    pool: {
        max: 5, // Макс. кол-во соединений в пуле
        min: 0,
        acquire: 30000, // Макс. время в мс на получение соединения из пула
        idle: 10000 // Макс. время в мс, которое соединение может быть неактивным
    },
    logging: console.log,
});

const db = {};
db.Sequelize = Sequelize;
db.sequelize = sequelize;

db.Faculty = require('./models/Faculty')(sequelize, Sequelize);
db.Pulpit = require('./models/Pulpit')(sequelize, Sequelize);
db.Teacher = require('./models/Teacher')(sequelize, Sequelize);
db.Subject = require('./models/Subject')(sequelize, Sequelize);
db.AuditoriumType = require('./models/AuditoriumType')(sequelize, Sequelize);
db.Auditorium = require('./models/Auditorium')(sequelize, Sequelize);

// Факультет -> Кафедра (Один ко многим)
db.Faculty.hasMany(db.Pulpit, { foreignKey: 'FACULTY', as: 'pulpits', onDelete: 'CASCADE', onUpdate: 'CASCADE' });
db.Pulpit.belongsTo(db.Faculty, { foreignKey: 'FACULTY', as: 'faculty' });

// Кафедра -> Преподаватель (Один ко многим)
db.Pulpit.hasMany(db.Teacher, { foreignKey: 'PULPIT', as: 'teachers', onDelete: 'CASCADE', onUpdate: 'CASCADE' });
db.Teacher.belongsTo(db.Pulpit, { foreignKey: 'PULPIT', as: 'pulpit' });

// Кафедра -> Дисциплина (Один ко многим)
db.Pulpit.hasMany(db.Subject, { foreignKey: 'PULPIT', as: 'subjects', onDelete: 'CASCADE', onUpdate: 'CASCADE' });
db.Subject.belongsTo(db.Pulpit, { foreignKey: 'PULPIT', as: 'pulpit' });

// Тип аудитории -> Аудитория (Один ко многим)
db.AuditoriumType.hasMany(db.Auditorium, { foreignKey: 'AUDITORIUM_TYPE', as: 'auditoriums', onDelete: 'CASCADE', onUpdate: 'CASCADE' });
db.Auditorium.belongsTo(db.AuditoriumType, { foreignKey: 'AUDITORIUM_TYPE', as: 'auditoriumType' });


module.exports = db;