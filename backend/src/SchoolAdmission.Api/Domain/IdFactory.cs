namespace SchoolAdmission.Api.Domain;

public static class IdFactory
{
    public static string NewClassId() => "c_" + Guid.NewGuid().ToString("N");
    public static string NewHoldId() => "h_" + Guid.NewGuid().ToString("N");
}
