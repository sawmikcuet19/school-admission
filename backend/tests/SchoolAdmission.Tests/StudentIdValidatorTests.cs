using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Validators;

namespace SchoolAdmission.Tests;

public class StudentIdValidatorTests
{
    [Theory]
    [InlineData("20152691234567890")]
    [InlineData("12345678901234567")]
    public void Accepts17DigitStudentId(string value)
    {
        Assert.Equal(value, StudentIdValidator.Require(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456a")]
    public void RejectsInvalidStudentId(string? value)
    {
        Assert.Throws<BadRequestException>(() => StudentIdValidator.Require(value));
    }
}
