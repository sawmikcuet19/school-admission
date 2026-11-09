namespace SchoolAdmission.Api.Domain;

public class SeatHold
{
    public string Id { get; set; } = IdFactory.NewHoldId();

    public string ClassId { get; set; } = default!;
    public AdmissionClass? Class { get; set; }

    public string StudentId { get; set; } = default!;

    public HoldStatus Status { get; set; } = HoldStatus.HELD;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
