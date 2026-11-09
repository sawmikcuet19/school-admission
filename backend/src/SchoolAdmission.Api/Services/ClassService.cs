using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Dtos;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Services;

public class ClassService : IClassService
{
    private readonly AppDbContext db;
    private readonly TimeProvider timeProvider;

    public ClassService(AppDbContext db, TimeProvider timeProvider)
    {
        this.db = db;
        this.timeProvider = timeProvider;
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var admissionClass = new AdmissionClass
        {
            Name = request.Name.Trim(),
            TotalSeats = request.TotalSeats,
            CreatedAtUtc = Now
        };

        db.Classes.Add(admissionClass);
        await db.SaveChangesAsync(cancellationToken);

        return new ClassResponse(admissionClass.Id, admissionClass.Name, admissionClass.TotalSeats);
    }

    public async Task<IReadOnlyList<ClassSeatsResponse>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        var classes = await db.Classes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var now = Now;

        var counts = await db.Holds
            .AsNoTracking()
            .Where(x => x.Status == HoldStatus.HELD || x.Status == HoldStatus.CONFIRMED)
            .GroupBy(x => new { x.ClassId, x.Status })
            .Select(g => new { g.Key.ClassId, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeHeld = await db.Holds
            .AsNoTracking()
            .Where(x => x.Status == HoldStatus.HELD && x.ExpiresAtUtc > now)
            .GroupBy(x => x.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, cancellationToken);

        var confirmed = counts
            .Where(x => x.Status == HoldStatus.CONFIRMED)
            .GroupBy(x => x.ClassId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        return classes
            .Select(c => Map(c, activeHeld.GetValueOrDefault(c.Id), confirmed.GetValueOrDefault(c.Id)))
            .ToList();
    }

    public async Task<ClassSeatsResponse> GetClassAsync(string classId, CancellationToken cancellationToken = default)
    {
        var admissionClass = await db.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == classId, cancellationToken)
            ?? throw new NotFoundException("Class not found.");

        var now = Now;

        var held = await db.Holds
            .AsNoTracking()
            .CountAsync(x => x.ClassId == classId && x.Status == HoldStatus.HELD && x.ExpiresAtUtc > now, cancellationToken);

        var confirmed = await db.Holds
            .AsNoTracking()
            .CountAsync(x => x.ClassId == classId && x.Status == HoldStatus.CONFIRMED, cancellationToken);

        return Map(admissionClass, held, confirmed);
    }

    private static ClassSeatsResponse Map(AdmissionClass admissionClass, int held, int confirmed)
    {
        var available = Math.Max(0, admissionClass.TotalSeats - held - confirmed);

        return new ClassSeatsResponse(
            admissionClass.Id,
            admissionClass.Name,
            admissionClass.TotalSeats,
            held,
            confirmed,
            available);
    }

    private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;
}
