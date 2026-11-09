using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Services.Interfaces;
using SchoolAdmission.Api.Validators;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class HoldsController : ControllerBase
{
    private readonly IHoldService holdService;

    public HoldsController(IHoldService holdService)
    {
        this.holdService = holdService;
    }

    [HttpGet("/holds/{holdId}")]
    public async Task<IActionResult> GetHold(string holdId, CancellationToken cancellationToken)
    {
        return Ok(await holdService.GetHoldAsync(holdId, cancellationToken));
    }

    [HttpPost("/holds/{holdId}/confirm")]
    public async Task<IActionResult> ConfirmHold(string holdId, CancellationToken cancellationToken)
    {
        var studentId = StudentIdValidator.Require(Request.Headers[StudentIdValidator.HeaderName]);
        return Ok(await holdService.ConfirmHoldAsync(holdId, studentId, cancellationToken));
    }

    [HttpDelete("/holds/{holdId}")]
    public async Task<IActionResult> CancelHold(string holdId, CancellationToken cancellationToken)
    {
        var studentId = StudentIdValidator.Require(Request.Headers[StudentIdValidator.HeaderName]);
        await holdService.CancelHoldAsync(holdId, studentId, cancellationToken);
        return NoContent();
    }
}
