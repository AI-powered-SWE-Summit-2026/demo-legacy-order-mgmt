USE LegacyOrderMgmtDb;
GO

-- Customers
INSERT INTO Customers (CustomerCode, CompanyName, ContactName, Email, Phone, BillingAddress, BillingCity, BillingCountry, BillingPostalCode, CreditLimit, IsActive)
VALUES
('AERO-001', 'Aerotech Dynamics Ltd', 'Sophie Laurent', 'sophie.laurent@aerotech.example.com', '+44 20 7946 0100', '12 Aviation Way', 'London', 'UK', 'SW1A 1AA', 'Gold', 1),
('AUTO-002', 'Kronfeld Automotive GmbH', 'Hans Becker', 'h.becker@kronfeld.example.com', '+49 89 1234 5678', 'Industriestrasse 45', 'Munich', 'Germany', '80939', 'Platinum', 1),
('ENER-003', 'NordPower Energy AS', 'Erik Solberg', 'e.solberg@nordpower.example.com', '+47 22 12 3456', 'Energiveien 8', 'Oslo', 'Norway', '0283', 'Silver', 1),
('MFG-004', 'Castellan Manufacturing Inc', 'Maria Reyes', 'm.reyes@castellan.example.com', '+1 312 555 0199', '800 Industrial Blvd', 'Chicago', 'USA', '60601', 'Gold', 1),
('DIST-005', 'Halcyon Distribution SRL', 'Luca Moretti', 'l.moretti@halcyon.example.com', '+39 02 1234 5678', 'Via della Industria 22', 'Milan', 'Italy', '20100', 'Standard', 1);

-- Products
INSERT INTO Products (ProductCode, Name, Description, UnitPrice, UnitOfMeasure, CategoryId, IsActive)
VALUES
('COMP-101', 'Precision Bearing Assembly', 'High-tolerance bearing assembly for aerospace applications', 245.00, 'EA', 1, 1),
('COMP-102', 'Hydraulic Seal Kit', 'Industrial hydraulic seal set, 12-piece', 89.50, 'KIT', 1, 1),
('COMP-103', 'Titanium Fastener Set', 'Grade 5 titanium bolts and nuts, 50-piece', 312.00, 'SET', 2, 1),
('COMP-104', 'Control Valve Assembly', 'Pneumatic control valve, 2-inch, stainless steel', 560.00, 'EA', 3, 1),
('COMP-105', 'Sensor Module XT-200', 'Industrial proximity sensor with 10m cable', 175.00, 'EA', 3, 1),
('COMP-106', 'Aluminium Extrusion Profile', '6063-T5 aluminium, 2m length, 40x40mm', 42.00, 'M', 2, 1),
('COMP-107', 'Electric Motor Drive 5kW', 'Variable frequency drive for 5kW motors', 890.00, 'EA', 4, 1),
('COMP-108', 'Gasket Set - Standard', 'EPDM gasket set for pipework, 20-piece', 34.00, 'SET', 1, 1);

-- Pricing Rules
INSERT INTO PricingRules (RuleName, CustomerTier, DiscountPercent, MinOrderValue, ValidFrom, IsActive)
VALUES
('Standard No Discount', 'Standard', 0.00, NULL, '2020-01-01', 1),
('Silver 5% Discount', 'Silver', 5.00, NULL, '2020-01-01', 1),
('Gold 10% Discount', 'Gold', 10.00, NULL, '2020-01-01', 1),
('Platinum 15% Discount', 'Platinum', 15.00, NULL, '2020-01-01', 1),
('Gold Bulk Order Bonus', 'Gold', 12.00, 5000.00, '2023-01-01', 1),
('Platinum Bulk Order Bonus', 'Platinum', 18.00, 5000.00, '2023-01-01', 1);

