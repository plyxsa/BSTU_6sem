CREATE DATABASE REPLICATION_DB;

USE REPLICATION_DB;

-- �������� ������� �������
CREATE TABLE Departments (
    DepartmentID INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(100) NOT NULL
);

-- �������� ������� �����������
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DepartmentID INT FOREIGN KEY REFERENCES Departments(DepartmentID),
    HireDate DATE NOT NULL,
    Salary DECIMAL(18, 2) NOT NULL
);

-- ������� ������ � ������� �������
INSERT INTO Departments (DepartmentName)
VALUES 
('HR'),
('IT'),
('Finance'),
('Marketing');

-- ������� ������ � ������� �����������
INSERT INTO Employees (FirstName, LastName, DepartmentID, HireDate, Salary)
VALUES 
('John', 'Doe', 1, '2020-01-15', 50000.00),
('Jane', 'Smith', 2, '2019-03-22', 60000.00),
('Alice', 'Johnson', 3, '2021-07-10', 55000.00),
('Bob', 'Brown', 2, '2018-11-05', 70000.00);

CREATE PROCEDURE AddEmployee
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @DepartmentID INT,
    @HireDate DATE,
    @Salary DECIMAL(18, 2)
AS
BEGIN
    INSERT INTO Employees (FirstName, LastName, DepartmentID, HireDate, Salary)
    VALUES (@FirstName, @LastName, @DepartmentID, @HireDate, @Salary);
END;

CREATE FUNCTION GetEmployeeCountByDepartment(@DepartmentID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*) 
    FROM Employees 
    WHERE DepartmentID = @DepartmentID;
    RETURN @Count;
END;

CREATE TRIGGER trg_SetHireDate
ON Employees
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Employees (FirstName, LastName, DepartmentID, HireDate, Salary)
    SELECT 
        FirstName, 
        LastName, 
        DepartmentID, 
        ISNULL(HireDate, GETDATE()), 
        Salary
    FROM inserted;
END;

-- ������������� ��������� ��� ���������� ������ ����������
EXEC AddEmployee 'Mike', 'Taylor', 4, '2023-01-01', 65000.00;

-- ������������� ������� ��� ��������� ���������� ����������� � ������
SELECT d.DepartmentName, dbo.GetEmployeeCountByDepartment(d.DepartmentID) AS EmployeeCount
FROM Departments d;

-- �������� ��������
INSERT INTO Employees (FirstName, LastName, DepartmentID, Salary)
VALUES ('Emily', 'Davis', 1, 48000.00);

-- �������� ������
SELECT * FROM Employees;


SELECT SERVERPROPERTY('Collation');

EXEC sp_configure 'clr enabled', 0;
RECONFIGURE;
EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;