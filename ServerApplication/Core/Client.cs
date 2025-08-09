using Net3;
using Net3.Packets;
using Net3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Core;

public class Client {
    private TcpSocket? socket { get; set; } = null;
    private CancellationTokenSource cts = new();

    public Action<Client>? OnCloseConnectionAction { get; set; } = null;
    public event Func<Message, Task>? OnMessageReceivedAction;

    public Client(TcpClient socket) {
        this.socket = new TcpSocket(socket);
        this.cts = new CancellationTokenSource();

        _ = Task.Run(() => HandshakeAsync(cts.Token));
        _ = Task.Run(() => TimeOutCheckAsync(cts.Token));
    }

    public async Task SendAsync(Packet packet) {
        Console.WriteLine($"Send >> {packet!.Serialize()}");
        await this.socket!.SendAsync(packet);
    }

    private async Task TimeOutCheckAsync(CancellationToken token) {
        while (!token.IsCancellationRequested && this.socket!.IsConnected) {
            await this.socket!.SendAsync(new Packet("Ping"));
            if (!this.socket.IsAlive) {
                OnCloseConnectionAction!.Invoke(this);
                break;
            }
            await Task.Delay(this.socket.PingTimeoutDelay, token);
        }
    }

    public async Task HandshakeAsync(CancellationToken token) {
        while (!token.IsCancellationRequested && this.socket!.IsConnected) {
            var packet = await this.socket.RecvAsync();
            Console.WriteLine($"Recv >> {packet!.Serialize()}");

            if (packet != null) {
                switch (packet.type) {
                    case "Connect": {

                        await this.socket.SendAsync(new Tcp_Mess_Pck { sender = "Server", text = "Hello" });

                        break;
                    }

                    case "Message": {
                        var msg = packet as Tcp_Mess_Pck;
                        var message = new Message {
                            sender = msg!.sender,
                            text = msg!.text,
                        };
                        await this.OnMessageReceivedAction!.Invoke(message);

                        break;
                    }

                    case "Ping": {
                        this.socket.UpdateLastPing();
                        var pingMs = (DateTime.Now - packet.timestamp!.Value).TotalMilliseconds;
                        Console.WriteLine($"Ping: {pingMs} ms");
                        break;
                    }

                    default:
                        break;
                }
            }
        }
    }

    public void Disconnect() {
        cts.Cancel();
        this.socket!.Disconnect();
        this.socket = null;
    }

    ~Client() {
        this.Disconnect();
    }
}
