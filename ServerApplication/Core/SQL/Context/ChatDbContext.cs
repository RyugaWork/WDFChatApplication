using Microsoft.EntityFrameworkCore;
using Net3.Model;

namespace ServerApplication.Core.SQL.Context;

public class ChatDbContext : DbContext {
    public DbSet<User> Users { get; set; }
    public DbSet<UserPassword> UserPasswords { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Group> Groups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Composite key for Group
        modelBuilder.Entity<Group>()
            .HasKey(g => new { g.UserId, g.ConversationId });

        // Group relationships
        modelBuilder.Entity<Group>()
            .HasOne(g => g.User)
            .WithMany(u => u.Groups)
            .HasForeignKey(g => g.UserId);

        modelBuilder.Entity<Group>()
            .HasOne(g => g.Conversation)
            .WithMany(c => c.Groups)
            .HasForeignKey(g => g.ConversationId);

        // User-Password 1:1
        modelBuilder.Entity<User>()
            .HasOne(u => u.Password)
            .WithOne(p => p.User)
            .HasForeignKey<UserPassword>(p => p.Id);

        // Message relationships
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.From)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.To)
            .OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<Message>()
        //    .HasOne(m => m.Conversation)
        //    .WithMany(c => c.Messages)
        //    .HasForeignKey(m => m.ConversationId);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=ChatContext.db");
}