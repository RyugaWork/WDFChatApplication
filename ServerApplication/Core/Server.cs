using Net3;
using Net3.Model;
using Net3.Packets;
using ServerApplication.Core.SQL;
using ServerApplication.Core.SQL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        
    private async Task OnMessageReceivedAction(Message MessagePacket) {
        //using var db = new ChatDbContext();
        //Console.WriteLine("***");
        //await db.Messages.AddAsync(new Message {
        //    ConversationId = MessagePacket.ConversationId, // required FK
        //    Sender = MessagePacket.Sender,
        //    Text = MessagePacket.Text,
        //    Time = MessagePacket.Time,
        //});
        //await db.SaveChangesAsync();


        Console.WriteLine("***");

        using (var context = new ChatDbContext())
        using (var repo = new ChatRepository(context)) {
            // Add a message
            var message = new Message {
                //ConversationId = MessagePacket.ConversationId,
                //Sender = someUser,
                //Receiver = otherUser,
                Text = MessagePacket.Text,
                Time = MessagePacket.Time
            };

            Console.WriteLine(".^.");
            await repo.AddMessageAsync(message);

        }


        Console.WriteLine("...");
        // Broadcast to all clients
        foreach (var Client in Clients!.ToList()) {
            await Client.SendAsync(new Tcp_Mess_Pck {
                sender = MessagePacket.From,
                text = MessagePacket.Text
            });
        }
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
