namespace SchoolAdmission.Api.Services.Interfaces;

public interface IAdminService
{
    Task<bool> VerifyPasswordAsync(string password, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
