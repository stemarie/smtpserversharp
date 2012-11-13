using System;

namespace ConsoleApplication
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            SMTPServer server = new SMTPServer();
            server.RunServer();
        }
    }
}
