namespace HotelAppDDD.Domain.Reservation
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid GuestId { get; private set; }
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public bool IsCancelled { get; private set; }

        private Reservation() { }

        public static Reservation Create(Guid roomId, Guid guestId, DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate >= checkOutDate)
                throw new ArgumentException("Check-in date must be before check-out date.");

            var day = (checkOutDate - checkInDate).Days;
            if (day <= 0)
                throw new ArgumentException("Reservation must be at least one day.");
            if (day > 30)
                throw new ArgumentException("Reservation cannot be longer than 30 days.");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                GuestId = guestId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                IsCancelled = false
            };
            

            return reservation;
        }
    }
}