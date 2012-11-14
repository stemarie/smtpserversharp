using System.Collections;

namespace SmtpServer.Processor
{
    public class CommandMessageAttachment
    {
        public string Header { get; set; }

        public string Data { get; set; }

        public Hashtable Headers { get; set; }
    }
}