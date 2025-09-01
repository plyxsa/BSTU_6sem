use DataWarehouse;

CREATE TABLE Dim_Student (
    StudentID INT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Faculty NVARCHAR(50) NOT NULL,
    YearOfEnrollment INT NOT NULL
);

CREATE TABLE Dim_Course (
    CourseID INT PRIMARY KEY,
    CourseName NVARCHAR(100) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    Credits INT NOT NULL
);

CREATE TABLE Fact_Grades (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    CourseID INT NOT NULL,
    SemesterID INT NOT NULL,
    Grade DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (StudentID) REFERENCES Dim_Student(StudentID),
    FOREIGN KEY (CourseID) REFERENCES Dim_Course(CourseID)
);


INSERT INTO Dim_Student (StudentID, Name, Faculty, YearOfEnrollment) VALUES
(101, 'Иван Иванов', 'ФКТИ', 2020),
(102, 'Анна Смирнова', 'ФЭУ', 2019),
(103, 'Петр Сидоров', 'ФКТИ', 2021),
(104, 'Мария Петрова', 'ФЭУ', 2020),
(105, 'Алексей Козлов', 'ФКТИ', 2019),
(106, 'Елена Воробьева', 'ФЭУ', 2021),
(107, 'Дмитрий Соколов', 'ФКТИ', 2022);

INSERT INTO Dim_Course (CourseID, CourseName, Department, Credits) VALUES
(301, 'Алгебра', 'Математика', 3),
(302, 'Программирование', 'ФКТИ', 4),
(303, 'Базы данных', 'ФКТИ', 4),
(304, 'Экономика', 'ФЭУ', 3),
(305, 'Дискретная математика', 'Математика', 3),
(306, 'Веб-разработка', 'ФКТИ', 4);


INSERT INTO Fact_Grades (StudentID, CourseID, SemesterID, Grade) VALUES
(101, 301, 202301, 90),
(102, 302, 202301, 85),
(101, 301, 202301, 93),
(102, 302, 202301, 82),
(101, 302, 202302, 88), 
(102, 301, 202302, 78),  
(105, 304, 202301, 76),  
(106, 305, 202301, 89),  
(107, 306, 202301, 93); 

delete from Fact_Grades;
delete from Dim_Course;
delete from Dim_Student;

select * from Dim_Course;
select * from Dim_Student;
select * from Fact_Grades;

delete from Fact_Grades where CourseID=303;


SELECT * FROM Fact_Grades WHERE CourseID = 304;