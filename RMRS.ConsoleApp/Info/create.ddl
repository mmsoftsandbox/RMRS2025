CREATE TABLE dbo.Employees (
  EmployeeID INT IDENTITY
 ,FirstName NVARCHAR(50) NULL
 ,LastName NVARCHAR(50) NULL
 ,Email NVARCHAR(100) NULL
 ,DateOfBirth DATE NULL
 ,Salary DECIMAL NULL
 ,CONSTRAINT PK_Employees_EmployeeID PRIMARY KEY CLUSTERED (EmployeeID)
) ON [PRIMARY]
GO

CREATE INDEX IDX_Employees_DateOfBirth
ON dbo.Employees (DateOfBirth)
ON [PRIMARY]
GO

CREATE INDEX IDX_Employees_Email
ON dbo.Employees (Email)
ON [PRIMARY]
GO

CREATE INDEX IDX_Employees_FirstName
ON dbo.Employees (FirstName)
ON [PRIMARY]
GO

CREATE INDEX IDX_Employees_LastName
ON dbo.Employees (LastName)
ON [PRIMARY]
GO

CREATE INDEX IDX_Employees_Salary
ON dbo.Employees (Salary)
ON [PRIMARY]
GO