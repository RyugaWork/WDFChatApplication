using System.Text.Json;

namespace Net3.Packets;
public class Packet {
    // Type of the packet (e.g., "Connect", "Ping", "Message")
    public string? type { get; set; } = null;

    // Optional timestamp indicating when the packet was created or sent
    public DateTime? timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Packet() { }

    /// <summary>
    /// Constructor that sets the packet type.
    /// </summary>
    /// <param name="type">The type of packet.</param>
    public Packet(string? type) => this.type = type;

    // Serializes the current object to a JSON string using its runtime type.
    public string Serialize() => JsonSerializer.Serialize(this, GetType());

    // Deserializes a JSON string into a specific Packet type based on the "type" property.
    // returns <A Packet object or null if deserialization fails.>
    public static Packet? Deserialize(string json) {
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("type", out var typeProp))
            return null;

        string? type = typeProp.GetString();
        return type switch {
            "Connect" => JsonSerializer.Deserialize<Packet>(json),
            "Ping" => JsonSerializer.Deserialize<Packet>(json),
            "Message" => JsonSerializer.Deserialize<Tcp_Mess_Pck>(json),
            _ => JsonSerializer.Deserialize<Packet>(json)
        };
    }
}
