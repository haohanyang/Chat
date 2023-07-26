using Microsoft.EntityFrameworkCore;
using Chat.Server.Data.Entity;
namespace Chat.Server.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Space> Spaces { get; set; } = null!;
    public DbSet<DirectChat> DirectChats { get; set; } = null!;
    public DbSet<DirectMessage> DirectMessages { get; set; } = null!;
    public DbSet<SpaceMessage> SpaceMessages { get; set; } = null!;
    public DbSet<SpaceMembership> SpaceMemberships { get; set; } = null!;
    public DbSet<DirectMessageAttachment> DirectMessageAttachments { get; set; } = null!;
    public DbSet<SpaceMessageAttachment> SpaceMessageAttachments { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString =
           Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        if (connectionString == null)
        {
            throw new Exception("Connection string is not set");
        }
        options.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(e => e.DirectChats1)
            .WithOne(e => e.User1)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.User1Id)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.DirectChats2)
            .WithOne(e => e.User2)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.User2Id)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.SpaceMemberships)
            .WithOne(e => e.Member)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.MemberId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.SpaceMessages)
            .WithOne(e => e.Author)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.AuthorId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.DirectMessages)
            .WithOne(e => e.Author)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.AuthorId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.DirectMessageAttachments)
            .WithOne(e => e.Uploader)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.UploaderId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasMany(e => e.SpaceMessageAttachments)
            .WithOne(e => e.Uploader)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.UploaderId)
            .IsRequired();

        modelBuilder.Entity<Space>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.Space)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.SpaceId)
            .IsRequired();

        modelBuilder.Entity<Space>()
            .HasMany(e => e.Messages)
            .WithOne(e => e.Space)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(e => e.SpaceId)
            .IsRequired();

        modelBuilder.Entity<DirectMessage>()
            .HasOne(e => e.Attachment)
            .WithOne(e => e.DirectMessage)
            .HasForeignKey<DirectMessageAttachment>(e => e.DirectMessageId)
            .IsRequired();

        modelBuilder.Entity<SpaceMessage>()
            .HasOne(e => e.Attachment)
            .WithOne(e => e.SpaceMessage)
            .HasForeignKey<SpaceMessageAttachment>(e => e.SpaceMessageId)
            .IsRequired();

        // Message content length is limited to 500 characters
        modelBuilder.Entity<DirectMessage>()
            .Property(m => m.Content)
            .HasColumnType("VARCHAR")
            .HasMaxLength(500);

        modelBuilder.Entity<SpaceMessage>()
            .Property(m => m.Content)
            .HasColumnType("VARCHAR")
            .HasMaxLength(500);

        // User Id is limited to 64 characters
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .HasColumnType("VARCHAR")
            .HasMaxLength(64);

        // Username is limited to 32 characters
        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(32);

        // User first name and last name length is limited to 30 characters
        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(30);

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(30);

        // User bio length is limited to 300 characters
        modelBuilder.Entity<User>()
            .Property(u => u.Bio)
            .HasColumnType("VARCHAR")
            .HasMaxLength(300);

        // Space name length is limited to 30 characters
        modelBuilder.Entity<Space>()
            .Property(s => s.Name)
            .HasColumnType("VARCHAR")
            .HasMaxLength(30);

        // Space description length is limited to 300 characters
        modelBuilder.Entity<Space>()
            .Property(s => s.Description)
            .HasColumnType("VARCHAR")
            .HasMaxLength(300);

        modelBuilder.Entity<DirectMessageAttachment>()
            .Property(a => a.FileName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(80);

        modelBuilder.Entity<DirectMessageAttachment>()
            .Property(a => a.ContentType)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50);

        modelBuilder.Entity<SpaceMessageAttachment>()
            .Property(a => a.FileName)
            .HasColumnType("VARCHAR")
            .HasMaxLength(80);

        modelBuilder.Entity<SpaceMessageAttachment>()
            .Property(a => a.ContentType)
            .HasColumnType("VARCHAR")
            .HasMaxLength(50);
    }
}