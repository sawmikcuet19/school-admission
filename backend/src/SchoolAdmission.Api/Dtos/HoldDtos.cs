using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Dtos;

public record HoldResponse(
    string HoldId,
    string ClassId,
    string StudentId,
    HoldStatus Status,
    DateTime ExpiresAt
);

public record HoldStatusResponse(
    string HoldId,
    string ClassId,
    HoldStatus Status,
    DateTime ExpiresAt
);

public record ConfirmHoldResponse(string HoldId, HoldStatus Status);

public record MyHoldResponse(
    string HoldId,
    string ClassId,
    string ClassName,
    HoldStatus Status,
    DateTime ExpiresAt
);
