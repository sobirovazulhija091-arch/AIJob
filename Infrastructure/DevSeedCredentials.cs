namespace Infrastructure;

/// <summary>Default demo accounts created on startup (see WebApi Program.cs). No passwords are exposed via API.</summary>
public static class DevSeedCredentials
{
    public sealed record SeedAccount(string Email, string FullName, string Phone, string Password, string Role);

    public static readonly SeedAccount[] Accounts =
    [
        new("admin@example.com", "Admin User", "+0000000000", "Admin123!", "Admin"),
        new("candidate@example.com", "Test Candidate", "+1111111111", "Candidate123!", "Candidate"),
        new("organization@example.com", "Test Organization", "+2222222222", "Organization123!", "Organization"),
    ];
}
