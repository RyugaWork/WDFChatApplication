using System;
using System.Threading.Tasks;
using System.Windows;

using WDFChatApplication.Core;

namespace WDFChatApplication;

class Program {
    //[STAThread] // required for WPF
    static async Task Main(string[] args) {
        var client = new Client();
        await client.ConnectAsync(); // await the async method

        while (true) { }

        Console.WriteLine("Starting WPF window...");

        //var app = new Application();
        //var window = new MainWindow();
        //app.Run(window);
    }
}