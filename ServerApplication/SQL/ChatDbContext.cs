using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Net3.Model;
using System.Threading.Tasks;

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
        await _context.SaveChangesAsync();
    }

    public async Task Clear() {
        _context.RemoveRange(_context.Messages);
        await _context.SaveChangesAsync();
    }

    public void Dispose() {
        _context.Dispose();
    }
}