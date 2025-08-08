namespace Net3.API;
public class Message {
    public int Id { get; set; }
    public string? sender { get; set; } = null;
    public string? text { get; set; } = null;
    public DateTime? Timestamp { get; set; }
}

