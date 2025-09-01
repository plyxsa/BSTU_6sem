// server.js
const express = require('express');
const { join } = require('path');
const db = require('./db.js'); 

const { Faculty, Pulpit, Subject, AuditoriumType, Auditorium, sequelize } = db;

const app = express();
const PORT = 3000;

// Middleware для парсинга JSON тел запросов
app.use(express.json()); 

app.use(express.static(join(__dirname, 'public')));

// --- Обработчик ошибок ---
const errorHandler = (err, req, res, entityName) => {
    console.error(`Ошибка при работе с ${entityName}:`, err);
    if (err.name === 'SequelizeValidationError' || err.name === 'SequelizeUniqueConstraintError') {
        res.status(400).json({ error: `Ошибка валидации: ${err.message}`, details: err.errors });
    } else if (err.name === 'SequelizeForeignKeyConstraintError') {
        res.status(400).json({ error: `Ошибка внешнего ключа: ${err.message}`, details: err.parent?.message });
    } else {
        res.status(500).json({ error: `Внутренняя ошибка сервера при работе с ${entityName}`, details: err.message });
    }
};

// --- CRUD Функции-обертки ---

// GET all
const getAllHandler = (model, entityName) => async (req, res) => {
    try {
        const items = await model.findAll();
        res.json(items);
    } catch (err) {
        errorHandler(err, req, res, entityName);
    }
};

// POST create
const createHandler = (model, entityName) => async (req, res) => {
    try {
        const newItem = await model.create(req.body);
        res.status(201).json(newItem);
    } catch (err) {
        errorHandler(err, req, res, entityName);
    }
};

// PUT update
const updateHandler = (model, pkField, entityName) => async (req, res) => {
    try {
        const pkValue = req.body[pkField];
        if (!pkValue) {
            return res.status(400).json({ error: `Не указан первичный ключ '${pkField}' в теле запроса` });
        }
        const [numberOfAffectedRows, affectedRows] = await model.update(req.body, {
            where: { [pkField]: pkValue },
            returning: true,
        });

        if (numberOfAffectedRows > 0) {
             const updatedItem = await model.findByPk(pkValue);
             if (updatedItem) {
                 res.json(updatedItem);
             } else {
                 res.status(200).json({ message: `${entityName} успешно обновлен, но не найден после обновления.` });
             }
        } else {
            res.status(404).json({ error: `${entityName} с ключом ${pkValue} не найден` });
        }
    } catch (err)
{
        errorHandler(err, req, res, entityName);
    }
};

// DELETE by PK
const deleteHandler = (model, pkField, entityName) => async (req, res) => {
    try {
        const pkValue = req.params.xyz;
        if (!pkValue) {
             return res.status(400).json({ error: `Не указан идентификатор в пути запроса (/api/.../xxx)` });
        }
        const numberOfDeletedRows = await model.destroy({
            where: { [pkField]: pkValue }
        });
        if (numberOfDeletedRows > 0) {
            res.json({ message: `${entityName} с ключом ${pkValue} успешно удален`, deletedCount: numberOfDeletedRows });
        } else {
            res.status(404).json({ error: `${entityName} с ключом ${pkValue} не найден` });
        }
    } catch (err) {
        errorHandler(err, req, res, entityName);
    }
};

// --- Маршруты API ---

// Faculty routes
app.get('/api/faculties', getAllHandler(Faculty, 'Faculty'));
app.post('/api/faculties', createHandler(Faculty, 'Faculty'));
app.put('/api/faculties', updateHandler(Faculty, 'FACULTY', 'Faculty'));
app.delete('/api/faculties/:xyz', deleteHandler(Faculty, 'FACULTY', 'Faculty'));

// Pulpit routes
app.get('/api/pulpits', getAllHandler(Pulpit, 'Pulpit'));
app.post('/api/pulpits', createHandler(Pulpit, 'Pulpit'));
app.put('/api/pulpits', updateHandler(Pulpit, 'PULPIT', 'Pulpit'));
app.delete('/api/pulpits/:xyz', deleteHandler(Pulpit, 'PULPIT', 'Pulpit'));

// Subject routes
app.get('/api/subjects', getAllHandler(Subject, 'Subject'));
app.post('/api/subjects', createHandler(Subject, 'Subject'));
app.put('/api/subjects', updateHandler(Subject, 'SUBJECT', 'Subject'));
app.delete('/api/subjects/:xyz', deleteHandler(Subject, 'SUBJECT', 'Subject'));

// AuditoriumType routes
app.get('/api/auditoriumstypes', getAllHandler(AuditoriumType, 'AuditoriumType'));
app.post('/api/auditoriumstypes', createHandler(AuditoriumType, 'AuditoriumType'));
app.put('/api/auditoriumstypes', updateHandler(AuditoriumType, 'AUDITORIUM_TYPE', 'AuditoriumType'));
app.delete('/api/auditoriumtypes/:xyz', deleteHandler(AuditoriumType, 'AUDITORIUM_TYPE', 'AuditoriumType'));

// Auditorium routes
app.get('/api/auditoriums', getAllHandler(Auditorium, 'Auditorium'));
app.post('/api/auditoriums', createHandler(Auditorium, 'Auditorium'));
app.put('/api/auditoriums', updateHandler(Auditorium, 'AUDITORIUM', 'Auditorium'));
app.delete('/api/auditoriums/:xyz', deleteHandler(Auditorium, 'AUDITORIUM', 'Auditorium'));

// --- Запуск сервера ---
sequelize.authenticate()
    .then(() => {
        console.log('Соединение с базой данных установлено успешно.');
        // return sequelize.sync(); // db.sequelize.sync() or just sequelize.sync()
    })
    .then(() => {
        app.listen(PORT, () => {
            console.log(`Сервер запущен на порту ${PORT}`);
            console.log(`Перейдите на http://localhost:${PORT}/`);
        });
    })
    .catch(err => {
        console.error('Не удалось подключиться к базе данных:', err);
    });