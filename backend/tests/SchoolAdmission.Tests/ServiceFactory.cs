using Microsoft.Extensions.Options;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Tests;

internal static class ServiceFactory
{
    public static ClassService CreateClassService(AppDbContext db)
    {
        return new ClassService(db, TimeProvider.System);
    }

    public static HoldService CreateHoldService(AppDbContext db, int holdDurationSeconds = 600)
    {
        return new HoldService(
            db,
            TimeProvider.System,
            Options.Create(new HoldOptions { HoldDurationSeconds = holdDurationSeconds }));
    }

    public static AdminService CreateAdminService(AppDbContext db)
    {
        return new AdminService(db, TimeProvider.System);
    }
}
