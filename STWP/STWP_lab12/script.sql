USE LPV_DB;
GO

-- 1. Создание таблицы FACULTY
CREATE TABLE FACULTY (
    FACULTY NVARCHAR(10) PRIMARY KEY,
    FACULTY_NAME NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- 2. Создание таблицы PULPIT (Кафедра)
CREATE TABLE PULPIT (
    PULPIT NVARCHAR(20) PRIMARY KEY,
    PULPIT_NAME NVARCHAR(100) NOT NULL,
    FACULTY NVARCHAR(10) NOT NULL,
    FOREIGN KEY (FACULTY) REFERENCES FACULTY(FACULTY) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 3. Создание таблицы TEACHER (Преподаватель)
CREATE TABLE TEACHER (
    TEACHER NVARCHAR(20) PRIMARY KEY,
    TEACHER_NAME NVARCHAR(100) NOT NULL,
    PULPIT NVARCHAR(20) NOT NULL,
    FOREIGN KEY (PULPIT) REFERENCES PULPIT(PULPIT) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 4. Создание таблицы SUBJECT (Дисциплина)
CREATE TABLE SUBJECT (
    SUBJECT NVARCHAR(20) PRIMARY KEY,
    SUBJECT_NAME NVARCHAR(100) NOT NULL,
    PULPIT NVARCHAR(20) NOT NULL,
    FOREIGN KEY (PULPIT) REFERENCES PULPIT(PULPIT) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 5. Создание таблицы AUDITORIUM_TYPE (Тип аудитории)
CREATE TABLE AUDITORIUM_TYPE (
    AUDITORIUM_TYPE NVARCHAR(10) PRIMARY KEY,
    AUDITORIUM_TYPENAME NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 6. Создание таблицы AUDITORIUM (Аудитория)
CREATE TABLE AUDITORIUM (
    AUDITORIUM NVARCHAR(20) PRIMARY KEY,
    AUDITORIUM_NAME NVARCHAR(50),
    AUDITORIUM_CAPACITY INT NOT NULL CHECK (AUDITORIUM_CAPACITY > 0),
    AUDITORIUM_TYPE NVARCHAR(10) NOT NULL,
    FOREIGN KEY (AUDITORIUM_TYPE) REFERENCES AUDITORIUM_TYPE(AUDITORIUM_TYPE) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- --- Заполнение таблиц данными ---

-- Заполнение FACULTY
INSERT INTO FACULTY (FACULTY, FACULTY_NAME) VALUES
('ИДиП', N'Издательское дело и полиграфия'),
('ХТиТ', N'Химическая технология и техника'),
('ЛХФ', N'Лесохозяйственный факультет'),
('ИЭФ', N'Инженерно-экономический факультет'),
('ТОВ', N'Технология органических веществ'),
('ТТЛП', N'Технология и техника лесной промышленности'),
('ИТ', N'Информационных технологий');
GO

-- Заполнение AUDITORIUM_TYPE
INSERT INTO AUDITORIUM_TYPE (AUDITORIUM_TYPE, AUDITORIUM_TYPENAME) VALUES
('ЛК', N'Лекционная'),
('ЛБ-К', N'Компьютерный класс'),
('ЛБ-Х', N'Химическая лаборатория'),
('ЛК-К', N'Лекционная с компьютерами');
GO

-- Заполнение PULPIT
INSERT INTO PULPIT (PULPIT, PULPIT_NAME, FACULTY) VALUES
('ИСиТ', N'Информационных систем и технологий', 'ИТ'),
('ПОИСОИ', N'Полиграфического оборудования и систем обработки информации', 'ИДиП'),
('ЛВ', N'Лесоводства', 'ЛХФ'),
('ЭТиМ', N'Экономической теории и маркетинга', 'ИЭФ'),
('ТНХСиППМ', N'Технологии нефтехимического синтеза и переработки полимерных материалов', 'ХТиТ'),
('ПИ', N'Программной инженерии', 'ИТ');
GO


-- Заполнение TEACHER
INSERT INTO TEACHER (TEACHER, TEACHER_NAME, PULPIT) VALUES
('СМЛВ', N'Смелов Владимир Владиславович', 'ИСиТ'),
('БРКВЧ', N'Бракович Андрей Игоревич', 'ИСиТ'),
('ГРН', N'Гурин Николай Иванович', 'ИСиТ'),
('УРБ', N'Урбанович Павел Павлович', 'ПОИСОИ'),
('ЛБХ', N'Лабоха Константин Валентинович', 'ЛВ'),
('НВРК', N'Навроцкий Анатолий Карлович', 'ЭТиМ');
GO

-- Заполнение SUBJECT
INSERT INTO SUBJECT (SUBJECT, SUBJECT_NAME, PULPIT) VALUES
('БД', N'Базы данных', 'ИСиТ'),
('ОАиП', N'Основы алгоритмизации и программирования', 'ИСиТ'),
('ПСП', N'Программирование сетевых приложений', 'ИСиТ'),
('КГ', N'Компьютерная графика', 'ПОИСОИ'),
('ЭП', N'Экономика природопользования', 'ЭТиМ'),
('ЛВВ', N'Лесоводсво', 'ЛВ'),
('ООП', N'Объектно-ориентированное программирование', 'ПИ');
GO

-- Заполнение AUDITORIUM
INSERT INTO AUDITORIUM (AUDITORIUM, AUDITORIUM_NAME, AUDITORIUM_CAPACITY, AUDITORIUM_TYPE) VALUES
('201-4', N'201-4', 30, 'ЛБ-К'),
('202-4', N'202-4', 30, 'ЛБ-К'),
('301-4', N'301-4', 60, 'ЛК'),
('401-4', N'401-4', 90, 'ЛК'),
('101-1', N'101-1', 40, 'ЛБ-Х'),
('102-1', N'102-1', 40, 'ЛБ-Х'),
('324-4', N'324-4', 50, 'ЛК'),
('110-4', N'110-4', 25, 'ЛБ-К');
GO