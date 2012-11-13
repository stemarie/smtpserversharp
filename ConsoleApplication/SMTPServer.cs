using System;
using System.Net;
using System.Threading;

namespace ConsoleApplication
{
    public class SMTPServer
    {
        public void RunServer()
        {
            MailListener listener = null;

            do
            {
                Console.WriteLine("New MailListener started");
                listener = new MailListener(this, IPAddress.Loopback, 25)
                {
                    OutputToFile = true
                };
                listener.Start();
                while (listener.IsThreadAlive)
                {
                    Thread.Sleep(500);
                }
            } while (listener != null);

        }
    }
}