-- LegacyOrderMgmt Database Creation Script
-- Run against: (localdb)\MSSQLLocalDB or SQL Server Express

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'LegacyOrderMgmtDb')
    DROP DATABASE LegacyOrderMgmtDb;
GO

CREATE DATABASE LegacyOrderMgmtDb;
GO

USE LegacyOrderMgmtDb;
GO

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerCode NVARCHAR(20) NOT NULL UNIQUE,
    CompanyName NVARCHAR(200) NOT NULL,
    ContactName NVARCHAR(100),
    Email NVARCHAR(200),
    Phone NVARCHAR(50),
    BillingAddress NVARCHAR(300),
    BillingCity NVARCHAR(100),
    BillingCountry NVARCHAR(100),
    BillingPostalCode NVARCHAR(20),
    CreditLimit NVARCHAR(50),       -- Stored as string: legacy design flaw
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    UnitPrice DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20),
    CategoryId INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PricingRules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RuleName NVARCHAR(100) NOT NULL,
    CustomerTier NVARCHAR(20),
    ProductCategoryId INT,
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    MinOrderValue DECIMAL(18,2),
    ValidFrom DATETIME NOT NULL,
    ValidTo DATETIME,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(30) NOT NULL UNIQUE,
    CustomerId INT NOT NULL REFERENCES Customers(Id),
    OrderDate DATETIME NOT NULL,
    RequiredDate DATETIME,
    ShippedDate DATETIME,
    Status INT NOT NULL DEFAULT 0,  -- 0=Draft,1=Confirmed,2=Processing,3=Shipped,4=Invoiced,5=Cancelled
    ShippingAddress NVARCHAR(300),
    ShippingCity NVARCHAR(100),
    ShippingCountry NVARCHAR(100),
    ShippingPostalCode NVARCHAR(20),
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(1000),
    CreatedBy NVARCHAR(100),
    LastConfirmedBy NVARCHAR(100),  -- Updated by raw SQL in OrderService.ConfirmOrder
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE OrderLines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
    ProductId INT NOT NULL REFERENCES Products(Id),
    ProductCode NVARCHAR(50),
    ProductName NVARCHAR(200),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(18,2) NOT NULL,
    LineStatus INT NOT NULL DEFAULT 0,
    Notes NVARCHAR(500)
);

CREATE TABLE Invoices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL REFERENCES Orders(Id),
    InvoiceNumber NVARCHAR(30) NOT NULL UNIQUE,
    InvoiceDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    PaidDate DATETIME,
    PaidBy NVARCHAR(100),
    Notes NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Shipments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL REFERENCES Orders(Id),
    TrackingNumber NVARCHAR(100),
    Carrier NVARCHAR(100),
    ShipDate DATETIME NOT NULL,
    EstimatedDelivery DATETIME,
    ActualDelivery DATETIME,
    ShipmentStatus INT NOT NULL DEFAULT 0,  -- 0=Pending,1=InTransit,2=Delivered,3=Failed
    ShippingAddress NVARCHAR(300),
    Notes NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

PRINT 'LegacyOrderMgmtDb schema created successfully.';
