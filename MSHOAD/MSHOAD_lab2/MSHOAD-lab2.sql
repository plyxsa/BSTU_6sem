use University;
sp_configure 'clr enabled', 1;
RECONFIGURE;

ALTER DATABASE University SET TRUSTWORTHY ON;

DROP PROCEDURE IF EXISTS WriteToFile;
DROP ASSEMBLY IF EXISTS UniversityCLR;

CREATE ASSEMBLY UniversityCLR
FROM 'D:\Uni\MSHOAD\MSHOAD-lab2\UniversityCLR\UniversityCLR\bin\Release\UniversityCLR.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS;

go
CREATE PROCEDURE dbo.WriteToFile
    @content NVARCHAR(MAX),
    @filePath NVARCHAR(255)
AS EXTERNAL NAME UniversityCLR.[UniversityCLR.FileOperations].WriteToFile;

go
EXEC dbo.WriteToFile @content = N'привет', @filePath = N'D:\Uni\MSHOAD\MSHOAD-lab2\File.txt';


CREATE TYPE dbo.PhoneNumber EXTERNAL NAME UniversityCLR.[UniversityCLR.PhoneNumber];


CREATE TABLE dbo.ContactInfo (
    ContactID INT PRIMARY KEY,
    PhoneNumber dbo.PhoneNumber
);


INSERT INTO dbo.ContactInfo (ContactID, PhoneNumber)
VALUES (1, '+375 (29) 123-45-67');
INSERT INTO dbo.ContactInfo (ContactID, PhoneNumber)
VALUES (2, '+375 (44) 123-45-67');
INSERT INTO dbo.ContactInfo (ContactID, PhoneNumber)
VALUES (3, '+375 (33) 123-45-67');

DELETE FROM dbo.ContactInfo;

SELECT 
    ContactID,
    PhoneNumber.ToString() AS PhoneNumberFormatted
FROM 
    dbo.ContactInfo;


INSERT INTO dbo.ContactInfo (ContactID, PhoneNumber)
VALUES (5, '+345 (29) 123-45-78');
