using System;
using System.Windows;

using ServerApplication.Core;
namespace ServerApplication;
class Program {
    static void Main(string[] args) {

        var Server = new Server(5000);
        Server.Start();

        while (true) { }

        //Console.WriteLine("End");
    }
}
