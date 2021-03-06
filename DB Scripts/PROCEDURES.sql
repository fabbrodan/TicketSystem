IF EXISTS (
SELECT 1 FROM sys.procedures
WHERE name = 'SP_Purchase')
BEGIN
DROP PROCEDURE [SP_Purchase];
END
GO

CREATE PROCEDURE [SP_Purchase] (@concertId int, @customerId int)
AS
BEGIN

BEGIN TRAN
BEGIN TRY

UPDATE c
SET c.Currency -= con.Price
FROM Customers c,
Concerts con
WHERE CustomerId = @customerId
AND con.ConcertId = @concertId;

INSERT INTO Tickets (ConcertId, CustomerId) VALUES(@concertId, @customerId);

UPDATE c
SET c.TicketsLeft = c.TicketsLeft - 1
FROM Concerts c
WHERE c.ConcertId = @concertId;

END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRAN
END CATCH
IF @@TRANCOUNT > 0
COMMIT TRAN
END
GO

IF EXISTS (
SELECT 1
FROM sys.procedures
WHERE name = 'SP_CancelConcert')
BEGIN
DROP PROCEDURE [SP_CancelConcert];
END
GO

CREATE PROCEDURE [SP_CancelConcert] (@concertId int)
AS
BEGIN

BEGIN TRAN
BEGIN TRY

UPDATE Concerts SET Cancelled = 1
WHERE ConcertId = @concertId;

;WITH cte AS (
	SELECT c.CustomerId, con.ConcertId
	FROM Customers c
	INNER JOIN CustomerTickets ct ON c.CustomerId = ct.CustomerId
	INNER JOIN Tickets t ON ct.TicketId = t.TicketId
	INNER JOIN Concerts con ON con.ConcertId = t.ConcertId
	WHERE con.ConcertId = @concertId
	)
UPDATE c
SET c.Currency = c.Currency + (
SELECT SUM(con.Price)
FROM Concerts con
INNER JOIN cte ON con.ConcertId = cte.ConcertId
GROUP BY cte.CustomerId)
FROM Customers c
INNER JOIN cte ON c.CustomerId = cte.CustomerId;

INSERT INTO Coupons
(TicketId)
SELECT t.TicketId
FROM Tickets t
INNER JOIN Concerts c ON t.ConcertId = t.ConcertId
WHERE c.ConcertId = @concertId;

END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRAN
END CATCH
IF @@TRANCOUNT > 0
COMMIT TRAN
END
GO