using System.Windows;
using WDFChatApplication.Core;
using WDFChatApplication.WDF;

namespace WDFChatApplication;

public class AppIntenalInterface {
    private Application? app { get; set; } = null;

    private CancellationTokenSource cts = new();

    public AppIntenalInterface(Application App) {
        this.app = App;
    }

    [STAThread]
    public void Start() {
        MainAppInterface window = new MainAppInterface();
        this.app!.Run(window);
    }

    ~AppIntenalInterface() {
        cts.Cancel();

        this.app?.Shutdown();
        this.app = null;
    }
}