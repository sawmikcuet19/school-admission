using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Tests;

[Collection(nameof(PostgresCollection))]
public class HoldServiceTests
{
    private readonly PostgresFixture fixture;

    public HoldServiceTests(PostgresFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CreatingHold_ReservesOneSeatAndShowsInClassCounts()
    {
        await using var db = fixture.CreateDbContext();
        var classService = ServiceFactory.CreateClassService(db);
        var holdService = ServiceFactory.CreateHoldService(db);

        var admissionClass = await classService.CreateClassAsync(
            new CreateClassRequest("Class 6 - Morning Shift", 120));

        var hold = await holdService.CreateHoldAsync(admissionClass.Id, StudentId(1));

        Assert.Equal(HoldStatus.HELD, hold.Status);
        Assert.Equal(admissionClass.Id, hold.ClassId);

        var seats = await classService.GetClassAsync(admissionClass.Id);

        Assert.Equal(120, seats.TotalSeats);
        Assert.Equal(1, seats.HeldSeats);
        Assert.Equal(0, seats.ConfirmedSeats);
        Assert.Equal(119, seats.AvailableSeats);
    }

    [Fact]
    public async Task Student_CannotHoldTwoActiveSeatsInSameClass()
    {
        await using var db = fixture.CreateDbContext();
        var classService = ServiceFactory.CreateClassService(db);
        var holdService = ServiceFactory.CreateHoldService(db);

        var admissionClass = await classService.CreateClassAsync(
            new CreateClassRequest("Class 7 - Day Shift", 50));

        var studentId = StudentId(2);

        await holdService.CreateHoldAsync(admissionClass.Id, studentId);

        await Assert.ThrowsAsync<ConflictException>(
            () => holdService.CreateHoldAsync(admissionClass.Id, studentId));

        var seats = await classService.GetClassAsync(admissionClass.Id);
        Assert.Equal(1, seats.HeldSeats);
    }

    [Fact]
    public async Task ConfirmingHold_IsIdempotentAndMovesSeatToConfirmed()
    {
        await using var db = fixture.CreateDbContext();
        var classService = ServiceFactory.CreateClassService(db);
        var holdService = ServiceFactory.CreateHoldService(db);

        var admissionClass = await classService.CreateClassAsync(
            new CreateClassRequest("Class 8 - Morning Shift", 10));

        var studentId = StudentId(3);
        var hold = await holdService.CreateHoldAsync(admissionClass.Id, studentId);

        var first = await holdService.ConfirmHoldAsync(hold.HoldId, studentId);
        var second = await holdService.ConfirmHoldAsync(hold.HoldId, studentId);

        Assert.Equal(HoldStatus.CONFIRMED, first.Status);
        Assert.Equal(HoldStatus.CONFIRMED, second.Status);

        var seats = await classService.GetClassAsync(admissionClass.Id);

        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(1, seats.ConfirmedSeats);
        Assert.Equal(9, seats.AvailableSeats);
    }

    [Fact]
    public async Task ExpiredHold_CannotBeConfirmedAndReleasesTheSeat()
    {
        await using var db = fixture.CreateDbContext();
        var classService = ServiceFactory.CreateClassService(db);
        var holdService = ServiceFactory.CreateHoldService(db, holdDurationSeconds: 0);

        var admissionClass = await classService.CreateClassAsync(
            new CreateClassRequest("Class 9 - Morning Shift", 5));

        var studentId = StudentId(4);
        var hold = await holdService.CreateHoldAsync(admissionClass.Id, studentId);

        await Task.Delay(50);

        await Assert.ThrowsAsync<ConflictException>(
            () => holdService.ConfirmHoldAsync(hold.HoldId, studentId));

        var status = await holdService.GetHoldAsync(hold.HoldId);
        Assert.Equal(HoldStatus.EXPIRED, status.Status);

        var seats = await classService.GetClassAsync(admissionClass.Id);
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(0, seats.ConfirmedSeats);
        Assert.Equal(5, seats.AvailableSeats);
    }

    [Fact]
    public async Task ConcurrentRequests_NeverOverbookAClass()
    {
        const int totalSeats = 5;
        const int attempts = 40;

        await using (var setupDb = fixture.CreateDbContext())
        {
            var classService = ServiceFactory.CreateClassService(setupDb);
            var admissionClass = await classService.CreateClassAsync(
                new CreateClassRequest("Concurrency Class - Morning Shift", totalSeats));

            var studentIds = Enumerable.Range(0, attempts)
                .Select(i => StudentId(1000 + i))
                .ToArray();

            var tasks = studentIds.Select(async studentId =>
            {
                await using var db = fixture.CreateDbContext();
                var holdService = ServiceFactory.CreateHoldService(db);

                try
                {
                    await holdService.CreateHoldAsync(admissionClass.Id, studentId);
                    return true;
                }
                catch (ConflictException)
                {
                    return false;
                }
            });

            var results = await Task.WhenAll(tasks);

            Assert.Equal(totalSeats, results.Count(success => success));

            var seats = await classService.GetClassAsync(admissionClass.Id);
            Assert.Equal(totalSeats, seats.HeldSeats);
            Assert.Equal(0, seats.ConfirmedSeats);
            Assert.Equal(0, seats.AvailableSeats);

            await using var verificationDb = fixture.CreateDbContext();
            var activeHolds = await verificationDb.Holds.CountAsync(x =>
                x.ClassId == admissionClass.Id
                && x.Status == HoldStatus.HELD);

            Assert.Equal(totalSeats, activeHolds);
        }
    }

    private static string StudentId(int value) => "2" + value.ToString("D16");
}
