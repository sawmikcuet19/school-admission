using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Services.Interfaces;

public interface IHoldService
{
    Task<HoldResponse> CreateHoldAsync(string classId, string studentId, CancellationToken cancellationToken = default);
    Task<HoldStatusResponse> GetHoldAsync(string holdId, CancellationToken cancellationToken = default);
    Task<ConfirmHoldResponse> ConfirmHoldAsync(string holdId, string studentId, CancellationToken cancellationToken = default);
    Task CancelHoldAsync(string holdId, string studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MyHoldResponse>> GetMyHoldsAsync(string studentId, CancellationToken cancellationToken = default);
}
