using System.Collections;

namespace src.SmtpServer
{
    /// <summary>
	/// Provides a memory based IMessageSpool.
	/// </summary>
	public class MemoryMessageSpool : IMessageSpool
	{
		#region Variables
		
		private Queue queue;
		
		#endregion
		
		/// <summary>
		/// Initializes the queue.
		/// </summary>
		public MemoryMessageSpool()
		{
			queue = new Queue();
		}
		
		#region IMessageSpool methods
		
		/// <summary>
		/// Addes the message to the in memory queue.
		/// </summary>
		/// <param name='message'>The message to queue.</param>
		public virtual bool SpoolMessage(SMTPMessage message)
		{
			queue.Enqueue( message );
			return true;
		}
		
		#endregion
		
		#region Public Methods
		
		/// <summary>Returns the oldest message in the spool.</summary>
		public virtual SMTPMessage NextMessage()
		{
			return (SMTPMessage) queue.Dequeue();
		}
		
		/// <summary>Removes any messages from the spool.</summary>
		public virtual void ClearSpool()
		{
			queue.Clear();
		}
		
		#endregion
	}
}
