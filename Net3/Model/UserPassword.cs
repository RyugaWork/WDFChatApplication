namespace Net3.Model;

public class UserPassword {
    public required string Id { get; set; }
    public string? Password { get; set; }

    public User? User { get; set; }
}
