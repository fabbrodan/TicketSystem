CREATE VIEW [ConcertIndexView]
AS

SELECT c.ConcertId,
c.CalendarDate as ConcertDate,
c.Cancelled,
c.Price as ConcertPrice,
c.TicketsLeft,
a.ArtistName,
v.VenueName,
v.Capacity as VenueCapacity,
v.Coordinates,
v.City
FROM Concerts c
INNER JOIN Artists a ON c.ArtistId = a.ArtistId
INNER JOIN Venues v ON c.VenueId = v.VenueId;
GO