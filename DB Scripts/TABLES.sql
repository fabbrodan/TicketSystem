-- Create database if not exists
IF NOT EXISTS (
SELECT 1 FROM sys.databases
WHERE name = 'TicketSystem')
BEGIN
CREATE DATABASE [TicketSystem];
END
GO

-- Set connection to use database
USE [TicketSystem];
GO

-- Drop all the foreign keys to avoid conflicts when setting up tables
BEGIN
declare @sql nvarchar(max);

SET @sql = (
SELECT 'ALTER TABLE ' + QUOTENAME(schema_name(schema_id)) + '.' + 
	QUOTENAME(object_name(parent_object_id)) +
	' DROP CONSTRAINT ' + QUOTENAME(name) + '; '
	FROM sys.foreign_keys
	FOR XML PATH(''));

EXEC(@sql);
END

-- Setup of Administrators table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Administrators')
BEGIN

SELECT * INTO #adminTmp FROM [Administrators];

DROP TABLE [Administrators];

CREATE TABLE [Administrators] (
[AdminId] int not null identity(1,1),
[LoginId] nvarchar(50) not null,
[Email] nvarchar(100) null,
[Password] nvarchar(max),
[PasswordSalt] nvarchar(max),
[RegisteredDate] datetime not null,
CONSTRAINT [PK_Administrators] PRIMARY KEY CLUSTERED ([AdminId]),
CONSTRAINT [UK1_Administrators] UNIQUE ([LoginId]),
CONSTRAINT [UK2_Administrators] UNIQUE ([Email]));

BEGIN
SET IDENTITY_INSERT [Administrators] ON;
INSERT INTO [Administrators]
(AdminId, LoginId, Email, Password, PasswordSalt, RegisteredDate)
SELECT AdminId, LoginId, Email, Password, PasswordSalt, RegisteredDate
FROM #adminTmp;
SET IDENTITY_INSERT [Administrators] OFF;
END

DROP TABLE #adminTmp;
END
ELSE
BEGIN
CREATE TABLE [Administrators] (
[AdminId] int not null identity(1,1),
[LoginId] nvarchar(50) not null,
[Email] nvarchar(100) null,
[Password] nvarchar(max),
[PasswordSalt] nvarchar(max),
[RegisteredDate] datetime not null,
CONSTRAINT [PK_Administrators] PRIMARY KEY CLUSTERED ([AdminId]),
CONSTRAINT [UK1_Administrators] UNIQUE ([LoginId]),
CONSTRAINT [UK2_Administrators] UNIQUE ([Email]));
END

-- Setup of Customers table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN

SELECT * INTO #custTmp FROM [Customers];

DROP TABLE [Customers];

CREATE TABLE [Customers] (
[CustomerId] int not null identity(1,1),
[LoginId] nvarchar(50) not null,
[Email] nvarchar(100) not null,
[PhoneNumber] nvarchar(15) null,
[Password] nvarchar(max) not null,
[PasswordSalt] nvarchar(max) not null,
[RegisteredDate] datetime not null,
[Currency] numeric(10,2) default(0.00)
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId]),
CONSTRAINT [UK1_Customers] UNIQUE ([LoginId]),
CONSTRAINT [UK2_Customers] UNIQUE ([Email]));

BEGIN
SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers]
(CustomerId, LoginId, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate, Currency)
SELECT CustomerId, LoginId, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate, Currency
FROM #custTmp;
SET IDENTITY_INSERT [Customers] OFF;
END

DROP TABLE #custTmp;
END
ELSE
BEGIN
CREATE TABLE [Customers] (
[CustomerId] int not null identity(1,1),
[LoginId] nvarchar(50) not null,
[Email] nvarchar(100) not null,
[PhoneNumber] nvarchar(15) null,
[Password] nvarchar(max) not null,
[PasswordSalt] nvarchar(max) not null,
[RegisteredDate] datetime not null,
[Currency] numeric(10,2) default(0.00),
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId]),
CONSTRAINT [UK1_Customers] UNIQUE ([LoginId]),
CONSTRAINT [UK2_Customers] UNIQUE ([Email]));
END

-- Setup of Tickets table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Tickets')
BEGIN

SELECT * INTO #ticketTmp FROM [Tickets];

DROP TABLE [Tickets];

CREATE TABLE [Tickets] (
[TicketId] int not null identity(1000, 1),
[ConcertId] int not null,
CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED ([TicketId]));

BEGIN
SET IDENTITY_INSERT [Tickets] ON;
INSERT INTO [Tickets]
(TicketId, ConcertId)
SELECT TicketId, ConcertId
FROM #ticketTmp;
SET IDENTITY_INSERT [Tickets] OFF;
END

DROP TABLE #ticketTmp;
END
ELSE
BEGIN
CREATE TABLE [Tickets] (
[TicketId] int not null identity(1000, 1),
[ConcertId] int not null,
[Price] numeric (10, 2),
CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED ([TicketId]));
END

-- Setup of Coupons table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Coupons')
BEGIN

SELECT * INTO #couponTmp FROM [Coupons];

DROP TABLE [Coupons];

CREATE TABLE [Coupons] (
[CouponId] int not null identity(5000, 1),
[TicketId] int not null,
CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([CouponId]));

BEGIN
SET IDENTITY_INSERT [Coupons] ON;
INSERT INTO [Coupons]
(CouponId, TicketId)
SELECT CouponId, TicketId
FROM #couponTmp;
SET IDENTITY_INSERT [Coupons] OFF;
END

DROP TABLE #couponTmp;
END
ELSE
BEGIN
CREATE TABLE [Coupons] (
[CouponId] int not null identity (5000,1),
[TicketId] int not null);
END

