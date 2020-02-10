CREATE TRIGGER [TR_TicketsLeft_Concert] ON [Concerts]
AFTER INSERT
AS
BEGIN

UPDATE [Concerts]
SET TicketsLeft = (
SELECT Capacity
FROM Venues v
INNER JOIN inserted i ON i.VenueId = v.VenueId);

END
GO

CREATE TRIGGER [TR_Cancel_Concert] ON [Concerts]
AFTER UPDATE
AS
BEGIN
IF UPDATE(Cancelled)
BEGIN
INSERT INTO Coupons
(TicketId)
SELECT t.TicketId
FROM Tickets t
INNER JOIN inserted i ON t.ConcertId = i.ConcertId
END
END
GO