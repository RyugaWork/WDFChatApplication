using Microsoft.EntityFrameworkCore;
using Net3;
using Net3.Model;
using Net3.Packets;
using ServerApplication.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Core;
public class Server {
    private CancellationTokenSource cts = new();
    private TcpListener? Tcplistener { get; set; } = null;
    private List<Client>? Clients { get; set; } = null;

    public int ListeningPort => ((IPEndPoint)Tcplistener!.LocalEndpoint).Port;
    
    public Server(int port = 0) {
        this.cts = new CancellationTokenSource();
        this.Tcplistener = new TcpListener(IPAddress.Parse(TcpSocket.LocalIPAddress), port);
        this.Clients = new List<Client>();
    }

    public async Task Start() {
        using (var repo = new ChatRepository(new ChatDbContext()))
            await repo.Clear();

        _ = Task.Run(() => Listener(cts.Token));
    }

    public void Listener(CancellationToken token) {
        Tcplistener!.Start();
        Console.WriteLine($"Server started on {TcpSocket.LocalIPAddress}:{ListeningPort}");
        while (!token.IsCancellationRequested && Tcplistener != null) {
            var client = new Client(Tcplistener.AcceptTcpClient());
            client.OnCloseConnectionAction += RemoveClient;
            client.OnMessageReceivedAction += OnMessageReceivedAction;
            client.OnLoadMessageAction += OnLoadMessageAction;

            Console.WriteLine("Client Connected!");

            Clients!.Add(client);
        }
    }

    private async Task OnMessageReceivedAction(Message msgprk) {
        using (var repo = new ChatRepository(new ChatDbContext()))
        await repo.AddMessageAsync(new Message {
            Id = msgprk.Id,
            sender = msgprk.sender,
            text = msgprk.text,
            timestamp = msgprk.timestamp,
        });

        foreach (var Client in Clients!.ToList()) {
            await Client.SendAsync(new Tcp_Mess_Pck {
                sender = msgprk.sender,
                text = msgprk.text,
                timestamp = msgprk.timestamp,
            });
        }
    }

    private async Task<List<Tcp_Mess_Pck>> OnLoadMessageAction() {
        using var db = new ChatDbContext();
        return await db.Messages
            .OrderBy(m => m.timestamp)
            .Select(m => new Tcp_Mess_Pck {
                sender = m.sender,
                text = m.text,
                timestamp = m.timestamp
            }).Take(10).ToListAsync();
    }

    private void RemoveClient(Client client) {
        Clients!.Remove(client);
    }

    ~Server() {
        cts.Cancel();
        Tcplistener?.Stop();
        Tcplistener = null;
    }
}
