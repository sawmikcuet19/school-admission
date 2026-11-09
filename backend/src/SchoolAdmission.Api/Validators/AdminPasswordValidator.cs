using SchoolAdmission.Api.Common;

namespace SchoolAdmission.Api.Validators;

public static class AdminPasswordValidator
{
    public const string HeaderName = "X-Admin-Password";

    public static string Require(string? rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            throw new BadRequestException(
                $"Header {HeaderName} is required to perform admin actions.");
        }

        return rawPassword;
    }
}