-- Setup of Concerts table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Concerts')
BEGIN

SELECT * INTO #concertTmp FROM [Concerts];

DROP TABLE [Concerts];

CREATE TABLE [Concerts] (
[ConcertId] int not null identity(10000, 1),
[ArtistId] int not null,
[VenueId] int not null,
[CalendarDate] datetime not null,
[Cancelled] bit default(0),
[Price] numeric(10, 2),
[TicketsLeft] int null,
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]));

BEGIN
SET IDENTITY_INSERT [Concerts] ON;
INSERT INTO  [Concerts]
(ConcertId, ArtistId, VenueId, CalendarDate, Cancelled, Price, TicketsLeft)
SELECT ConcertId, ArtistId, VenueId, CalendarDate, Cancelled, Price, TicketsLeft
FROM #concertTmp;
SET IDENTITY_INSERT [Concerts] OFF;
END

DROP TABLE #concertTmp;
END
ELSE
BEGIN
CREATE TABLE [Concerts] (
[ConcertId] int not null identity(10000, 1),
[ArtistId] int not null,
[VenueId] int not null,
[CalendarDate] datetime not null,
[Cancelled] bit default(0),
[Price] numeric (10,2) null,
[TicketsLeft] int null,
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]));
END

-- Setup of Venues table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Venues')
BEGIN

SELECT * INTO #venuesTmp FROM [Venues];

DROP TABLE [Venues];

CREATE TABLE [Venues] (
[VenueId] int not null identity(1,1),
[VenueName] nvarchar(255) not null,
[Capacity] int null,
[Coordinates] nvarchar(150) null,
[City] nvarchar(100) null,
CONSTRAINT [PK_Venues] PRIMARY KEY CLUSTERED ([VenueId]),
CONSTRAINT [CHK_CenueCapacity] CHECK ([Capacity] > 0));

BEGIN
SET IDENTITY_INSERT [Venues] ON;
INSERT INTO [Venues]
(VenueId, VenueName, Capacity, Coordinates)
SELECT VenueId, VenueName, Capacity, Coordinates, City
FROM #venuesTmp;
SET IDENTITY_INSERT [Venues] OFF;
END

DROP TABLE #venuesTmp;
END
ELSE
BEGIN
CREATE TABLE [Venues] (
[VenueId] int not null identity (1,1),
[VenueName] nvarchar(255) not null,
[Capacity] int null,
[Coordinates] nvarchar(150) null,
CONSTRAINT [PK_Venues] PRIMARY KEY CLUSTERED ([VenueId]),
CONSTRAINT [CHK_CenueCapacity] CHECK ([Capacity] > 0));
END

-- Setup of CustomerTickets table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'CustomerTickets')
BEGIN

SELECT * INTO #custTicketTmp FROM [CustomerTickets];

DROP TABLE [CustomerTickets];

CREATE TABLE [CustomerTickets] (
[CustomerId] int not null,
[TicketId] int not null,
[SoldDate] datetime not null default(GETDATE()),
CONSTRAINT [PK_CustomerTickets] PRIMARY KEY CLUSTERED ([CustomerId], [TicketId]));

BEGIN
INSERT INTO [CustomerTickets]
(CustomerId, TicketId, SoldDate)
SELECT CustomerId, TicketId, SoldDate
FROM #custTicketTmp;
END

DROP TABLE #custTicketTmp;
END
ELSE
BEGIN
CREATE TABLE [CustomerTickets] (
[CustomerId] int not null,
[TicketId] int not null,
[SoldDate] datetime not null default(GETDATE()),
CONSTRAINT [PK_CustomerTickets] PRIMARY KEY CLUSTERED ([CustomerId], [TicketId]));
END

-- Setup of Artists table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Artists')
BEGIN

SELECT * INTO #artistTmp FROM [Artists];

DROP TABLE [Artists];

CREATE TABLE [Artists] (
[ArtistId] int not null identity(1,1),
[ArtistName] nvarchar(255) not null,
CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ArtistId]));

BEGIN
SET IDENTITY_INSERT [Artists] ON;
INSERT INTO [Artists]
(ArtistId, ArtistName)
SELECT ArtistId, ArtistName
FROM #artistTmp;
SET IDENTITY_INSERT [Artists] OFF;
END

DROP TABLE #artistTmp;
END
ELSE
BEGIN
CREATE TABLE [Artists] (
[ArtistId] int not null identity(1,1),
[ArtistName] nvarchar(255) not null,
CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ArtistId]));
END

-- Setup of Foreign Keys
ALTER TABLE [Concerts]
ADD CONSTRAINT [FK1_Concerts]
FOREIGN KEY ([ArtistId])
REFERENCES [Artists] ([ArtistId])
ON DELETE CASCADE;

ALTER TABLE [Concerts]
ADD CONSTRAINT [FK2_Concerts]
FOREIGN KEY ([VenueId])
REFERENCES [Venues] ([VenueId]);

ALTER TABLE [CustomerTickets]
ADD CONSTRAINT [FK1_CustomerTickets]
FOREIGN KEY ([CustomerId])
REFERENCES [Customers] ([CustomerId]);

ALTER TABLE [CustomerTickets]
ADD CONSTRAINT [FK2_CustomerTickets]
FOREIGN KEY ([TicketId])
REFERENCES [Tickets] ([TicketId]);

ALTER TABLE [Tickets]
ADD CONSTRAINT [FK1_Tickets]
FOREIGN KEY ([ConcertId])
REFERENCES [Concerts] ([ConcertId]);

ALTER TABLE [Coupons]
ADD CONSTRAINT [FK1_Coupons]
FOREIGN KEY ([TicketId])
REFERENCES [Tickets] ([TicketId]);