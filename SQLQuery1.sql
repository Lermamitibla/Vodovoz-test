CREATE DATABASE VodovozTEST;
go

USE VodovozTEST;

CREATE TABLE dbo.Departments (
depID INT not null identity(10,10) Primary key,
depName VARCHAR(30) not null,
managerID INT null
);


CREATE TABLE dbo.Employees (
empid INT not null identity(1,1) Primary key,
firstname VARCHAR(30) not null,
lastname VARCHAR(30) not null,
patronymic VARCHAR(30) null,
gender tinyint not null check(gender = 1 or gender = 2),
dateOfBirth DateTime not null,
depID INT not null Foreign key References dbo.Departments(depID),
);



Alter table dbo.Departments 
Add constraint Manager_foreign_key
foreign key (managerID)
references dbo.Employees(empid);
