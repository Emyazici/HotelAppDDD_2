using HotelAppDDD.Domain.Common;
using HotelAppDDD.Domain.Room.Events;
using HotelAppDDD.Domain.Exceptions;

namespace HotelAppDDD.Domain.Room
{
    public class Room : AggregateRoot
    {
        public Guid Id { get; private set; }
        public int RoomNumber { get; private set; }
        public string RoomType { get; private set; } = null!;
        public bool IsAvailable { get; private set; }
        public int Capacity { get; private set; }

        private Room(){}
        public static Room Create(int roomNumber, string roomType, int capacity)
        {
			if(capacity <= 0)
                throw new BusinessRuleException("Room capacity must be a positive number.");

			if(string.IsNullOrWhiteSpace(roomType))
				throw new BusinessRuleException("Room type cannot be empty.");

			if(roomNumber <= 0)
				throw new BusinessRuleException("Room number must be a positive integer.");

            var room = new Room
            {
                Id = Guid.NewGuid(),
                RoomNumber = roomNumber,
                RoomType = roomType,
                IsAvailable = true,
                Capacity = capacity
            };

            room.AddDomainEvent(new RoomCreatedEvent(room.Id, room.RoomNumber, room.RoomType, room.Capacity));
            return room;
        }

        public void RoomReserve(int capacity)
        {
            if (!IsAvailable)
                throw new BusinessRuleException("Room is not available for reservation.");

            if (capacity > Capacity)
                throw new BusinessRuleException("Room capacity exceeded.");

            IsAvailable = false;
            AddDomainEvent(new RoomReservedEvent(Id, RoomNumber));
        }

        public void RoomRelease()
        {
            if (IsAvailable)
                throw new BusinessRuleException("Room is already available.");

            IsAvailable = true;
            AddDomainEvent(new RoomReleasedEvent(Id, RoomNumber));
        }

    }
}