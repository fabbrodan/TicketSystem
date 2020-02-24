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
[AdminId] int not null identity(1,1), --Using identity in order to properly increment. WCan be reseeded using DBCC if necessary
[LoginName] nvarchar(50) not null,
[Email] nvarchar(100) not null,
[Password] nvarchar(max) not null,
[PasswordSalt] nvarchar(max) not null,
[RegisteredDate] datetime not null,
[IsActive] bit default(1) not null,
CONSTRAINT [PK_Administrators] PRIMARY KEY CLUSTERED ([AdminId]), -- Primary key on the ID
CONSTRAINT [UK1_Administrators] UNIQUE ([Email])); -- But unique constraint on Email
-- But not on LoginName, since that can be "passed on" from inactive users

BEGIN
SET IDENTITY_INSERT [Administrators] ON;
INSERT INTO [Administrators]
(AdminId, LoginName, Email, Password, PasswordSalt, RegisteredDate)
SELECT AdminId, LoginName, Email, Password, PasswordSalt, RegisteredDate
FROM #adminTmp;
SET IDENTITY_INSERT [Administrators] OFF;
END

DROP TABLE #adminTmp;
END
ELSE
BEGIN
CREATE TABLE [Administrators] (
[AdminId] int not null identity(1,1),
[LoginName] nvarchar(50) not null,
[Email] nvarchar(100) not null,
[Password] nvarchar(max) not null,
[PasswordSalt] nvarchar(max) not null,
[RegisteredDate] datetime not null,
[IsActive] bit default(1) not null,
CONSTRAINT [PK_Administrators] PRIMARY KEY CLUSTERED ([AdminId]),
CONSTRAINT [UK1_Administrators] UNIQUE ([Email]));
END

-- Setup of Customers table
IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN

SELECT * INTO #custTmp FROM [Customers];

DROP TABLE [Customers];

CREATE TABLE [Customers] (
[CustomerId] int not null identity(1,1),
[LoginName] nvarchar(50) not null,
[Email] nvarchar(100) not null,
[PhoneNumber] nvarchar(15) null,
[Password] nvarchar(max) not null,
[PasswordSalt] nvarchar(max) not null,
[RegisteredDate] datetime not null,
[Currency] numeric(10,2) default(100.00),
[IsActive] bit default(1) not null
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId]),
CONSTRAINT [UK1_Customers] UNIQUE ([Email]),
CONSTRAINT [CHK1_Customers] CHECK ([Currency] !< 0));
-- Constraints here follow the same principle as the Adminstrators table
-- Except Currency check

BEGIN
SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers]
(CustomerId, LoginName, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate, Currency)
SELECT CustomerId, LoginName, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate, Currency
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
[Currency] numeric(10,2) default(100.00) not null,
[IsActive] bit default(1),
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId]),
CONSTRAINT [UK1_Customers] UNIQUE ([Email]),
CONSTRAINT [CHK1_Customers] CHECK ([Currency] !< 0));
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
[CustomerId] int not null,
[SoldDate] datetime not null default(GETDATE()),
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
[CustomerId] int not null,
[SoldDate] datetime not null default(GETDATE()),
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
[ExpirationDate] datetime default(DATEADD(mm, 3, CONVERT(date, GETDATE()))),
-- Default constraint to always set the expiration date three months from the date concert was cancelled
CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([CouponId]));

BEGIN
SET IDENTITY_INSERT [Coupons] ON;
INSERT INTO [Coupons]
(CouponId, TicketId, ExpirationDate)
SELECT CouponId, TicketId, ExpirationDate
FROM #couponTmp;
SET IDENTITY_INSERT [Coupons] OFF;
END

