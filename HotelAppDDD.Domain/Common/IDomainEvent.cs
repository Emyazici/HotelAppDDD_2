using MediatR;

namespace HotelAppDDD.Domain.Common
{
    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
    }
}