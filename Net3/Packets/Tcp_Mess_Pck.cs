namespace Net3.Packets;

/// <summary>
/// Represents a message packet containing text and sender info.
/// Inherits from Packet with type set to "Message".
/// </summary>
public class Tcp_Mess_Pck : Packet {
    // The message content
    public required string? text { get; set; } = null;
    // The sender's identifier (e.g., username)
    public required string? sender { get; set; } = null;

    // Constructor sets the base Packet type to "Message".
    public Tcp_Mess_Pck() : base("Message") { }
}
