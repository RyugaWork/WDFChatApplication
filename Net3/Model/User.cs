using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3.Model;

public class User {
    public required string Id { get; set; }
    public string? DisplayName { get; set; }

    public UserPassword? Password { get; set; }
    public ICollection<Message>? SentMessages { get; set; }
    public ICollection<Message>? ReceivedMessages { get; set; }
    public ICollection<Group>? Groups { get; set; }
}