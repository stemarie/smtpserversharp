using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConsoleApplication
{
    public class MailListener : TcpListener
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _thread = null;
        private SMTPServer _owner;
        private const string Subject = "Subject: ";
        private const string From = "From: ";
        private const string To = "To: ";
        private const string MimeVersion = "MIME-Version: ";
        private const string Date = "Date: ";
        private const string ContentType = "Content-Type: ";
        private const string ContentTransferEncoding = "Content-Transfer-Encoding: ";
        private const string ResponseOk = "250 OK";
        private const string CommandQuit = "QUIT";
        private const string CommandData = "DATA";
        private const string ResponseStartInput = "354 Start input, end data with <CRLF>.<CRLF>";
        private const string ResponseServer = "220 localhost -- Fake proxy server";

        public MailListener(SMTPServer aOwner, IPAddress localaddr, int port)
            : base(localaddr, port)
        {
            _owner = aOwner;
        }

        public new void Start()
        {
            base.Start();

            _client = AcceptTcpClient();
            _client.ReceiveTimeout = 5000;
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream) { NewLine = "\r\n", AutoFlush = true };

            _thread = new Thread(RunThread);
            _thread.Start();
        }

        protected void RunThread()
        {
            _writer.WriteLine(ResponseServer);

            try
            {
                while (_reader != null)
                {
                    string line = _reader.ReadLine();
                    Console.Error.WriteLine("Read line {0}", line);

                    switch (line)
                    {
                        case CommandData:
                            _writer.WriteLine(ResponseStartInput);
                            StringBuilder data = new StringBuilder();
                            String subject = "";
                            string from = "";
                            string to = "";
                            string mimeVersion = "";
                            string date = "";
                            string contentType = "";
                            string contentTransferEncoding = "";

                            line = _reader.ReadLine();

                            while (line != null && line != ".")
                            {
                                if (line.StartsWith(Subject))
                                {
                                    subject = line.Substring(Subject.Length);
                                }
                                else if (line.StartsWith(From))
                                {
                                    from = line.Substring(From.Length);
                                }
                                else if (line.StartsWith(To))
                                {
                                    to = line.Substring(To.Length);
                                }
                                else if (line.StartsWith(MimeVersion))
                                {
                                    mimeVersion = line.Substring(MimeVersion.Length);
                                }
                                else if (line.StartsWith(Date))
                                {
                                    date = line.Substring(Date.Length);
                                }
                                else if (line.StartsWith(ContentType))
                                {
                                    contentType = line.Substring(ContentType.Length);
                                }
                                else if (line.StartsWith(ContentTransferEncoding))
                                {
                                    contentTransferEncoding = line.Substring(ContentTransferEncoding.Length);
                                }
                                else
                                {
                                    data.AppendLine(line);
                                }

                                line = _reader.ReadLine();
                            }

                            String message = data.ToString();

                            WriteMessage(from, to, subject, message, contentType, contentTransferEncoding);

                            _writer.WriteLine(ResponseOk);
                            break;

                        case CommandQuit:
                            _writer.WriteLine(ResponseOk);
                            _reader = null;
                            break;

                        default:
                            _writer.WriteLine(ResponseOk);
                            break;
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Connection lost.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                _client.Close();
                Stop();
            }
        }

        private static string DecodeQuotedPrintable(string input)
        {
            var occurences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match m in matches)
            {
                byte[] bytes = new byte[m.Value.Length / 3];
                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = m.Value.Substring(i * 3 + 1, 2);
                    int iHex = Convert.ToInt32(hex, 16);
                    bytes[i] = Convert.ToByte(iHex);
                }
                input = input.Replace(m.Value, Encoding.Default.GetString(bytes));
            }
            return input.Replace("=\r\n", "");
        }

        private void WriteMessage(string from, string to, string subject, string message, string contentType,
                                  string transferEncoding)
        {
            if (transferEncoding == "quoted-printable")
            {
                message = DecodeQuotedPrintable(message);
            }

            if (OutputToFile)
            {
                string header =
                    string.Format(
                        "<strong>FROM: </strong>{0}<br/><strong>TO: </strong>{1}<br/><strong>SUBJECT: </strong>{2}<br/><br/>",
                        new object[] { from, to, subject });
                string docText = string.Format("<html><body>{0}{1}</body></html>", header, message);

                // Create a file to write to.
                string path = string.Format("mail_{0}.html", DateTime.Now.ToFileTimeUtc());
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write(docText);
                }
            }

            Console.Error.WriteLine(
                "===============================================================================");
            Console.Error.WriteLine("Received ­email");
            Console.Error.WriteLine("Type: " + contentType);
            Console.Error.WriteLine("Encoding: " + transferEncoding);
            Console.Error.WriteLine("From: " + from);
            Console.Error.WriteLine("To: " + to);
            Console.Error.WriteLine("Subject: " + subject);
            Console.Error.WriteLine(
                "-------------------------------------------------------------------------------");
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(
                "===============================================================================");
            Console.Error.WriteLine("");
        }

        public bool OutputToFile { get; set; }

        public bool IsThreadAlive
        {
            get { return _thread.IsAlive; }
        }
    }
}