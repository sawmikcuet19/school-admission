using SchoolAdmission.Api.Common;

namespace SchoolAdmission.Api.Validators;

public static class StudentIdValidator
{
    public const string HeaderName = "X-Student-Id";

    public static string Require(string? rawStudentId)
    {
        if (string.IsNullOrWhiteSpace(rawStudentId))
        {
            throw new BadRequestException(
                $"Header {HeaderName} is required. It must be the 17 digit birth registration number.");
        }

        var studentId = rawStudentId.Trim();

        if (studentId.Length != 17 || !studentId.All(char.IsAsciiDigit))
        {
            throw new BadRequestException(
                $"Header {HeaderName} must be exactly 17 digits (the student's birth registration number).");
        }

        return studentId;
    }
}