DROP TABLE #couponTmp;
END
ELSE
BEGIN
CREATE TABLE [Coupons] (
[CouponId] int not null identity (5000,1),
[TicketId] int not null,
[ExpirationDate] datetime default(DATEADD(mm, 3, CONVERT(date, GETDATE()))),
CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([CouponId]));
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
[Price] numeric(10, 2) not null,
[TicketsLeft] int null,
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]),
CONSTRAINT [UK1_Concerts] UNIQUE ([ArtistId], [CalendarDate]), -- To prevent same artist having two shows booked same day
CONSTRAINT [UK2_Concerts] UNIQUE ([VenueId], [CalendarDate]), -- To prevent same Venue having two shows booked same day
CONSTRAINT [CHK1_CalendarDate] CHECK(CalendarDate !< GETDATE()), -- To prevent show being booked in the past
CONSTRAINT [CHK2_TicketsLeft] CHECK(TicketsLeft !< 0)); -- Prevent a ticket being sold if there are no tickets left

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
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]),
CONSTRAINT [UK1_Concerts] UNIQUE ([ArtistId], [CalendarDate]),
CONSTRAINT [UK2_Concerts] UNIQUE ([VenueId], [CalendarDate]),
CONSTRAINT [CHK1_CalendarDate] CHECK(CalendarDate !< GETDATE()));
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
CONSTRAINT [UK1_Venues] UNIQUE ([VenueName]), -- Should be unique, but not a good PK
CONSTRAINT [CHK_VenueCapacity] CHECK ([Capacity] > 0)); -- Venues has to have seats

BEGIN
SET IDENTITY_INSERT [Venues] ON;
INSERT INTO [Venues]
(VenueId, VenueName, Capacity, Coordinates, City)
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
CONSTRAINT [UK1_Venues] UNIQUE ([VenueName]),
CONSTRAINT [CHK_VenueCapacity] CHECK ([Capacity] > 0));
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
CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ArtistId]),
CONSTRAINT [UK1_Artists] UNIQUE([ArtistName]));
-- ArtistId is a good PK, but not name since, well nvarchar, but name should still be unique

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
CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ArtistId]),
CONSTRAINT [UK1_Artists] UNIQUE([ArtistName]));
END

-- Setup of Foreign Keys
ALTER TABLE [Concerts]
ADD CONSTRAINT [FK1_Concerts]
FOREIGN KEY ([ArtistId])
REFERENCES [Artists] ([ArtistId]);
-- A concert needs a valid artist

ALTER TABLE [Concerts]
ADD CONSTRAINT [FK2_Concerts]
FOREIGN KEY ([VenueId])
REFERENCES [Venues] ([VenueId]);
-- A concert needs a valid venue

ALTER TABLE [Tickets]
ADD CONSTRAINT [FK1_Tickets]
FOREIGN KEY ([ConcertId])
REFERENCES [Concerts] ([ConcertId]);
-- A ticket needs to be sold to an actual concert

ALTER TABLE [Tickets]
ADD CONSTRAINT [FK2_Tickets]
FOREIGN KEY ([CustomerId])
REFERENCES [Customers] ([CustomerId]);


ALTER TABLE [Coupons]
ADD CONSTRAINT [FK1_Coupons]
FOREIGN KEY ([TicketId])
REFERENCES [Tickets] ([TicketId]);
-- A coupon has to be referred to an actual ticket

-- Indexes
-- To prevent the same username being used if there already is an ACTIVE
-- user with that name but allow it if the username belongs to an inactive user
CREATE NONCLUSTERED INDEX [NC_IX_LoginId_Customers] ON [Customers] ([LoginName])
WHERE [IsActive] = 1;

CREATE NONCLUSTERED INDEX [NC_IX_LoginName_Administrators] ON [Administrators] ([LoginName])
WHERE [IsActive] = 1;

-- Add Default admin user if one does not exists with credentials
-- Username: admin@admin
-- Password: admin
IF (SELECT COUNT(*) FROM Administrators) = 0
BEGIN
INSERT INTO Administrators
(LoginName, Email, Password, PasswordSalt)
VALUES(
'admin',
'admin@admin.com',
'KXq?l?K{??.??r??{?x?y??];?0??????[j????U?3???J?T?????????-k',
'jM.?^?1|?=V??,4iB???0r? ?.?K');
END