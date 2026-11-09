namespace SchoolAdmission.Api.Domain;

public class AdminCredential
{
    public const string SingletonId = "admin";

    public string Id { get; set; } = SingletonId;
    public string PasswordHash { get; set; } = default!;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
