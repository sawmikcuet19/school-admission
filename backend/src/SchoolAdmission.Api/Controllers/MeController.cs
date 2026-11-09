using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Services.Interfaces;
using SchoolAdmission.Api.Validators;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class MeController : ControllerBase
{
    private readonly IHoldService holdService;

    public MeController(IHoldService holdService)
    {
        this.holdService = holdService;
    }

    [HttpGet("/me/holds")]
    public async Task<IActionResult> GetMyHolds(CancellationToken cancellationToken)
    {
        var studentId = StudentIdValidator.Require(Request.Headers[StudentIdValidator.HeaderName]);
        return Ok(await holdService.GetMyHoldsAsync(studentId, cancellationToken));
    }
}
