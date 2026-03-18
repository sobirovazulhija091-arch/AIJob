public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; }=null!;
    public string LastName { get; set; }=null!;
    public string AboutMe { get; set; }=null!;
    public int ExperienceYears { get; set; }
    public decimal ExpectedSalary { get; set; }
    public string? CVFileUrl { get; set;}
}