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