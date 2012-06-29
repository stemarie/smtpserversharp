using System.Collections.Generic;

namespace src.SmtpServer
{
    /// <summary>
    /// Provides a memory based IMessageSpool.
    /// </summary>
    public class MemoryMessageSpool : IMessageSpool
    {
        #region Variables

        private readonly Queue<SMTPMessage> queue;

        #endregion

        /// <summary>
        /// Initializes the queue.
        /// </summary>
        public MemoryMessageSpool()
        {
            queue = new Queue<SMTPMessage>();
        }

        #region IMessageSpool methods

        /// <summary>
        /// Addes the message to the in memory queue.
        /// </summary>
        /// <param name='message'>The message to queue.</param>
        public virtual bool SpoolMessage(SMTPMessage message)
        {
            queue.Enqueue(message);
            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>Returns the oldest message in the spool.</summary>
        public virtual SMTPMessage NextMessage()
        {
            return queue.Dequeue();
        }

        /// <summary>Removes any messages from the spool.</summary>
        public virtual void ClearSpool()
        {
            queue.Clear();
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
