using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Education> Educations { get; set; } = null!;
    public DbSet<UserEducation> UserEducations { get; set; } = null!;
    public DbSet<UserExperience> UserExperiences { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<UserSkill> UserSkills { get; set; } = null!;
    public DbSet<ProfileSkill> ProfileSkills { get; set; } = null!;
    public DbSet<Language> Languages { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationMember> OrganizationMembers { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<JobCategory> JobCategories { get; set; } = null!;
    public DbSet<JobSkill> JobSkills { get; set; } = null!;
    public DbSet<JobApplication> JobApplications { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.FullName)
            .HasMaxLength(100);

        modelBuilder.Entity<Skill>()
            .HasIndex(s => s.Name)
            .IsUnique();

        modelBuilder.Entity<Skill>()
            .Property(s => s.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<JobCategory>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<JobCategory>()
            .Property(c => c.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.Name)
            .IsUnique();

        modelBuilder.Entity<Organization>()
            .Property(o => o.Name)
            .HasMaxLength(150);

        modelBuilder.Entity<Language>()
            .HasIndex(l => l.Name)
            .IsUnique();

        modelBuilder.Entity<Language>()
            .Property(l => l.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Job>()
            .Property(j => j.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<Job>()
            .Property(j => j.Description)
            .HasMaxLength(4000);

        modelBuilder.Entity<UserProfile>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.UserId);

        
        modelBuilder.Entity<UserExperience>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

     
        modelBuilder.Entity<UserEducation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        
        modelBuilder.Entity<UserSkill>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserSkill>()
            .HasOne<Skill>()
            .WithMany()
            .HasForeignKey(us => us.SkillId);

        // Profile ↔ Education (one-to-many)
        modelBuilder.Entity<Education>()
            .HasOne<Profile>()
            .WithMany()
            .HasForeignKey(e => e.ProfileId);

        // Profile ↔ ProfileSkill ↔ Skill (many-to-many via ProfileSkill)
        modelBuilder.Entity<ProfileSkill>()
            .HasOne<Profile>()
            .WithMany()
            .HasForeignKey(ps => ps.ProfileId);

        modelBuilder.Entity<ProfileSkill>()
            .HasOne<Skill>()
            .WithMany()
            .HasForeignKey(ps => ps.SkillId);

      
        modelBuilder.Entity<Job>()
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(j => j.OrganizationId);

       
        modelBuilder.Entity<Job>()
            .HasOne<JobCategory>()
            .WithMany()
            .HasForeignKey(j => j.CategoryId);

        
        modelBuilder.Entity<JobSkill>()
            .HasOne<Job>()
            .WithMany()
            .HasForeignKey(js => js.JobId);

        modelBuilder.Entity<JobSkill>()
            .HasOne<Skill>()
            .WithMany()
            .HasForeignKey(js => js.SkillId);

       
        modelBuilder.Entity<JobApplication>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(ja => ja.UserId);

        modelBuilder.Entity<JobApplication>()
            .HasOne<Job>()
            .WithMany()
            .HasForeignKey(ja => ja.JobId);

       
        modelBuilder.Entity<OrganizationMember>()
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(om => om.OrganizationId);

        modelBuilder.Entity<OrganizationMember>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(om => om.UserId);

     
        modelBuilder.Entity<Notification>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId);

        
        modelBuilder.Entity<RefreshToken>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

    }
}