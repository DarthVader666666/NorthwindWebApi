go

CREATE PROCEDURE [dbo].[InsertEmployee]
	@lastName nvarchar(10),
	@firstName nvarchar(20),
	@title nvarchar(30),
	@titleOfCourtesy nvarchar(25),
	@birthDate datetime2,
	@hireDate datetime2,
	@address nvarchar(60),
	@city nvarchar(15),
	@region nvarchar(15),
	@postalCode nvarchar(10),
	@country nvarchar(15),
	@homePhone nvarchar(24),
	@extension nvarchar(4),
	@photo image,
	@notes ntext,
	@reportsTo int,
	@photoPath nvarchar(255)

AS	INSERT INTO dbo.Employees (
                LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate, Address, City,
                Region, PostalCode, Country, HomePhone, Extension, Photo, Notes, ReportsTo, PhotoPath)
                OUTPUT Inserted.EmployeeID
                VALUES (
                @lastName, @firstName, @title, @titleOfCourtesy, @birthDate, @hireDate, @address, @city,
                @region, @postalCode, @country, @homePhone, @extension, @photo, @notes, @reportsTo, @photoPath)
				
go

create procedure [dbo].[DeleteEmployee]
@employeeID int

AS DELETE FROM dbo.Employees WHERE EmployeeID = @employeeID
SELECT @@ROWCOUNT

go

create procedure [dbo].[FindEmployee]
@employeeId int

AS SELECT p.EmployeeID, p.LastName, p.FirstName, p.Title, p.TitleOfCourtesy, p.BirthDate,
p.HireDate, p.Address, p.City, p.Region, p.PostalCode, p.Country, p.HomePhone, p.Extension,
p.Photo, p.Notes, p.ReportsTo, p.PhotoPath
FROM dbo.Employees as p
WHERE p.EmployeeID = @employeeId

go

create procedure [dbo].[SelectEmployees]
@offset int,
@limit int

AS SELECT p.EmployeeID, p.LastName, p.FirstName, p.Title, p.TitleOfCourtesy, p.BirthDate,
p.HireDate, p.Address, p.City, p.Region, p.PostalCode, p.Country, p.HomePhone, p.Extension,
p.Photo, p.Notes, p.ReportsTo, p.PhotoPath
FROM dbo.Employees as p
ORDER BY p.EmployeeID
OFFSET @offset ROWS
FETCH FIRST @limit ROWS ONLY

go

create procedure [dbo].[SelectEmployeesByName]
@names nvarchar(100)

AS SELECT p.EmployeeID, p.LastName, p.FirstName, p.Title, p.TitleOfCourtesy, p.BirthDate,
p.HireDate, p.Address, p.City, p.Region, p.PostalCode, p.Country, p.HomePhone, p.Extension,
p.Photo, p.Notes, p.ReportsTo, p.PhotoPath
FROM dbo.Employees as p
WHERE p.LastName in (@names)
ORDER BY p.EmployeeID

go

create procedure [dbo].[UpdateEmployee]
    @employeeId int,
	@lastName nvarchar(10),
	@firstName nvarchar(20),
	@title nvarchar(30),
	@titleOfCourtesy nvarchar(25),
	@birthDate datetime2,
	@hireDate datetime2,
	@address nvarchar(60),
	@city nvarchar(15),
	@region nvarchar(15),
	@postalCode nvarchar(10),
	@country nvarchar(15),
	@homePhone nvarchar(24),
	@extension nvarchar(4),
	@photo image,
	@notes ntext,
	@reportsTo int,
	@photoPath nvarchar(255)

AS UPDATE dbo.Employees
SET
LastName=@lastName,FirstName=@firstName,Title=@title,TitleOfCourtesy=@titleOfCourtesy,BirthDate=@birthDate,
HireDate=@hireDate,Address=@address,City=@city,Region=@region,PostalCode=@postalCode,Country=@country,
HomePhone=@homePhone,Extension=@extension,Photo=@photo,Notes=@notes,ReportsTo=@reportsTo,PhotoPath=@photoPath
WHERE EmployeeID = @employeeId
SELECT @@ROWCOUNT