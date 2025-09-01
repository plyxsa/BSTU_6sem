CREATE LOGIN AdminUser WITH PASSWORD = 'AdminUser';

USE University;
USE PlushFood;
CREATE USER AdminUser FOR LOGIN AdminUser;
ALTER ROLE db_owner ADD MEMBER AdminUser;


