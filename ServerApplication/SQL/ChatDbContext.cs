using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Net3.Model;

namespace ServerApplication.SQL;

//Add-Migration InitialCreate
//Update-Database

public class ChatDbContext : DbContext {
    public DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=ChatContext.db")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
}

public class ChatRepository : IDisposable {
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context) {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task AddMessageAsync(Message message) {
        await _context.Messages.AddAsync(message);
        Console.WriteLine(".E.");
        await _context.SaveChangesAsync();
        Console.WriteLine(".F.");
    }

    public void Dispose() {
        _context.Dispose();
    }
}