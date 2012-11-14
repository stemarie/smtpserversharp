using System.Collections.Generic;

namespace SmtpServer.Mailbox
{
    public interface IMailbox
    {
        string Identifier { get; }
        IFolder SentItems { get; }
        IFolder Inbox { get; }
        Dictionary<string, IFolder> Folders { get; }
    }
}
