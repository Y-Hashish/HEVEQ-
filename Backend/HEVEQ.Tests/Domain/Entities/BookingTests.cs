using FluentAssertions;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using Xunit;

namespace HEVEQ.Tests.Domain.Entities;

public class BookingTests
{
    [Fact]
    public void Booking_ShouldInitializeWithDefaultValues()
    {
        // Act
        var booking = new Booking();

        // Assert
        booking.Id.Should().NotBeEmpty();
        booking.BookingNumber.Should().BeEmpty();
        booking.Status.Should().Be(BookingStatus.Draft);
        booking.ReassignedFromBookings.Should().BeEmpty();
        booking.TimeAdjustmentRequests.Should().BeEmpty();
        booking.OperatorAssignments.Should().BeEmpty();
    }
}
