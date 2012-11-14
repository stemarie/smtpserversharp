using System.Collections;
using System.Collections.Generic;

namespace SmtpServer.Processor
{
    public class CommandMessage : ICommandMessage
    {
        public CommandMessage(string data, string from, IEnumerable<string> addresses, IEnumerable<CommandMessageAttachment> attachments, Hashtable headers)
        {
            Data = data;
            From = from;
            Addresses = addresses;
            Parts = attachments;
            Headers = headers;
        }

        public string Data { get; private set; }

        public string From { get; private set; }

        public IEnumerable<string> Addresses { get; private set; }

        public IEnumerable<CommandMessageAttachment> Parts { get; private set; }

        public Hashtable Headers { get; private set; }
    }
}