namespace SchoolAdmission.Api.Services;

public class HoldOptions
{
    public const int DefaultHoldDurationSeconds = 600;

    public int HoldDurationSeconds { get; set; } = DefaultHoldDurationSeconds;
}
