public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; }=null!;
    public string? Description { get; set; }
    public OrganizationType Type { get; set; }
    public string? Location { get; set; }
}