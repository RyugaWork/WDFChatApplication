using Microsoft.EntityFrameworkCore;
using Net3.Model;
using ServerApplication.Core.SQL.Context;

namespace ServerApplication.Core.SQL;

public class ChatRepository : IDisposable {
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context) {
        _context = context;
    }

    // ----------- SEARCH / QUERY ------------

    public async Task<User?> GetUserByIdAsync(string id) {
        return await _context.Users
            .Include(u => u.Password)
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<User>> SearchUsersByNameAsync(string name) {
        return await _context.Users
            .Where(u => EF.Functions.Like(u.DisplayName, $"%{name}%"))
            .ToListAsync();
    }

    //public async Task<List<Message>> GetMessagesInConversationAsync(string conversationId) {
    //    return await _context.Messages
    //        .Where(m => m.ConversationId == conversationId)
    //        .Include(m => m.Sender)
    //        .Include(m => m.Receiver)
    //        .OrderBy(m => m.Time)
    //        .ToListAsync();
    //}

    // ----------- ADD ------------

    public async Task AddUserAsync(User user) {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task AddMessageAsync(Message message) {
        Console.WriteLine(".b.");
        _context.Messages.Add(message);

        Console.WriteLine(".a.");
        await _context.SaveChangesAsync();
    }

    public async Task AddConversationAsync(Conversation conversation) {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
    }

    // ----------- REMOVE ------------

    public async Task RemoveUserAsync(int userId) {
        var user = await _context.Users.FindAsync(userId);
        if (user != null) {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveMessageAsync(int messageId) {
        var msg = await _context.Messages.FindAsync(messageId);
        if (msg != null) {
            _context.Messages.Remove(msg);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveConversationAsync(int conversationId) {
        var convo = await _context.Conversations.FindAsync(conversationId);
        if (convo != null) {
            _context.Conversations.Remove(convo);
            await _context.SaveChangesAsync();
        }
    }

    // ----------- EXTRA: Search Groups ------------

    public async Task<List<Group>> GetGroupsByUserAsync(string userId) {
        return await _context.Groups
            .Where(g => g.UserId == userId)
            .Include(g => g.Conversation)
            .ToListAsync();
    }

    // ----------- CLEANUP ------------
    public void Dispose() {
        _context.Dispose();
    }
}
