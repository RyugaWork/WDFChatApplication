using System;
using System.Threading.Tasks;
using System.Windows;

using WDFChatApplication.Core;

namespace WDFChatApplication;

public class Program {
    [STAThread] // required for WPF
    static void Main(string[] args) {
        var app = new Application();

        var AII = new AppIntenalInterface(app);

        AII.Start();
    }
}