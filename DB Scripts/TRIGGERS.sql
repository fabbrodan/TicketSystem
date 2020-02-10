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