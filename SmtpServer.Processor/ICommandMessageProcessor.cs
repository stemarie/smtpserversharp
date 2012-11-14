namespace SmtpServer.Processor
{
    public interface ICommandMessageProcessor
    {
        bool Process(CommandMessage commandMessage);
    }
}