using System.Net.Sockets;
using System.Text;

using Net3.Packets;
namespace Net3;

/// <summary>
/// Represents a TCP socket that handles sending and receiving serialized packets.
/// </summary>
public class TcpSocket {
    // Underlying TCP socket used for communication
    private TcpClient? Tcpsocket { get; set; } = null;

    // Network stream used for reading/writing data
    private NetworkStream? Tcpstream { get; set; } = null;

    /// <summary>
    /// Default constructor - initializes LastPing to current time.
    /// </summary>
    public TcpSocket() => this.LastPing = DateTime.Now;

    /// <summary>
    /// Constructor that initializes with an existing TcpClient.
    /// Sets up the network stream and updates LastPing.
    /// </summary>
    /// <param name="socket">The connected TcpClient.</param>
    public TcpSocket(TcpClient socket) {
        this.LastPing = DateTime.Now;

        this.Tcpsocket = socket;
        this.Tcpstream = socket.GetStream();
    }

    /// <summary>
    /// Initializes the network stream by setting up the reader and writer with UTF-8 encoding.
    /// </summary>
    public void InitNetworkStream() {
        try {
            // Create a StreamReader for reading from the TCP stream
            this.reader = new StreamReader(Tcpstream!, Encoding.UTF8);

            // Create a StreamWriter for writing to the TCP stream (auto-flush enabled)
            this.writer = new StreamWriter(Tcpstream!, Encoding.UTF8) { AutoFlush = true };
        }
        catch (Exception ex) {
            throw new Exception(ex.ToString());
        }
    }

    /// <summary>
    /// The duration in seconds to determine if the client is still considered alive 
    /// based on the time of the last received ping.
    /// </summary>
    public int PingTimeoutSeconds { get; set; } = 120;
    private DateTime? LastPing { get; set; } = null;
    public bool IsAlive => (DateTime.Now - LastPing!.Value).TotalSeconds <= PingTimeoutSeconds;

    // Checks if the TCP socket is connected.
    public bool IsConnected => this.Tcpsocket!.Connected;

    /// <summary>
    /// Function for recive and sent packet asynchronously over the TCP stream.
    /// </summary>
    private StreamReader? reader;
    private StreamWriter? writer;

    //Sends a packet asynchronously over the TCP stream.
    public async Task SendAsync(Packet prk) {
        if (Tcpstream == null)
            return;

        var packet = prk.Serialize() + "\n";
        var data = Encoding.UTF8.GetBytes(packet);
        
        try {
            await Tcpstream.WriteAsync(data, 0, data.Length);
            await Tcpstream.FlushAsync();
        }
        catch (Exception ex) {
            throw new Exception(ex.ToString());
        }
    }
    // Receives a packet asynchronously from the TCP stream.
    public async Task<Packet?> RecvAsync() {
        if (Tcpstream == null)
            return null;

        try {
            using var reader = new StreamReader(Tcpstream, Encoding.UTF8, leaveOpen: true);
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                return null;

            return Packet.Deserialize(line);
        }
        catch (Exception ex) {
            throw new Exception(ex.ToString());
        }
    }

    /// <summary>
    /// Disconnects the TCP socket and disposes of its resources.
    /// </summary>
    public void Disconnect() {
        if (Tcpsocket != null && !Tcpsocket.Connected)
            return;

        try {
            Tcpstream?.Close();
            Tcpsocket?.Close();
            Tcpstream?.Dispose();
        }
        catch (Exception ex) {
            throw new Exception(ex.ToString());
        }

        Tcpstream = null;
    }

    ~TcpSocket() {
        // Finalizer to ensure the socket is disconnected when the object is destroyed.
        Disconnect();
    }
}

