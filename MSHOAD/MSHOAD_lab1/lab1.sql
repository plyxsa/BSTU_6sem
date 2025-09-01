-- 2.Продемонстрировать обработку данных из объектных таблиц при помощи коллекций следующим образом по варианту
-- таблица t1 - Университет  вторая t2 - Студенты и группы
-- Университет
CREATE TABLE t1 (
    UniversityId NUMBER PRIMARY KEY,
    UniversityName VARCHAR2(255),
    Location VARCHAR2(255)
);

-- Студенты и группы
CREATE TABLE t2 (
    StudentId NUMBER PRIMARY KEY,
    Name VARCHAR2(255),
    GroupName VARCHAR2(255),
    UniversityId NUMBER,
    FOREIGN KEY (UniversityId) REFERENCES t1 (UniversityId)
);

INSERT INTO t1 VALUES (1, 'МГУ', 'Москва');
INSERT INTO t1 VALUES (2, 'СПбГУ', 'Санкт-Петербург');
INSERT INTO t1 VALUES (3, 'БГТУ', 'Минск');

INSERT INTO t1 VALUES (4, 'БГУ', 'Минск');

INSERT INTO t2 VALUES (1, 'Иванов Иван', 'Группа 1', 1);
INSERT INTO t2 VALUES (2, 'Петрова Мария', 'Группа 1', 1);
INSERT INTO t2 VALUES (3, 'Сидоров Алексей', 'Группа 2', 2);
INSERT INTO t2 VALUES (4, 'Кузнецова Ольга', 'Группа 2', 2);
INSERT INTO t2 VALUES (5, 'Морозов Дмитрий', 'Группа 3', 3);

INSERT INTO t2 VALUES (6, 'Иванов Иван', 'Группа 3', 3);

DELETE FROM t1;
DELETE FROM t2;
COMMIT;


-- a.Создать коллекцию на основе t1, далее K1, для нее как атрибут – вложенную коллекцию на основе t2, далее К2;
CREATE OR REPLACE TYPE Student_obj AS OBJECT (
    StudentId NUMBER,
    Name VARCHAR2(255),
    GroupName VARCHAR2(255)
);

CREATE OR REPLACE TYPE StudentCollection AS TABLE OF Student_obj;

CREATE OR REPLACE TYPE University_obj AS OBJECT (
    UniversityId NUMBER,
    UniversityName VARCHAR2(255),
    Location VARCHAR2(255),
    Students StudentCollection
);         

CREATE OR REPLACE TYPE UniversityCollection AS TABLE OF University_obj;

-- b.Выяснить для каких коллекций К1 коллекции К2 пересекаются;
DECLARE
    K1 UniversityCollection := UniversityCollection();
    intersection_found BOOLEAN := FALSE;
    is_member BOOLEAN := FALSE;
    has_empty_collection BOOLEAN := FALSE;
    cl1 NUMBER := 1;  -- для обмена атрибутами
    cl2 NUMBER := 2;
