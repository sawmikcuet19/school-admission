using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext db;
    private readonly TimeProvider timeProvider;

    public AdminService(AppDbContext db, TimeProvider timeProvider)
    {
        this.db = db;
        this.timeProvider = timeProvider;
    }

    public async Task<bool> VerifyPasswordAsync(string password, CancellationToken cancellationToken = default)
    {
        var credential = await db.AdminCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == AdminCredential.SingletonId, cancellationToken);

        return credential is not null && AdminPasswordHasher.Verify(password, credential.PasswordHash);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var credential = await db.AdminCredentials
            .FirstOrDefaultAsync(x => x.Id == AdminCredential.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("The admin verification password is not initialized.");

        if (!AdminPasswordHasher.Verify(currentPassword, credential.PasswordHash))
        {
            throw new UnauthorizedException("The current verification password is incorrect.");
        }

        credential.PasswordHash = AdminPasswordHasher.Hash(newPassword);
        credential.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        await db.SaveChangesAsync(cancellationToken);
    }
}
