using System.Collections.Generic;

namespace src.Mailbox
{
    public class Mailbox : IMailbox
    {
        public string Identifier { get; private set; }

        public IFolder SentItems { get; private set; }

        public IFolder Inbox { get; private set; }

        public Dictionary<string, IFolder> Folders { get; private set; }

        private Mailbox(string identifier, IFolder inbox, IFolder sentItems, IEnumerable<IFolder> folders)
        {
            Identifier = identifier;
            Inbox = inbox;
            SentItems = sentItems;
            Folders = new Dictionary<string, IFolder>();
            foreach (IFolder folder in folders)
            {
                Folders.Add(folder.Name, folder);
            }
        }
    }
}