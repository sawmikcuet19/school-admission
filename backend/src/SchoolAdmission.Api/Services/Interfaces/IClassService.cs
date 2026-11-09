using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Services.Interfaces;

public interface IClassService
{
    Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSeatsResponse>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassSeatsResponse> GetClassAsync(string classId, CancellationToken cancellationToken = default);
}
