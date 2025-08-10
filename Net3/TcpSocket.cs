using Net3.Packets;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Net3;

/// <summary>
/// Represents a TCP socket that handles sending and receiving serialized packets.
/// </summary>
public class TcpSocket : IDisposable {
    // Underlying TCP socket used for communication
    private TcpClient? Tcpsocket { get; set; } = null;

    // Network stream used for reading/writing data
    private NetworkStream? Tcpstream { get; set; } = null;

    /// <summary>
    /// Default constructor - initializes LastPing to current time.
    /// </summary>
    public TcpSocket() {
        this.LastPing = DateTime.Now;

        this.Tcpsocket = new TcpClient();
    }

    /// <summary>
    /// Constructor that initializes with an existing TcpClient.
    /// Sets up the network stream and updates LastPing.
    /// </summary>
    /// <param name="socket">The connected TcpClient.</param>
    public TcpSocket(TcpClient socket) {
        this.LastPing = DateTime.Now;

        this.Tcpsocket = socket;

        InitNetworkStream();
    }

    /// <summary>
    /// Initializes the network stream by setting up the reader and writer with UTF-8 encoding.
    /// </summary>
    public void InitNetworkStream() {
        //this.Tcpsocket!.NoDelay = true;
        this.Tcpstream = Tcpsocket!.GetStream();

        try {
            // Create a StreamReader for reading from the TCP stream
            this.reader = new StreamReader(Tcpstream!, Encoding.UTF8);

            // Create a StreamWriter for writing to the TCP stream (auto-flush enabled)
            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            writer = new StreamWriter(Tcpstream!, utf8NoBom) { AutoFlush = true };
        }
        catch (Exception ex) {
            throw new Exception($"NetworkStream unexpected error: {ex}");
        }
    }

    // Gets the local machine's IPv4 address.
    public static string LocalIPAddress => GetLocalIPAddress();

    // Retrieves the first available IPv4 address of the local machine.
    // Throws an exception if no IPv4 address is found.ns>
    private static string GetLocalIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString(); 
            }
        }
        throw new Exception("No network adapters with an IPv4 address found.");
    }

    // Connects to the local IP on the specified port.
    public void Connect(int port) {
        try {
            this.Connect(LocalIPAddress, port);
        }
        catch (Exception ex) {
            throw new Exception($"Connection unexpected error: {ex}");
        }
    }

    // Connects to a remote host using a given IP and port.
    // Initializes the network stream upon success.
    public void Connect(string IP, int port) {
        if (this.IsConnected)
            return;

        try {
            Tcpsocket!.Connect(IP, port);
        }
        catch (Exception ex) {
            throw new Exception($"Connection unexpected error: {ex}");
        }

        InitNetworkStream();
    }

    /// <summary>
    /// The duration in seconds to determine if the client is still considered alive 
    /// based on the time of the last received ping.
    /// </summary>
    public int PingTimeoutSeconds { get; set; } = 120;
    public int PingTimeoutDelay{ get; set; } = 60000; //default 60000
    private DateTime? LastPing { get; set; } = null;
    public void UpdateLastPing() => LastPing = DateTime.Now;
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
            throw new Exception("NetworkStream is not initiated");

        var packet = prk.Serialize() + "\n";
        var data = Encoding.UTF8.GetBytes(packet);
        
        try {
            await Tcpstream.WriteAsync(data, 0, data.Length);
            await Tcpstream.FlushAsync();
        }
        catch (IOException ioEx) { // Signal disconnect
            // This happens when the connection is closed/reset
            Disconnect();
            throw new Exception($"NetworkStream connection closed: {ioEx}"); 
        }
        catch (SocketException sockEx) { // Signal disconnect
            Disconnect();
            throw new Exception($"NetworkStream socket error {sockEx}");  
        }
        catch (Exception ex) {
            throw new Exception($"NetworkStream unexpected error: {ex}");
        }
    }

    // Receives a packet asynchronously from the TCP stream.
    public async Task<Packet?> RecvAsync() {
        if (Tcpstream == null)
            throw new Exception("NetworkStream is not initiated");

        try {
            using var reader = new StreamReader(Tcpstream, Encoding.UTF8, leaveOpen: true);
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                return null;

            return Packet.Deserialize(line);
        }
        catch (IOException ioEx) { // Signal disconnect
            // This happens when the connection is closed/reset
            Disconnect();
            throw new Exception($"NetworkStream connection closed: {ioEx}"); 
        }
        catch (SocketException sockEx) { // Signal disconnect
            Disconnect();
            throw new Exception($"NetworkStream socket error {sockEx}");  
        }
        catch (Exception ex) {
            throw new Exception($"NetworkStream unexpected error: {ex}");
        }
    }

    /// <summary>
    /// Disconnects the TCP socket and disposes of its resources.
    /// </summary>
    public void Disconnect() {
        try {
            if (Tcpsocket != null) {
                if (Tcpsocket.Connected) {
                    try {
                        Tcpsocket.Client.Shutdown(SocketShutdown.Both);
                    }
                    catch { /* ignore shutdown errors */ }
                }

                Tcpsocket.Close();
                Tcpsocket.Dispose();
                Tcpsocket = null;
            }

            reader?.Dispose();
            writer?.Dispose();

            Tcpstream?.Close();
            Tcpstream?.Dispose();
            Tcpstream = null;
        }
        catch (Exception ex) {
            throw new Exception($"Disconnect unexpected error: {ex}");
        }
    }

    private bool disposed = false; // to detect redundant calls
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this); // prevent finalizer from running again
    }

    protected virtual void Dispose(bool disposing) {
        if (disposed)
            return;
        disposed = true;

        if (disposing) {
            // Dispose managed resources
            Disconnect();
        }

        // No unmanaged resources directly, so nothing special here
    }

    ~TcpSocket() {
        Dispose(false); // cleanup unmanaged only (but we don't have any here)
    }
}

