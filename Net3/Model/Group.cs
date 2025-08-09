using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3.Model;

public class Group {
    public required string? UserId { get; set; }
    public required string? ConversationId { get; set; }

    public User? User { get; set; }
    public Conversation? Conversation { get; set; }
}

