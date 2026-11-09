using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Dtos;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Services;

public class HoldService : IHoldService
{
    private readonly AppDbContext db;
    private readonly TimeProvider timeProvider;
    private readonly HoldOptions options;

    public HoldService(AppDbContext db, TimeProvider timeProvider, IOptions<HoldOptions> options)
    {
        this.db = db;
        this.timeProvider = timeProvider;
        this.options = options.Value;
    }

    public async Task<HoldResponse> CreateHoldAsync(string classId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(classId))
        {
            throw new BadRequestException("Class id is required.");
        }

        var now = Now;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await LockRowAsync("Classes", classId, cancellationToken);

        var admissionClass = await db.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == classId, cancellationToken)
            ?? throw new NotFoundException("Class not found.");

        await db.Holds
            .Where(x => x.ClassId == classId
                        && x.StudentId == studentId
                        && x.Status == HoldStatus.HELD
                        && x.ExpiresAtUtc <= now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, HoldStatus.EXPIRED)
                    .SetProperty(x => x.UpdatedAtUtc, now),
                cancellationToken);

        var hasActiveApplication = await db.Holds
            .AsNoTracking()
            .AnyAsync(
                x => x.ClassId == classId
                     && x.StudentId == studentId
                     && (x.Status == HoldStatus.CONFIRMED
                         || (x.Status == HoldStatus.HELD && x.ExpiresAtUtc > now)),
                cancellationToken);

        if (hasActiveApplication)
        {
            throw new ConflictException("You already have an active application or confirmed admission for this class.");
        }

        var heldSeats = await db.Holds
            .AsNoTracking()
            .CountAsync(x => x.ClassId == classId && x.Status == HoldStatus.HELD && x.ExpiresAtUtc > now, cancellationToken);

        var confirmedSeats = await db.Holds
            .AsNoTracking()
            .CountAsync(x => x.ClassId == classId && x.Status == HoldStatus.CONFIRMED, cancellationToken);

        if (heldSeats + confirmedSeats >= admissionClass.TotalSeats)
        {
            throw new ConflictException("No seats are available for this class.");
        }

        var hold = new SeatHold
        {
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddSeconds(Math.Max(0, options.HoldDurationSeconds))
        };

        db.Holds.Add(hold);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("You already have an active application or confirmed admission for this class.");
        }

        return new HoldResponse(hold.Id, hold.ClassId, hold.StudentId, hold.Status, hold.ExpiresAtUtc);
    }

    public async Task<HoldStatusResponse> GetHoldAsync(string holdId, CancellationToken cancellationToken = default)
    {
        var hold = await db.Holds
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        var now = Now;
        var status = EffectiveStatus(hold, now);

        return new HoldStatusResponse(hold.Id, hold.ClassId, status, hold.ExpiresAtUtc);
    }

    public async Task<ConfirmHoldResponse> ConfirmHoldAsync(string holdId, string studentId, CancellationToken cancellationToken = default)
    {
        var now = Now;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await LockRowAsync("Holds", holdId, cancellationToken);

        var hold = await db.Holds
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        EnsureOwner(hold, studentId);

        if (hold.Status == HoldStatus.CONFIRMED)
        {
            await transaction.CommitAsync(cancellationToken);
            return new ConfirmHoldResponse(hold.Id, HoldStatus.CONFIRMED);
        }

        if (hold.Status != HoldStatus.HELD)
        {
            throw new ConflictException($"This application cannot be confirmed because it is {hold.Status}.");
        }

        if (hold.ExpiresAtUtc <= now)
        {
            hold.Status = HoldStatus.EXPIRED;
            hold.UpdatedAtUtc = now;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            throw new ConflictException("The hold has expired. Please apply again.");
        }

        hold.Status = HoldStatus.CONFIRMED;
        hold.ConfirmedAtUtc = now;
        hold.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ConfirmHoldResponse(hold.Id, HoldStatus.CONFIRMED);
    }

    public async Task CancelHoldAsync(string holdId, string studentId, CancellationToken cancellationToken = default)
    {
        var now = Now;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await LockRowAsync("Holds", holdId, cancellationToken);

        var hold = await db.Holds
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        EnsureOwner(hold, studentId);

        if (hold.Status == HoldStatus.CONFIRMED)
        {
            throw new ConflictException("A confirmed admission cannot be cancelled.");
        }

        if (hold.Status == HoldStatus.HELD)
        {
            hold.Status = HoldStatus.CANCELLED;
            hold.CancelledAtUtc = now;
            hold.UpdatedAtUtc = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MyHoldResponse>> GetMyHoldsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var now = Now;

        var holds = await db.Holds
            .AsNoTracking()
            .Include(x => x.Class)
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return holds
            .Select(x => new MyHoldResponse(
                x.Id,
                x.ClassId,
                x.Class?.Name ?? string.Empty,
                EffectiveStatus(x, now),
                x.ExpiresAtUtc))
            .ToList();
    }

    private static HoldStatus EffectiveStatus(SeatHold hold, DateTime now)
    {
        if (hold.Status == HoldStatus.HELD && hold.ExpiresAtUtc <= now)
        {
            return HoldStatus.EXPIRED;
        }

        return hold.Status;
    }

    private static void EnsureOwner(SeatHold hold, string studentId)
    {
        if (!string.Equals(hold.StudentId, studentId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("You can only act on your own application.");
        }
    }

    private async Task LockRowAsync(string table, string id, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = $"SELECT \"Id\" FROM \"{table}\" WHERE \"Id\" = @id FOR UPDATE";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        await command.ExecuteScalarAsync(cancellationToken);
    }

    private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;
}
