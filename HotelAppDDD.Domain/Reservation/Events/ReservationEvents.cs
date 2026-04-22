using HotelAppDDD.Domain.Common;

namespace HotelAppDDD.Domain.Reservation.Events;

public record class ReservationCreatedEvent(Guid ReservationId, Guid RoomId, Guid GuestId, DateTime ExpectedCheckInDate, DateTime ExpectedCheckOutDate) : IDomainEvent
{
    public Guid Id { get;  } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class ReservationCancelledEvent(Guid ReservationId, Guid RoomId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class ReservationCheckedInEvent(Guid ReservationId, Guid RoomId, Guid GuestId, DateTime ActualCheckInDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class ReservationCheckedOutEvent(Guid ReservationId, Guid RoomId, Guid GuestId, DateTime ActualCheckOutDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

