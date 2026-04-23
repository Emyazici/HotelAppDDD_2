using HotelAppDDD.Domain.Common;
using HotelAppDDD.Domain.Reservation.Events;
using HotelAppDDD.Domain.Exceptions;
using System.Linq.Expressions;

namespace HotelAppDDD.Domain.Reservation
{
    public class Reservation :AggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid GuestId { get; private set; }
        public DateTime ExpectedCheckInDate { get; private set; }
        public DateTime ExpectedCheckOutDate { get; private set; }
        public DateTime? ActualCheckOutDate { get; private set; }
        public DateTime? ActualCheckInDate { get; private set; }
        public bool IsCheckedOut => ActualCheckOutDate.HasValue;
        public bool IsCheckedIn => ActualCheckInDate.HasValue;
        public bool IsActive { get; private set; }
        private Reservation() { }

        public static Reservation Create(Guid roomId, Guid guestId, DateTime expectedCheckInDate, DateTime expectedCheckOutDate)
        {
            var day = (expectedCheckOutDate - expectedCheckInDate).Days;
            if (day <= 0)
                throw new BusinessRuleException("Reservation must be at least one day.");
            if (day > 30)
                throw new BusinessRuleException("Reservation cannot be longer than 30 days.");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                GuestId = guestId,
                ExpectedCheckInDate = expectedCheckInDate,
                ExpectedCheckOutDate = expectedCheckOutDate,
                IsActive = true
            };

            reservation.AddDomainEvent(new ReservationCreatedEvent(
                reservation.Id,
                reservation.RoomId,
                reservation.GuestId,
                reservation.ExpectedCheckInDate,
                reservation.ExpectedCheckOutDate
                ));
            
            return reservation;
        }

        public void Cancel()
        {
            if (!IsActive)
                throw new BusinessRuleException("Reservation has been also cancelled");
			if (IsCheckedIn)
				throw new BusinessRuleException("Cannot cancel a reservation that has already been checked in.");
            IsActive = false;
            AddDomainEvent(new ReservationCancelledEvent(Id, RoomId));
        }

        public void CheckIn()
        {
            if (IsCheckedIn)
                throw new BusinessRuleException("The Reservation has been also CheckedIn");

            ActualCheckInDate = DateTime.UtcNow;
            AddDomainEvent(new ReservationCheckedInEvent(Id, RoomId, GuestId, ActualCheckInDate.Value));
        }

        public void CheckOut(DateTime checkOutDate)
        {
            if (!IsCheckedIn)
                throw new BusinessRuleException("Cannot check out without checking in first.");

            if (IsCheckedOut)
                throw new BusinessRuleException("The Reservation has been also CheckedOut");

            ActualCheckOutDate = checkOutDate;
            AddDomainEvent(new ReservationCheckedOutEvent(Id, RoomId, GuestId, ActualCheckOutDate.Value));
        }


        public int CalculateLateFee()
        {
            if (!IsCheckedOut) return 0;

            var day = (ActualCheckOutDate.Value - ExpectedCheckOutDate).Days;

            if (day <= 0) return 0;

            return day * 2000;
        }


    }
}