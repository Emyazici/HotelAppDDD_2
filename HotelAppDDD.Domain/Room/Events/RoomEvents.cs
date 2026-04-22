using HotelAppDDD.Domain.Common;

namespace HotelAppDDD.Domain.Room.Events;

public record class RoomCreatedEvent(Guid RoomId, int RoomNumber, string RoomType, int Capacity) : IDomainEvent
{
    public Guid Id { get;  } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class RoomReservedEvent(Guid RoomId,int RoomNumber) : IDomainEvent
{
    public Guid Id { get;  } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record class RoomReleasedEvent(Guid RoomId,int RoomNumber) : IDomainEvent
{
    public Guid Id { get;  } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}