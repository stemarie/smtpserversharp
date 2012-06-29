using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using log4net;
using log4net.Config;
using src.SmtpServer;

namespace src_test.SmtpServer
{
    [TestFixture]
    // <summary>
    // Tests the SMTPProcessor class.
    // </summary>
    public class SMTPProcessorTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            messageSpool.ClearSpool();
            var threadlistener = 
                new Thread(Listener)
                    {
                        IsBackground = true
                    };
            threadlistener.Start();
            // Block for a second to make sure the socket gets started.
            Thread.Sleep(1000);
        }

        [TearDown]
        public void Teardown()
        {
            listener.Stop();
        }

        #endregion

        private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 9900);
        private TcpListener listener;
        private readonly MemoryMessageSpool messageSpool;

        public SMTPProcessorTest()
        {
            LogManager.ResetConfiguration();
            BasicConfigurator.Configure();
            messageSpool = new MemoryMessageSpool();
        }

        private void Listener()
        {
            try
            {
                var processor = new SMTPProcessor("testdomain.com", messageSpool);

                listener = new TcpListener(endPoint);
                listener.Start();
                Console.WriteLine("Socket listener started...");
                Socket clientSocket = listener.AcceptSocket();
                processor.ProcessConnection(clientSocket);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception in Listener: " + exception);
                Console.WriteLine(exception.StackTrace);
            }
        }

        private Socket Connect()
        {
            Console.WriteLine("Connecting...");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            // Read Welcome Message
            string line = ReadLine(socket);
            Assert.IsTrue(line.StartsWith("220"), "Welcome Message not recieved.");

            // Helo
            WriteLine(socket, "helo nunittestdomain.com");
            line = ReadLine(socket);
            Assert.IsTrue(line.Equals("250 testdomain.com"), "Helo response incorrect.");

            return socket;
        }

        private void Disconnect(Socket socket)
        {
            // Quit
            WriteLine(socket, "quit");
            string line = ReadLine(socket);
            Assert.IsTrue(line.StartsWith("221"), "Quit ack incorrect.");

            socket.Close();
        }

        private void CheckResponse(Socket socket, string command, string responseCode)
        {
            String line = WriteAndRead(socket, command);
            Assert.IsTrue(line.StartsWith(responseCode),
                          command + " did not result in the correct response code: " + responseCode);
        }

        /// <summary>Helper method to combine a write and a read.</summary>
        public string WriteAndRead(Socket socket, string data)
        {
            WriteLine(socket, data);
            return ReadLine(socket);
        }

        /// <summary>
        /// Writes the string to the socket as an entire line.  This
        /// method will append the end of line characters, so the data
        /// parameter should not contain them.
        /// </summary>
        /// <param name="socket">The socket to write to.</param>
        /// <param name="data">The data to write the the client</param>
        public void WriteLine(Socket socket, string data)
        {
            Console.WriteLine("Wrote: " + data);
            socket.Send(Encoding.ASCII.GetBytes(data + "\r\n"));
        }

        /// <summary>
        /// Reads an entire line from the socket.  This method
        /// will block until an entire line has been read.
        /// </summary>
        /// <param name="socket"></param>
        public String ReadLine(Socket socket)
        {
            var inputBuffer = new byte[80];
            var inputString = new StringBuilder();
            String currentValue;

            // Read from the socket until an entire line has been read.			
            do
            {
                // Read the input data.
                int count = socket.Receive(inputBuffer);

                inputString.Append(Encoding.ASCII.GetString(inputBuffer, 0, count));
                currentValue = inputString.ToString();
            } while (currentValue.IndexOf("\r\n", StringComparison.Ordinal) == -1);

            // Strip off EOL.
            currentValue = currentValue.Remove(currentValue.IndexOf("\r\n", StringComparison.Ordinal), 2);

            Console.WriteLine("Read Line: " + currentValue);
            return currentValue;
        }

        [Test]
        public void BasicConnectionTest()
        {
            Socket socket = Connect();
            Disconnect(socket);
        }

        [Test]
        public void DataTest()
        {
            Socket socket = Connect();
            CheckResponse(socket, "mail from:<user name@domain123.com>", "250");
            CheckResponse(socket, "rcpt to:<username@testdomain.com>", "250");
            CheckResponse(socket, "data", "354");

            WriteLine(socket, "Date: Tue, 4 Nov 2003 10:13:27 -0600 (CST)");
            WriteLine(socket, "Subject: Test");
            WriteLine(socket, "");
            WriteLine(socket, "Message Body.");

            CheckResponse(socket, ".", "250");

            Disconnect(socket);

            SMTPMessage message = messageSpool.NextMessage();

            Console.WriteLine("Message Recieved: ");
            Console.Write(message.Data);
        }

        [Test]
        public void MailFromAddressParsingTest()
        {
            Socket socket = Connect();

            CheckResponse(socket, "mail from:username@domain.com", "451");
            CheckResponse(socket, "mail from:<user name@domain123.com>", "250");

            Disconnect(socket);
        }

        [Test]
        public void RcptToAddressParsingTest()
        {
            Socket socket = Connect();

            CheckResponse(socket, "mail from:<user name@domain123.com>", "250");
            CheckResponse(socket, "rcpt to:username@domain.com", "451");
            CheckResponse(socket, "rcpt to:<user name@domain123.com>", "550");
            CheckResponse(socket, "rcpt to:<username@domain.com>", "550");
            CheckResponse(socket, "rcpt to:<username@testdomain.com>", "250");
            CheckResponse(socket, "rcpt to:<user_100@testdomain.com>", "250");

            Disconnect(socket);
        }
    }
}