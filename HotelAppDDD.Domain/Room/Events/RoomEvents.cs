using HotelAppDDD.Domain.Common;

namespace HotelAppDDD.Domain.Room.Events;

public record class RoomCreatedEvent(Guid Id, int RoomNumber, string RoomType, int Capacity) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class RoomReservedEvent(Guid Id,int RoomNumber) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class RoomReleasedEvent(Guid Id,int RoomNumber) : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}