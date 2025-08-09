using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3.Model;

public class Conversation {
    public required string Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Message>? Messages { get; set; }
    public ICollection<Group>? Groups { get; set; }
}
