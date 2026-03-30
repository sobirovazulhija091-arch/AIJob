using Domain.Entities;
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

    public DbSet<UserProfile> UserProfiles { get; set; } 
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Education> Educations { get; set; } 
    public DbSet<UserEducation> UserEducations { get; set; } 
    public DbSet<UserExperience> UserExperiences { get; set; } 
    public DbSet<Skill> Skills { get; set; } 
    public DbSet<UserSkill> UserSkills { get; set; } 
    public DbSet<ProfileSkill> ProfileSkills { get; set; } 
    public DbSet<ProfileLanguage> ProfileLanguages { get; set; }
    public DbSet<Language> Languages { get; set; } 
    public DbSet<Organization> Organizations { get; set; } 
    public DbSet<OrganizationMember> OrganizationMembers { get; set; } 
    public DbSet<Job> Jobs { get; set; } 
    public DbSet<JobCategory> JobCategories { get; set; } 
    public DbSet<JobSkill> JobSkills { get; set; } 
    public DbSet<JobApplication> JobApplications { get; set; } 
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<Endorsement> Endorsements { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }

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

        modelBuilder.Entity<Profile>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId);

        
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

        // Profile ↔ ProfileLanguage ↔ Language (many-to-many via ProfileLanguage)
        modelBuilder.Entity<ProfileLanguage>()
            .HasOne<Profile>()
            .WithMany()
            .HasForeignKey(pl => pl.ProfileId);

        modelBuilder.Entity<ProfileLanguage>()
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(pl => pl.LanguageId);

      
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

        modelBuilder.Entity<Connection>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.RequesterId);

        modelBuilder.Entity<Connection>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.AddresseeId);

        modelBuilder.Entity<Conversation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.User1Id);

        modelBuilder.Entity<Conversation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.User2Id);

        modelBuilder.Entity<Message>()
            .HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId);

        modelBuilder.Entity<Message>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.SenderId);

        modelBuilder.Entity<Post>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<Post>()
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(p => p.RepostOfPostId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PostLike>()
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostLike>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostLike>()
            .HasIndex(l => new { l.PostId, l.UserId })
            .IsUnique();

        modelBuilder.Entity<PostComment>()
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(c => c.PostId);

        modelBuilder.Entity<PostComment>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<PostComment>()
            .Property(c => c.Content)
            .HasMaxLength(2000);

        modelBuilder.Entity<Endorsement>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.EndorserId);

        modelBuilder.Entity<Endorsement>()
            .HasOne<ProfileSkill>()
            .WithMany()
            .HasForeignKey(e => e.ProfileSkillId);

        modelBuilder.Entity<Endorsement>()
            .HasIndex(e => new { e.EndorserId, e.ProfileSkillId })
            .IsUnique();

        modelBuilder.Entity<Recommendation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.AuthorId);

        modelBuilder.Entity<Recommendation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.RecipientId);

        modelBuilder.Entity<Recommendation>()
            .Property(r => r.Content)
            .HasMaxLength(2000);
    }
}