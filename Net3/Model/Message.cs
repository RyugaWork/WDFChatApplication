namespace Net3.Model;

public class Message {
    public int Id { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Text { get; set; }
    public DateTime? Time { get; set; }
    //public required string ConversationId { get; set; }

    public User? Sender { get; set; }
    public User? Receiver { get; set; }
    public Conversation? Conversation { get; set; }
}

