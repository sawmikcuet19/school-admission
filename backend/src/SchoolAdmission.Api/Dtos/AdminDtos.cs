namespace SchoolAdmission.Api.Dtos;

public record VerifyAdminPasswordRequest(string Password);

public record ChangeAdminPasswordRequest(string CurrentPassword, string NewPassword);

public record AdminVerificationResponse(bool Valid);
