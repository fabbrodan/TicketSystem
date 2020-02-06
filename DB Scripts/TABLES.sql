IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Administrators')
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

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN
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
END

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Tickets')
BEGIN
CREATE TABLE [Tickets] (
[TicketId] int not null identity(1000, 1),
[ConcertId] int not null,
[Price] numeric (5, 2),
CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED ([TicketId]));
END

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Concerts')
BEGIN
CREATE TABLE [Concerts] (
[ConcertId] int not null identity(10000, 1),
[ArtistId] int not null,
[LocationId] int not null,
[CalendarDate] datetime not null,
CONSTRAINT [PK_Concerts] PRIMARY KEY CLUSTERED ([ConcertId]));
END

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Locations')
BEGIN
CREATE TABLE [Locations] (
[LocationId] int not null identity(1,1),
[LocationCoordinates] nvarchar(150) null,
CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([LocationId]));
END

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'CustomerTickets')
BEGIN
CREATE TABLE [CustomerTickets] (
[CustomerId] int not null,
[TicketId] int not null,
[SoldDate] datetime not null default(GETDATE()),
CONSTRAINT [PK_CustomerTickets] PRIMARY KEY CLUSTERED ([CustomerId], [TicketId]));
END

IF NOT EXISTS (
SELECT 1 FROM sys.tables WHERE name = 'Artists')
BEGIN
CREATE TABLE [Artists] (
[ArtistId] int not null identity(1,1),
[ArtistName] nvarchar(255) not null,
CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ArtistId]));
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