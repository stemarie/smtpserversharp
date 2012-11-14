using System.Collections;
using System.Collections.Generic;

namespace SmtpServer.Processor
{
    public interface ICommandMessage
    {
        string Data { get; }
        string From { get; }
        IEnumerable<string> Addresses { get; }
        IEnumerable<CommandMessageAttachment> Parts { get; }
        Hashtable Headers { get; }
    }
}