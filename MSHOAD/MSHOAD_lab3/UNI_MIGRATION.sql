CREATE TABLE Students (
    Student_ID NUMBER PRIMARY KEY,
    First_Name VARCHAR2(50) NOT NULL,
    Last_Name VARCHAR2(50) NOT NULL,
    Birth_Date DATE NOT NULL,
    Faculty VARCHAR2(100) NOT NULL
);

-- Создание последовательности для генерации ID
CREATE SEQUENCE Students_Seq START WITH 1 INCREMENT BY 1;

-- Заполнение таблицы начальными данными
INSERT INTO Students (Student_ID, First_Name, Last_Name, Birth_Date, Faculty) 
VALUES (Students_Seq.NEXTVAL, 'Иван', 'Петров', TO_DATE('2003-05-15', 'YYYY-MM-DD'), 'Информатика');

INSERT INTO Students (Student_ID, First_Name, Last_Name, Birth_Date, Faculty) 
VALUES (Students_Seq.NEXTVAL, 'Анна', 'Сидорова', TO_DATE('2002-08-22', 'YYYY-MM-DD'), 'Экономика');

INSERT INTO Students (Student_ID, First_Name, Last_Name, Birth_Date, Faculty) 
VALUES (Students_Seq.NEXTVAL, 'Петр', 'Иванов', TO_DATE('2004-01-10', 'YYYY-MM-DD'), 'Физика');

COMMIT;

CREATE OR REPLACE PROCEDURE Add_Student(
    p_First_Name IN VARCHAR2,
    p_Last_Name IN VARCHAR2,
    p_Birth_Date IN DATE,
    p_Faculty IN VARCHAR2
)
IS
BEGIN
    INSERT INTO Students (First_Name, Last_Name, Birth_Date, Faculty)
    VALUES (p_First_Name, p_Last_Name, p_Birth_Date, p_Faculty);
    COMMIT;
END;


CREATE OR REPLACE TRIGGER Check_Student_Age
BEFORE INSERT OR UPDATE ON Students
FOR EACH ROW
DECLARE
    min_age NUMBER := 16;
BEGIN
    IF (MONTHS_BETWEEN(SYSDATE, :NEW.Birth_Date) / 12) < min_age THEN
        RAISE_APPLICATION_ERROR(-20001, 'Возраст студента должен быть не менее 16 лет.');
    END IF;
END;



CREATE OR REPLACE FUNCTION Get_Avg_Student_Age RETURN NUMBER IS
    avg_age NUMBER;
BEGIN
    SELECT AVG(MONTHS_BETWEEN(SYSDATE, Birth_Date) / 12) INTO avg_age FROM Students;
    RETURN avg_age;
END;


SELECT Get_Avg_Student_Age FROM dual;

SELECT * FROM Students;
