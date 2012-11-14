using System.Linq;
using SmtpServer;
using SmtpServer.Processor;

namespace MessageSpool.Command
{
    public class CommandMessageSpool : IMessageSpool
    {
        private readonly ICommandMessageProcessor _processor;

        public CommandMessageSpool(ICommandMessageProcessor processor)
        {
            _processor = processor;
        }

        public bool SpoolMessage(SMTPMessage message)
        {
            return
                _processor.Process(
                    new CommandMessage(
                        message.Data, message.FromAddress.Address,
                        message.ToAddresses.Select(a => a.Address).AsEnumerable(),
                        message.MessageParts.Select(a => new CommandMessageAttachment
                                                             {
                                                                 Data = a.BodyData,
                                                                 Header = a.HeaderData,
                                                                 Headers = a.Headers
                                                             }), message.Headers));
        }

        public SMTPMessage NextMessage()
        {
            return null;
        }

        public void ClearSpool()
        { }

        public void Dispose()
        { }
    }
}
