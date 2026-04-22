using HotelAppDDD.Domain.Common;

namespace HotelAppDDD.Domain.Reservation.Events;

public record class ReservationCreatedEvent(Guid Id, Guid RoomId, Guid GuestId, DateTime ExpectedCheckInDate, DateTime ExpectedCheckOutDate) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class ReservationCancelledEvent(Guid Id, Guid RoomId) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

