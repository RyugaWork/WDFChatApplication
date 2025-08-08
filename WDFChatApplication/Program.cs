using System;
using System.Windows;

namespace MyApp {
    class Program {
        [STAThread] // required for WPF
        static void Main(string[] args) {
            Console.WriteLine("Starting WPF window...");

            //var app = new Application();
            //var window = new MainWindow();
            //app.Run(window);
        }
    }
}