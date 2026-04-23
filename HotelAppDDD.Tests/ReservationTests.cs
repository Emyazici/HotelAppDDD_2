using FluentAssertions;
using HotelAppDDD.Domain.Exceptions;
using HotelAppDDD.Domain.Reservation;
using HotelAppDDD.Domain.Reservation.Events;

namespace HotelAppDDD.Tests;

public class ReservationTests
{
    private static readonly Guid RoomId = Guid.NewGuid();
    private static readonly Guid GuestId = Guid.NewGuid();

    // --- Create ---

    [Fact]
    public void Create_WithValidDates_ShouldReturnActiveReservation()
    {
        var checkIn = DateTime.Today.AddDays(1);
        var checkOut = DateTime.Today.AddDays(3);

        var reservation = Reservation.Create(RoomId, GuestId, checkIn, checkOut);

        reservation.RoomId.Should().Be(RoomId);
        reservation.GuestId.Should().Be(GuestId);
        reservation.ExpectedCheckInDate.Should().Be(checkIn);
        reservation.ExpectedCheckOutDate.Should().Be(checkOut);
        reservation.IsActive.Should().BeTrue();
        reservation.IsCheckedIn.Should().BeFalse();
        reservation.IsCheckedOut.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidDates_ShouldRaiseReservationCreatedEvent()
    {
        var checkIn = DateTime.Today.AddDays(1);
        var checkOut = DateTime.Today.AddDays(5);

        var reservation = Reservation.Create(RoomId, GuestId, checkIn, checkOut);

        reservation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ReservationCreatedEvent>();
    }

    [Fact]
    public void Create_WhenCheckOutEqualsCheckIn_ShouldThrow()
    {
        var date = DateTime.Today.AddDays(1);

        var act = () => Reservation.Create(RoomId, GuestId, date, date);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*at least one day*");
    }

    [Fact]
    public void Create_WhenCheckOutBeforeCheckIn_ShouldThrow()
    {
        var checkIn = DateTime.Today.AddDays(5);
        var checkOut = DateTime.Today.AddDays(2);

        var act = () => Reservation.Create(RoomId, GuestId, checkIn, checkOut);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_WhenDurationExceeds30Days_ShouldThrow()
    {
        var checkIn = DateTime.Today;
        var checkOut = DateTime.Today.AddDays(31);

        var act = () => Reservation.Create(RoomId, GuestId, checkIn, checkOut);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*30 days*");
    }

    [Fact]
    public void Create_WithExactly30Days_ShouldSucceed()
    {
        var checkIn = DateTime.Today;
        var checkOut = DateTime.Today.AddDays(30);

        var act = () => Reservation.Create(RoomId, GuestId, checkIn, checkOut);

        act.Should().NotThrow();
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_ActiveReservation_ShouldDeactivate()
    {
        var reservation = CreateValidReservation();

        reservation.Cancel();

        reservation.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ActiveReservation_ShouldRaiseCancelledEvent()
    {
        var reservation = CreateValidReservation();

        reservation.Cancel();

        reservation.DomainEvents.OfType<ReservationCancelledEvent>()
            .Should().ContainSingle();
    }

    [Fact]
    public void Cancel_AlreadyCancelledReservation_ShouldThrow()
    {
        var reservation = CreateValidReservation();
        reservation.Cancel();

        var act = () => reservation.Cancel();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*cancelled*");
    }

    [Fact]
    public void Cancel_AfterCheckIn_ShouldThrow()
    {
        var reservation = CreateValidReservation();
        reservation.CheckIn();

        var act = () => reservation.Cancel();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*checked in*");
    }

    // --- CheckIn ---

    [Fact]
    public void CheckIn_ShouldSetActualCheckInDate()
    {
        var reservation = CreateValidReservation();

        reservation.CheckIn();

        reservation.IsCheckedIn.Should().BeTrue();
        reservation.ActualCheckInDate.Should().NotBeNull();
    }

    [Fact]
    public void CheckIn_ShouldRaiseCheckedInEvent()
    {
        var reservation = CreateValidReservation();

        reservation.CheckIn();

        reservation.DomainEvents.OfType<ReservationCheckedInEvent>()
            .Should().ContainSingle();
    }

    [Fact]
    public void CheckIn_WhenAlreadyCheckedIn_ShouldThrow()
    {
        var reservation = CreateValidReservation();
        reservation.CheckIn();

        var act = () => reservation.CheckIn();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*CheckedIn*");
    }

    // --- CheckOut ---

    [Fact]
    public void CheckOut_AfterCheckIn_ShouldSetActualCheckOutDate()
    {
        var reservation = CreateValidReservation();
        reservation.CheckIn();

        reservation.CheckOut(DateTime.Now);

        reservation.IsCheckedOut.Should().BeTrue();
        reservation.ActualCheckOutDate.Should().NotBeNull();
    }

    [Fact]
    public void CheckOut_AfterCheckIn_ShouldRaiseCheckedOutEvent()
    {
        var reservation = CreateValidReservation();
        reservation.CheckIn();

        reservation.CheckOut(DateTime.Now);

        reservation.DomainEvents.OfType<ReservationCheckedOutEvent>()
            .Should().ContainSingle();
    }

    [Fact]
    public void CheckOut_WithoutCheckIn_ShouldThrow()
    {
        var reservation = CreateValidReservation();

        var act = () => reservation.CheckOut(DateTime.Now);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*checking in*");
    }

    [Fact]
    public void CheckOut_WhenAlreadyCheckedOut_ShouldThrow()
    {
        var reservation = CreateValidReservation();
        reservation.CheckIn();
        reservation.CheckOut(DateTime.Now);

        var act = () => reservation.CheckOut(DateTime.Now);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*CheckedOut*");
    }

    // --- CalculateLateFee ---

    [Fact]
    public void CalculateLateFee_WhenNotCheckedOut_ShouldReturnZero()
    {
        var reservation = CreateValidReservation();

        reservation.CalculateLateFee().Should().Be(0);
    }

    [Fact]
    public void CalculateLateFee_WhenCheckedOutBeforeExpected_ShouldReturnZero()
    {
        // CheckOut zamanı ExpectedCheckOutDate'den önce olacak şekilde: bugünden
        // 10 gün sonrası expected, checkout şimdi oluyor
        var expectedCheckOut = new DateTime(2024, 1, 11);
        var expectedCheckIn = new DateTime(2024, 1, 1);
        var actualCheckOut = new DateTime(2024, 1, 9); 
        var reservation = Reservation.Create(RoomId, GuestId, expectedCheckIn, expectedCheckOut);
        reservation.CheckIn();
        reservation.CheckOut(actualCheckOut); // ActualCheckOutDate = UtcNow < ExpectedCheckOutDate

        reservation.CalculateLateFee().Should().Be(0);
    }

	[Fact]
	public void CalculateLateFee_WhenCheckedOutLate_ShouldCalculateCorrectly()
	{
    // Arrange — 3 gün geç
    	var checkIn = new DateTime(2024, 1, 1);
    	var expectedCheckOut = new DateTime(2024, 1, 7);
    
    	var reservation = Reservation.Create(RoomId, GuestId, checkIn, expectedCheckOut);
    	reservation.CheckIn();

    	// Act — 3 gün geç çıkış
    	var checkout = new DateTime(2024, 1, 10);
		reservation.CheckOut(checkout);
    	// Assert — 3 x 2000 = 6000 TL
    	reservation.CalculateLateFee().Should().Be(6000);
	}

    // --- Yardımcı Metot ---

    private static Reservation CreateValidReservation()
        => Reservation.Create(RoomId, GuestId, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));
}
