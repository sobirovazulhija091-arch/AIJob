namespace Domain.Seeds;

public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Organization = nameof(UserRole.Organization);
    public const string Candidate = nameof(UserRole.Candidate);

    public static readonly UserRole[] All = { UserRole.Admin, UserRole.Organization, UserRole.Candidate };

    public static string ToName(UserRole role) => role.ToString();
}