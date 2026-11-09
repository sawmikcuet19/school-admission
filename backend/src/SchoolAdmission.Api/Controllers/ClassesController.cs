using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Dtos;
using SchoolAdmission.Api.Services.Interfaces;
using SchoolAdmission.Api.Validators;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class ClassesController : ControllerBase
{
    private readonly IClassService classService;
    private readonly IHoldService holdService;
    private readonly IAdminService adminService;

    public ClassesController(
        IClassService classService,
        IHoldService holdService,
        IAdminService adminService)
    {
        this.classService = classService;
        this.holdService = holdService;
        this.adminService = adminService;
    }

    [HttpPost("/classes")]
    public async Task<IActionResult> CreateClass(CreateClassRequest request, CancellationToken cancellationToken)
    {
        var adminPassword = AdminPasswordValidator.Require(Request.Headers[AdminPasswordValidator.HeaderName]);

        if (!await adminService.VerifyPasswordAsync(adminPassword, cancellationToken))
        {
            throw new UnauthorizedException("The admin verification password is incorrect.");
        }

        var result = await classService.CreateClassAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("/classes")]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        return Ok(await classService.GetClassesAsync(cancellationToken));
    }

    [HttpGet("/classes/{classId}")]
    public async Task<IActionResult> GetClass(string classId, CancellationToken cancellationToken)
    {
        return Ok(await classService.GetClassAsync(classId, cancellationToken));
    }

    [HttpPost("/classes/{classId}/holds")]
    public async Task<IActionResult> CreateHold(string classId, CancellationToken cancellationToken)
    {
        var studentId = StudentIdValidator.Require(Request.Headers[StudentIdValidator.HeaderName]);
        var result = await holdService.CreateHoldAsync(classId, studentId, cancellationToken);
        return Ok(result);
    }
}
