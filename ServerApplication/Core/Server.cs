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

    public void Start() {
        _ = Task.Run(() => Listener(cts.Token));
    }

    public void Listener(CancellationToken token) {
        Tcplistener!.Start();
        Console.WriteLine($"Server started on {TcpSocket.LocalIPAddress}:{ListeningPort}");
        while (!token.IsCancellationRequested && Tcplistener != null) {
            var client = new Client(Tcplistener.AcceptTcpClient());
            client.OnCloseConnectionAction += RemoveClient;
            client.OnMessageReceivedAction += OnMessageReceivedAction;

            Console.WriteLine("Client Connected!");

            Clients!.Add(client);
        }
    }

    private async Task OnMessageReceivedAction(Message msgprk) {
        Console.WriteLine(".C.");
        using (var repo = new ChatRepository(new ChatDbContext()))
        await repo.AddMessageAsync(msgprk);
        Console.WriteLine("...");
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
