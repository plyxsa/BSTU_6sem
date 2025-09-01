CREATE DATABASE LINK VAV_db 
   CONNECT TO VAV
   IDENTIFIED BY "Password123"
   USING '26.69.212.139:1521/PDBORCL';
   
SELECT * FROM employees@VAV_db;

CREATE TABLE BUNS (
    ID NUMBER PRIMARY KEY,
    NAME VARCHAR2(100),
    DESCRIPTION VARCHAR2(255)
);

delete from BUNS;

INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (1, 'Булочка с маком', 'Пышная булочка с ароматным маком');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (2, 'Булочка с корицей', 'Ароматная булочка с корицей и сахарной глазурью');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (3, 'Булочка с изюмом', 'Мягкая булочка с изюмом и ванильным ароматом');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (7, 'Булочка с сахаром', 'НЯМ');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (11, 'Булочка с тестом', 'Двойная булка?');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (12, 'Булочка с углями', 'Кто забыл булку в печке?');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (13, 'Булочка с мясом', 'Очень питательно');

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (14, 'Булочка с яблоком', 'Почему в булочке целое яблоко?');

COMMIT;

SELECT * FROM BUNS FOR UPDATE;

----
-- Распределенные транзакции

-- INSERT/INSERT

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN
    INSERT INTO buns (id, name, description) VALUES (4, 'Булочка на пару', 'Эта булочка спасла мою семью в 2012 году.');
    INSERT INTO employees@VAV_db (id, name, salary) VALUES (3, 'Alice', 5000);

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Ошибка: ' || SQLERRM);
END;

SELECT * FROM buns;
SELECT * FROM employees@VAV_db;


-- INSERT/UPDATE

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN
    INSERT INTO  buns (id, name, description) VALUES (5, 'Булочка с бриллиантами', 'Она так прекрасно блестит.');
    UPDATE employees@VAV_db SET name = 'Обновлённые данные' WHERE id = 1;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Ошибка: ' || SQLERRM);
END;

SELECT * FROM buns;
SELECT * FROM employees@VAV_db;

-- UPDATE/INSERT

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN
    UPDATE  buns SET description = 'Обновлённые данные' WHERE id = 1;
    INSERT INTO employees@VAV_db (id, name, salary) VALUES (6, 'Alice', 5000);

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Ошибка: ' || SQLERRM);
END;

SELECT * FROM buns;
SELECT * FROM employees@VAV_db;

---

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN
    UPDATE  buns SET description = 'Обновлённые данные 2' WHERE id = 1;
    INSERT INTO employees@VAV_db (id, name, salary) VALUES (1, 'Alice', 5000);

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Ошибка: ' || SQLERRM);
END;

SELECT * FROM buns;
SELECT * FROM employees@VAV_db;

----
LOCK TABLE employees@LPV_db IN SHARE MODE;
----

SELECT * FROM employees@VAV_db WHERE id = 5 FOR UPDATE;
COMMIT;

UPDATE employees@VAV_db SET name = 'Bib' WHERE id = 5;

DELETE FROM employees@VAV_db WHERE id = 5;

INSERT INTO employees@VAV_db (id, name, salary) VALUES (99, 'Polina', 500000);

ROLLBACK;
