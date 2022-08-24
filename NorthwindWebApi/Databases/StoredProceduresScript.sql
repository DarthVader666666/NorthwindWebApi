GO
CREATE PROCEDURE [dbo].[CurrentProducts]
AS SELECT ProductName, UnitPrice FROM [dbo].[Products] ORDER BY ProductName

GO
CREATE PROCEDURE [dbo].[CurrentProductsWithLocalCurrency]
AS SELECT Products.ProductName, Products.UnitPrice, Suppliers.SupplierID, Suppliers.Country
FROM Products
INNER JOIN Suppliers ON Suppliers.SupplierID=Products.SupplierID

GO
CREATE PROCEDURE [dbo].[MostExpensiveProducts]
@count int
AS SELECT TOP(@count) ProductName, UnitPrice FROM Products ORDER BY UnitPrice DESC

GO
CREATE PROCEDURE [dbo].[PriceLessThenProducts]
@price decimal
AS SELECT ProductName, UnitPrice FROM Products WHERE UnitPrice < @price ORDER BY UnitPrice ASC