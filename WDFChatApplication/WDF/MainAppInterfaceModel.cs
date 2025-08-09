using Net3.Model;
using Net3.Packets;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Interop;
using WDFChatApplication.Core;

namespace WDFChatApplication.WDF;

public class MainAppInterfaceModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new(name));

    public ICommand ConnectCommand { get; set; }
    public ICommand DisconnectCommand { get; set; }
    public ICommand SendCommand { get; set; }

    public ObservableCollection<Message> Messages { get; set; } = new();

    private string _message = "";
    public string Message {
        get => _message;
        set {
            _message = value;
            OnPropertyChanged(nameof(Message));
        }
    }

    private string _name = "";
    public string Username {
        get => _name;
        set {
            _name = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    public Client? client { get; set; } = null;
    public MainAppInterfaceModel() {
        ConnectCommand = new Relay(async o => await Connect());
        DisconnectCommand = new Relay(o => Disconnect());
        SendCommand = new Relay(o => SendMsg());
    }

    public async Task Connect() {
        if (client?.IsConnected == true)
            return;

        client = new Client();
        client.OnMessageReceived += async (msg) => await AddMessage(msg);

        //! Why client not recv hello packet after reconnect?
        Messages.Clear();

        await client.ConnectAsync();
    }

    public void Disconnect() {

        client?.Disconnect();
        client = null;
    }

    public async void SendMsg() {
        if (client == null || Message == "")
            return;

        await client.SendAsync(new Tcp_Mess_Pck {
            sender = Username,
            text = Message,
        });

        Message = "";
    }

    public async Task AddMessage(Message msg) {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Messages.Add(msg);
        });
    }
}