USE LPV_DB;
GO

-- 1. �������� ������� FACULTY
CREATE TABLE FACULTY (
    FACULTY NVARCHAR(10) PRIMARY KEY,
    FACULTY_NAME NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- 2. �������� ������� PULPIT (�������)
CREATE TABLE PULPIT (
    PULPIT NVARCHAR(20) PRIMARY KEY,
    PULPIT_NAME NVARCHAR(100) NOT NULL,
    FACULTY NVARCHAR(10) NOT NULL,
    FOREIGN KEY (FACULTY) REFERENCES FACULTY(FACULTY) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 3. �������� ������� TEACHER (�������������)
CREATE TABLE TEACHER (
    TEACHER NVARCHAR(20) PRIMARY KEY,
    TEACHER_NAME NVARCHAR(100) NOT NULL,
    PULPIT NVARCHAR(20) NOT NULL,
    FOREIGN KEY (PULPIT) REFERENCES PULPIT(PULPIT) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 4. �������� ������� SUBJECT (����������)
CREATE TABLE SUBJECT (
    SUBJECT NVARCHAR(20) PRIMARY KEY,
    SUBJECT_NAME NVARCHAR(100) NOT NULL,
    PULPIT NVARCHAR(20) NOT NULL,
    FOREIGN KEY (PULPIT) REFERENCES PULPIT(PULPIT) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- 5. �������� ������� AUDITORIUM_TYPE (��� ���������)
CREATE TABLE AUDITORIUM_TYPE (
    AUDITORIUM_TYPE NVARCHAR(10) PRIMARY KEY,
    AUDITORIUM_TYPENAME NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 6. �������� ������� AUDITORIUM (���������)
CREATE TABLE AUDITORIUM (
    AUDITORIUM NVARCHAR(20) PRIMARY KEY,
    AUDITORIUM_NAME NVARCHAR(50),
    AUDITORIUM_CAPACITY INT NOT NULL CHECK (AUDITORIUM_CAPACITY > 0),
    AUDITORIUM_TYPE NVARCHAR(10) NOT NULL,
    FOREIGN KEY (AUDITORIUM_TYPE) REFERENCES AUDITORIUM_TYPE(AUDITORIUM_TYPE) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- --- ���������� ������ ������� ---

-- ���������� FACULTY
INSERT INTO FACULTY (FACULTY, FACULTY_NAME) VALUES
('����', N'������������ ���� � ����������'),
('����', N'���������� ���������� � �������'),
('���', N'����������������� ���������'),
('���', N'���������-������������� ���������'),
('���', N'���������� ������������ �������'),
('����', N'���������� � ������� ������ ��������������'),
('��', N'�������������� ����������');
GO

-- ���������� AUDITORIUM_TYPE
INSERT INTO AUDITORIUM_TYPE (AUDITORIUM_TYPE, AUDITORIUM_TYPENAME) VALUES
('��', N'����������'),
('��-�', N'������������ �����'),
('��-�', N'���������� �����������'),
('��-�', N'���������� � ������������');
GO

-- ���������� PULPIT
INSERT INTO PULPIT (PULPIT, PULPIT_NAME, FACULTY) VALUES
('����', N'�������������� ������ � ����������', '��'),
('������', N'���������������� ������������ � ������ ��������� ����������', '����'),
('��', N'�����������', '���'),
('����', N'������������� ������ � ����������', '���'),
('��������', N'���������� ���������������� ������� � ����������� ���������� ����������', '����'),
('��', N'����������� ���������', '��');
GO


-- ���������� TEACHER
INSERT INTO TEACHER (TEACHER, TEACHER_NAME, PULPIT) VALUES
('����', N'������ �������� �������������', '����'),
('�����', N'�������� ������ ��������', '����'),
('���', N'����� ������� ��������', '����'),
('���', N'��������� ����� ��������', '������'),
('���', N'������ ���������� ������������', '��'),
('����', N'��������� �������� ��������', '����');
GO

-- ���������� SUBJECT
INSERT INTO SUBJECT (SUBJECT, SUBJECT_NAME, PULPIT) VALUES
('��', N'���� ������', '����'),
('����', N'������ �������������� � ����������������', '����'),
('���', N'���������������� ������� ����������', '����'),
('��', N'������������ �������', '������'),
('��', N'��������� ������������������', '����'),
('���', N'����������', '��'),
('���', N'��������-��������������� ����������������', '��');
GO

-- ���������� AUDITORIUM
INSERT INTO AUDITORIUM (AUDITORIUM, AUDITORIUM_NAME, AUDITORIUM_CAPACITY, AUDITORIUM_TYPE) VALUES
('201-4', N'201-4', 30, '��-�'),
('202-4', N'202-4', 30, '��-�'),
('301-4', N'301-4', 60, '��'),
('401-4', N'401-4', 90, '��'),
('101-1', N'101-1', 40, '��-�'),
('102-1', N'102-1', 40, '��-�'),
('324-4', N'324-4', 50, '��'),
('110-4', N'110-4', 25, '��-�');
GO