BEGIN
    FOR rec IN (SELECT * FROM t1) LOOP
        K1.EXTEND;
        K1(K1.COUNT) := University_obj(
            rec.UniversityId,
            rec.UniversityName,
            rec.Location,
            StudentCollection()
        );

        FOR student IN (SELECT * FROM t2 WHERE UniversityId = rec.UniversityId) LOOP
            K1(K1.COUNT).Students.EXTEND;
            K1(K1.COUNT).Students(K1(K1.COUNT).Students.COUNT) := Student_obj(
                student.StudentId,
                student.Name,
                student.GroupName
            );
        END LOOP;
    END LOOP;

    -- a. Проверка на пересечение студентов по имени
    FOR i IN 1..K1.COUNT LOOP           -- Цикл по университетам K1
        FOR j IN i+1..K1.COUNT LOOP     -- Вложенный цикл для других университетов
            FOR k IN 1..K1(i).Students.COUNT LOOP    -- Цикл по студентам первого университета
                FOR m IN 1..K1(j).Students.COUNT LOOP  -- Цикл по студентам второго университета
                    IF K1(i).Students(k).Name = K1(j).Students(m).Name THEN
                        intersection_found := TRUE;
                        DBMS_OUTPUT.PUT_LINE(
                            'Пересечение найдено: Университет ' || K1(i).UniversityName || 
                            ' и Университет ' || K1(j).UniversityName || 
                            ' оба имеют студента с именем ' || K1(i).Students(k).Name
                        );
                    END IF;
                END LOOP;
            END LOOP;
        END LOOP;
    END LOOP;

    IF NOT intersection_found THEN
        DBMS_OUTPUT.PUT_LINE('Пересечений не найдено.');
    END IF;

    -- c. Выяснение, является ли членом коллекции K1 какой-то произвольный элемент
    FOR i IN 1..K1.COUNT LOOP
        IF K1(i).UniversityId = 2 THEN  -- Пример поиска университета с ID 2
            is_member := TRUE;
            DBMS_OUTPUT.PUT_LINE('Университет с ID 2 найден в коллекции K1: ' || K1(i).UniversityName);
            EXIT;
        END IF;
    END LOOP;

    IF NOT is_member THEN
        DBMS_OUTPUT.PUT_LINE('Университет с ID 2 отсутствует в коллекции K1.');
    END IF;

    -- d. Поиск пустых коллекций K1 (университетов без студентов)
    FOR i IN 1..K1.COUNT LOOP
        IF K1(i).Students.COUNT = 0 THEN
            has_empty_collection := TRUE;
            DBMS_OUTPUT.PUT_LINE('Университет ' || K1(i).UniversityName || ' не имеет студентов.');
        END IF;
    END LOOP;

    IF NOT has_empty_collection THEN
        DBMS_OUTPUT.PUT_LINE('Все университеты имеют студентов.');
    END IF;

    -- e. Обмен атрибутами студентов между двумя университетами
    DBMS_OUTPUT.PUT_LINE('До обмена:');
    DBMS_OUTPUT.PUT_LINE('Университет ' || K1(cl1).UniversityName || ' имеет студентов:');
    FOR i IN 1..K1(cl1).Students.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE('- ' || K1(cl1).Students(i).Name);
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Университет ' || K1(cl2).UniversityName || ' имеет студентов:');
    FOR i IN 1..K1(cl2).Students.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE('- ' || K1(cl2).Students(i).Name);
    END LOOP;

    DECLARE
        temp_students StudentCollection;
    BEGIN
        temp_students := K1(cl1).Students;
        K1(cl1).Students := K1(cl2).Students;
        K1(cl2).Students := temp_students;
    END;

    DBMS_OUTPUT.PUT_LINE('После обмена:');
    DBMS_OUTPUT.PUT_LINE('Университет ' || K1(cl1).UniversityName || ' теперь имеет студентов:');
    FOR i IN 1..K1(cl1).Students.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE('- ' || K1(cl1).Students(i).Name);
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('Университет ' || K1(cl2).UniversityName || ' теперь имеет студентов:');
    FOR i IN 1..K1(cl2).Students.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE('- ' || K1(cl2).Students(i).Name);
    END LOOP;
END;

INSERT INTO t2 VALUES (6, 'Иванов Иван', 'Группа 3', 3);
INSERT INTO t1 VALUES (4, 'БГУ', 'Минск');


-- 3.Преобразовать коллекцию к другому виду (к коллекции другого типа, к реляционным данным).
CREATE TYPE TeacherArray AS VARRAY(5) OF VARCHAR2(100);

CREATE TYPE TeacherTable AS TABLE OF VARCHAR2(100);

DECLARE
    teacher_array TeacherArray := TeacherArray();
    teacher_table TeacherTable := TeacherTable();
BEGIN
    teacher_array.EXTEND(5);
    teacher_array(1) := 'Иванов Петр';
    teacher_array(2) := 'Смирнова Анна';
    teacher_array(3) := 'Козлов Михаил';
    teacher_array(4) := 'Федорова Ольга';
    teacher_array(5) := 'Васильев Дмитрий';

    FOR r IN (SELECT COLUMN_VALUE AS Full_Name FROM TABLE(CAST(teacher_array AS TeacherArray))) LOOP
        DBMS_OUTPUT.PUT_LINE('Преподаватель: ' || r.Full_Name);
    END LOOP;
END;


-- 4.Продемонстрировать применение BULK операций на примере своих коллекций.
DECLARE
    K2 StudentCollection := StudentCollection();
BEGIN
    K2.EXTEND(3);
    K2(1) := Student_obj(10, 'Иван Иванов', 'Группа A');
    K2(2) := Student_obj(11, 'Петр Петров', 'Группа B');
    K2(3) := Student_obj(12, 'Сергей Сергеев', 'Группа C');

    FORALL j IN 1..K2.COUNT
        INSERT INTO t2 (StudentId, Name, GroupName)
        VALUES (K2(j).StudentId, K2(j).Name, K2(j).GroupName);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Массовая вставка студентов завершена.');
END;
SELECT * FROM t2;


DECLARE
    K2 StudentCollection;
BEGIN
    SELECT Student_obj(StudentId, Name, GroupName)
    BULK COLLECT INTO K2
    FROM t2;

    DBMS_OUTPUT.PUT_LINE('Загружено студентов: ' || K2.COUNT);
    FOR i IN 1..K2.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE('ID: ' || K2(i).StudentId || 
                             ', Имя: ' || K2(i).Name || 
                             ', Группа: ' || K2(i).GroupName);
    END LOOP;
END;


