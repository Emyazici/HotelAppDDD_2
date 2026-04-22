using HotelAppDDD.Domain.Common;

namespace HotelAppDDD.Domain.Reservation.Events
{
public record class ReservationCreatedEvent(Guid Id, Guid RoomId, Guid GuestId, DateTime CheckInDate, DateTime CheckOutDate) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
}