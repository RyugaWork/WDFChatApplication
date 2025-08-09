using Net3;
using Net3.Model;
using Net3.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WDFChatApplication.Core;
public class Client {
    private TcpSocket? socket { get; set; } = null;
    private CancellationTokenSource cts = new();

    public event Action<Message>? OnMessageReceived;

    public Client() {
        this.socket = new TcpSocket();
    }

    public bool IsConnected => this.socket!.IsConnected;

    public async Task SendAsync(Packet packet) {
        Console.WriteLine($"Send >> {packet!.Serialize()}");
        await this.socket!.SendAsync(packet);
    }

    private async Task OnConnect() {
        this.socket!.InitNetworkStream();

        await this.socket!.SendAsync(new Packet("Connect"));
    }

    public async Task ConnectAsync() {
        if (this.socket!.IsConnected)
            return;

        this.socket.Connect(5000);
        Console.WriteLine($"Client Connected to {TcpSocket.LocalIPAddress}:{5000}");

        await OnConnect();

        _ = Task.Run(() => HandshakeAsync(cts.Token));
        _ = Task.Run(() => TimeOutCheckAsync(cts.Token));
    }

    private async Task TimeOutCheckAsync(CancellationToken token) {
        while (!token.IsCancellationRequested && this.socket!.IsConnected) {
            if (!this.socket.IsAlive) {
                this.socket.Disconnect();
                break;
            }
            await Task.Delay(this.socket.PingTimeoutDelay, token);
        }
    }

    private async Task HandshakeAsync(CancellationToken token) {
        while (!token.IsCancellationRequested && this.socket!.IsConnected) {
            var packet = await this.socket.RecvAsync();
            Console.WriteLine($"Recv >> {packet!.Serialize()}");

            if (packet != null) {
                switch (packet.type) {
                    case "Message": {
                        var msg = packet as Tcp_Mess_Pck;
                        OnMessageReceived!.Invoke(new Message {
                            text = msg!.text,
                            sender = msg!.sender,
                            timestamp = msg!.timestamp
                        });
                        break;
                    }

                    case "Ping": {
                        this.socket!.UpdateLastPing();
                        await this.socket.SendAsync(new Packet("Ping"));
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
        Disconnect();
    }
}