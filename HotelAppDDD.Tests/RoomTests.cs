using FluentAssertions;
using HotelAppDDD.Domain.Exceptions;
using HotelAppDDD.Domain.Room;
using HotelAppDDD.Domain.Room.Events;

namespace HotelAppDDD.Tests;

public class RoomTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidData_ShouldReturnAvailableRoom()
    {
        var room = Room.Create(101, "Deluxe", 2);

        room.RoomNumber.Should().Be(101);
        room.RoomType.Should().Be("Deluxe");
        room.Capacity.Should().Be(2);
        room.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseRoomCreatedEvent()
    {
        var room = Room.Create(101, "Deluxe", 2);

        room.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoomCreatedEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCapacity_ShouldThrow(int capacity)
    {
        var act = () => Room.Create(101, "Standard", capacity);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*positive number*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyRoomType_ShouldThrow(string roomType)
    {
        var act = () => Room.Create(101, roomType, 2);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidRoomNumber_ShouldThrow(int roomNumber)
    {
        var act = () => Room.Create(roomNumber, "Standard", 2);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*positive integer*");
    }

    // --- RoomReserve ---

    [Fact]
    public void RoomReserve_AvailableRoom_ShouldBecomeUnavailable()
    {
        var room = Room.Create(101, "Standard", 3);

        room.RoomReserve(2);

        room.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void RoomReserve_AvailableRoom_ShouldRaiseRoomReservedEvent()
    {
        var room = Room.Create(101, "Standard", 3);

        room.RoomReserve(2);

        room.DomainEvents.OfType<RoomReservedEvent>()
            .Should().ContainSingle();
    }

    [Fact]
    public void RoomReserve_UnavailableRoom_ShouldThrow()
    {
        var room = Room.Create(101, "Standard", 3);
        room.RoomReserve(2);

        var act = () => room.RoomReserve(1);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public void RoomReserve_WithCapacityExceeded_ShouldThrow()
    {
        var room = Room.Create(101, "Standard", 2);

        var act = () => room.RoomReserve(5);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*capacity exceeded*");
    }

    [Fact]
    public void RoomReserve_WithExactCapacity_ShouldSucceed()
    {
        var room = Room.Create(101, "Standard", 2);

        var act = () => room.RoomReserve(2);

        act.Should().NotThrow();
    }

    // --- RoomRelease ---

    [Fact]
    public void RoomRelease_UnavailableRoom_ShouldBecomeAvailable()
    {
        var room = Room.Create(101, "Standard", 2);
        room.RoomReserve(1);

        room.RoomRelease();

        room.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void RoomRelease_UnavailableRoom_ShouldRaiseRoomReleasedEvent()
    {
        var room = Room.Create(101, "Standard", 2);
        room.RoomReserve(1);

        room.RoomRelease();

        room.DomainEvents.OfType<RoomReleasedEvent>()
            .Should().ContainSingle();
    }

    [Fact]
    public void RoomRelease_AlreadyAvailableRoom_ShouldThrow()
    {
        var room = Room.Create(101, "Standard", 2);

        var act = () => room.RoomRelease();

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*already available*");
    }
}
