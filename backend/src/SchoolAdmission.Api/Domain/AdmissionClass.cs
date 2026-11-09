namespace SchoolAdmission.Api.Domain;

public class AdmissionClass
{
    public string Id { get; set; } = IdFactory.NewClassId();
    public string Name { get; set; } = default!;
    public int TotalSeats { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SeatHold> Holds { get; set; } = new List<SeatHold>();
}
