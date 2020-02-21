CREATE PROCEDURE [SP_Purchase] (@concertId int, @customerId int)
AS
BEGIN

BEGIN TRAN
BEGIN TRY

UPDATE Customers SET Currency = Currency - (
SELECT Price FROM Concerts WHERE ConcertId = @concertId)
WHERE CustomerId = @customerId;

INSERT INTO Tickets (ConcertId) VALUES(@concertId);

INSERT INTO CustomerTickets
(CustomerId, TicketId)
VALUES(@customerId, (SELECT MAX(TicketId) FROM Tickets));

UPDATE Concerts
SET TicketsLeft = TicketsLeft - 1
WHERE ConcertId = @concertId;

END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRAN
END CATCH
IF @@TRANCOUNT > 0
COMMIT TRAN
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