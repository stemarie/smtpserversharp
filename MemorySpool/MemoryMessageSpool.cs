using System.Collections.Concurrent;

namespace SmtpServer.MessageSpool.Memory
{
    /// <summary>
    /// Provides a memory based IMessageSpool.
    /// </summary>
    public class MemoryMessageSpool : IMessageSpool
    {
        #region Variables

        private ConcurrentQueue<SMTPMessage> _queue;

        #endregion

        /// <summary>
        /// Initializes the queue.
        /// </summary>
        public MemoryMessageSpool()
        {
            _queue = new ConcurrentQueue<SMTPMessage>();
        }

        #region IMessageSpool methods

        /// <summary>
        /// Adds the message to the in memory queue.
        /// </summary>
        /// <param name='message'>The message to queue.</param>
        public virtual bool SpoolMessage(SMTPMessage message)
        {
            _queue.Enqueue(message);
            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>Returns the oldest message in the spool.</summary>
        public virtual SMTPMessage NextMessage()
        {
            SMTPMessage message;
            return _queue.TryDequeue(out message) ? message : null;
        }

        /// <summary>Removes any messages from the spool.</summary>
        public virtual void ClearSpool()
        {
            _queue = new ConcurrentQueue<SMTPMessage>();
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        { }
    }
}
