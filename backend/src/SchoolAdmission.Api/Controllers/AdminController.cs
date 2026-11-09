using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Dtos;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService adminService;

    public AdminController(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    [HttpPost("/admin/verify")]
    public async Task<IActionResult> Verify(VerifyAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        var valid = await adminService.VerifyPasswordAsync(request.Password, cancellationToken);

        if (!valid)
        {
            throw new UnauthorizedException("The verification password is incorrect.");
        }

        return Ok(new AdminVerificationResponse(true));
    }

    [HttpPost("/admin/password")]
    public async Task<IActionResult> ChangePassword(ChangeAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        await adminService.ChangePasswordAsync(
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        return NoContent();
    }
}
