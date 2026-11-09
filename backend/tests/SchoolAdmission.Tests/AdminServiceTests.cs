using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Tests;

[Collection(nameof(PostgresCollection))]
public class AdminServiceTests
{
    private readonly PostgresFixture fixture;

    public AdminServiceTests(PostgresFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task VerifyPassword_MatchesOnlyTheStoredHash()
    {
        await using var db = fixture.CreateDbContext();
        var adminService = await SeedAsync(db, "InitialPass1");

        Assert.True(await adminService.VerifyPasswordAsync("InitialPass1"));
        Assert.False(await adminService.VerifyPasswordAsync("WrongPass1"));
    }

    [Fact]
    public async Task ChangePassword_RequiresCurrentPasswordAndRehashes()
    {
        await using var db = fixture.CreateDbContext();
        var adminService = await SeedAsync(db, "InitialPass1");

        await adminService.ChangePasswordAsync("InitialPass1", "NewSecret99");

        await using var verificationDb = fixture.CreateDbContext();
        var verificationService = ServiceFactory.CreateAdminService(verificationDb);

        Assert.True(await verificationService.VerifyPasswordAsync("NewSecret99"));
        Assert.False(await verificationService.VerifyPasswordAsync("InitialPass1"));

        var credential = await verificationDb.AdminCredentials
            .AsNoTracking()
            .SingleAsync(x => x.Id == AdminCredential.SingletonId);

        Assert.NotEqual("NewSecret99", credential.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_IsRejected()
    {
        await using var db = fixture.CreateDbContext();
        var adminService = await SeedAsync(db, "InitialPass1");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => adminService.ChangePasswordAsync("WrongPass1", "NewSecret99"));

        Assert.True(await adminService.VerifyPasswordAsync("InitialPass1"));
    }

    private static async Task<AdminService> SeedAsync(AppDbContext db, string password)
    {
        await db.AdminCredentials.ExecuteDeleteAsync();

        db.AdminCredentials.Add(new AdminCredential
        {
            Id = AdminCredential.SingletonId,
            PasswordHash = AdminPasswordHasher.Hash(password),
            UpdatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return ServiceFactory.CreateAdminService(db);
    }
}