-- Orders
INSERT INTO Orders (OrderNumber, CustomerId, OrderDate, RequiredDate, Status, ShippingAddress, ShippingCity, ShippingCountry, ShippingPostalCode, SubTotal, DiscountAmount, TaxAmount, TotalAmount, CreatedBy, CreatedAt, UpdatedAt)
VALUES
('ORD-2025-00001', 1, '2025-03-10', '2025-03-20', 4, '12 Aviation Way', 'London', 'UK', 'SW1A 1AA', 1960.00, 196.00, 352.80, 2116.80, 'salesrep', '2025-03-10', '2025-03-12'),
('ORD-2025-00002', 2, '2025-03-15', '2025-03-25', 3, 'Industriestrasse 45', 'Munich', 'Germany', '80939', 7120.00, 1068.00, 1210.40, 7262.40, 'salesrep', '2025-03-15', '2025-03-17'),
('ORD-2025-00003', 3, '2025-04-02', '2025-04-15', 2, 'Energiveien 8', 'Oslo', 'Norway', '0283', 875.00, 43.75, 166.25, 997.50, 'system', '2025-04-02', '2025-04-02'),
('ORD-2025-00004', 4, '2025-04-20', '2025-04-30', 1, '800 Industrial Blvd', 'Chicago', 'USA', '60601', 3120.00, 312.00, 561.60, 3369.60, 'salesrep', '2025-04-20', '2025-04-20'),
('ORD-2025-00005', 1, '2025-05-05', '2025-05-15', 0, '12 Aviation Way', 'London', 'UK', 'SW1A 1AA', 0.00, 0.00, 0.00, 0.00, 'salesrep', '2025-05-05', '2025-05-05');

-- Order Lines
INSERT INTO OrderLines (OrderId, ProductId, ProductCode, ProductName, Quantity, UnitPrice, DiscountPercent, LineTotal, LineStatus)
VALUES
(1, 1, 'COMP-101', 'Precision Bearing Assembly', 4, 245.00, 10.00, 882.00, 4),
(1, 3, 'COMP-103', 'Titanium Fastener Set', 2, 312.00, 10.00, 561.60, 4),
(1, 5, 'COMP-105', 'Sensor Module XT-200', 3, 175.00, 10.00, 472.50, 4),
(2, 7, 'COMP-107', 'Electric Motor Drive 5kW', 5, 890.00, 15.00, 3782.50, 3),
(2, 4, 'COMP-104', 'Control Valve Assembly', 6, 560.00, 15.00, 2856.00, 3),
(3, 2, 'COMP-102', 'Hydraulic Seal Kit', 5, 89.50, 5.00, 424.63, 2),
(3, 8, 'COMP-108', 'Gasket Set - Standard', 10, 34.00, 5.00, 323.00, 2),
(4, 1, 'COMP-101', 'Precision Bearing Assembly', 8, 245.00, 10.00, 1764.00, 1),
(4, 5, 'COMP-105', 'Sensor Module XT-200', 8, 175.00, 10.00, 1260.00, 1);

-- Invoices
INSERT INTO Invoices (OrderId, InvoiceNumber, InvoiceDate, DueDate, Amount, TaxAmount, TotalAmount, PaymentStatus, PaidDate, CreatedAt)
VALUES
(1, 'INV-2025-000001', '2025-03-12', '2025-04-11', 1764.00, 352.80, 2116.80, 'Paid', '2025-04-08', '2025-03-12'),
(2, 'INV-2025-000002', '2025-03-17', '2025-04-16', 6052.00, 1210.40, 7262.40, 'Pending', NULL, '2025-03-17');

-- Shipments
INSERT INTO Shipments (OrderId, TrackingNumber, Carrier, ShipDate, EstimatedDelivery, ActualDelivery, ShipmentStatus, ShippingAddress, CreatedAt)
VALUES
(1, 'DHL-2025-AB1234', 'DHL', '2025-03-11', '2025-03-14', '2025-03-13', 2, '12 Aviation Way, London, UK', '2025-03-11'),
(2, 'FED-2025-XY9876', 'FedEx', '2025-03-16', '2025-03-19', NULL, 1, 'Industriestrasse 45, Munich, Germany', '2025-03-16');
GO

PRINT 'LegacyOrderMgmtDb seed data loaded: 5 customers, 8 products, 6 pricing rules, 5 orders, 9 order lines, 2 invoices, 2 shipments.';
