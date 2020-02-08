IF NOT EXISTS (
SELECT 1 FROM sys.databases
WHERE name = 'TicketSystem')
BEGIN
CREATE DATABASE [TicketSystem];
END
GO

USE [TicketSystem];
GO

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
CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([CustomerId]),
CONSTRAINT [UK1_Customers] UNIQUE ([LoginId]),
CONSTRAINT [UK2_Customers] UNIQUE ([Email]));

BEGIN
SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers]
(CustomerId, LoginId, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate)
SELECT CustomerId, LoginId, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate
FROM #custTmp;
SET IDENTITY_INSERT [Customers] OFF;
END

DROP TABLE #custTmp;
END

IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Tickets')
BEGIN

SELECT * INTO #ticketTmp FROM [Tickets];

DROP TABLE [Tickets];

CREATE TABLE [Tickets] (
[TicketId] int not null identity(1000, 1),
[ConcertId] int not null,
[Price] numeric (5, 2),
CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED ([TicketId]));

BEGIN
SET IDENTITY_INSERT [Tickets] ON;
INSERT INTO [Tickets]
(TicketId, ConcertId, Price)
SELECT TicketId, ConcertId, Price
FROM #ticketTmp;
SET IDENTITY_INSERT [Tickets] OFF;
END

DROP TABLE #ticketTmp;
END

IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Concerts')
BEGIN

SELECT * INTO #concertTmp FROM [Concerts];

DROP TABLE [Concerts];

CREATE TABLE [Concerts] (
[ConcertId] int not null identity(10000, 1),
[ArtistId] int not null,
[LocationId] int not null,
[CalendarDate] datetime not null,
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]));

BEGIN
SET IDENTITY_INSERT [Concerts] ON;
INSERT INTO  [Concerts]
(ConcertId, ArtistId, LocationId, CalendarDate)
SELECT ConcertId, ArtistId, LocationId, CalendarDate
FROM #concertTmp;
SET IDENTITY_INSERT [Concerts] OFF;
END

DROP TABLE #concertTmp;
END

IF EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Locations')
BEGIN

SELECT * INTO #locationTmp FROM [Locations];

DROP TABLE [Locations];

CREATE TABLE [Locations] (
[LocationId] int not null identity(1,1),
[LocationCoordinates] nvarchar(150) null,
CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([LocationId]));

BEGIN
SET IDENTITY_INSERT [Locations] ON;
INSERT INTO [Locations]
(LocationId, LocationCoordinates)
SELECT LocationId, LocationCoordinates
FROM #locationTmp;
SET IDENTITY_INSERT [Locations] OFF;
END

DROP TABLE #locationTmp;
END

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

ALTER TABLE [Concerts]
ADD CONSTRAINT [FK1_Concerts]
FOREIGN KEY ([ArtistId])
REFERENCES [Artists] ([ArtistId])
ON DELETE CASCADE;

ALTER TABLE [Concerts]
ADD CONSTRAINT [FK2_Concerts]
FOREIGN KEY ([LocationId])
REFERENCES [Locations] ([LocationId]);

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