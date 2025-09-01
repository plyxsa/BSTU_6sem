CREATE TABLE BUNS (
    ID NUMBER PRIMARY KEY,
    NAME VARCHAR2(100),
    DESCRIPTION VARCHAR2(255)
);

INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (1, 'Булочка с маком', 'Пышная булочка с ароматным маком');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (2, 'Булочка с корицей', 'Ароматная булочка с корицей и сахарной глазурью');
INSERT INTO BUNS (ID, NAME, DESCRIPTION) VALUES (3, 'Булочка с изюмом', 'Мягкая булочка с изюмом и ванильным ароматом');
COMMIT;

SELECT * FROM BUNS;