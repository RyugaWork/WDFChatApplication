namespace Net3.Model;
public class Message {
    public int Id { get; set; }
    public string? sender { get; set; } = null;
    public string? to { get; set; } = null;
    public string? text { get; set; } = null;
    public DateTime? Timestamp { get; set; }
}

