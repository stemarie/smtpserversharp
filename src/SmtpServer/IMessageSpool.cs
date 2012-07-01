using System;

namespace src.SmtpServer
{
    /// <summary>
    /// Provides an interface for the SMTPProcessor to use
    /// to store incoming messages.
    /// </summary>
    /// <remarks>
    /// The IMessageSpool defines the interface that classes must implement
    /// if they wish to provide custom message spooling of inbound messages.  If this
    /// method returns false, the message will be rejected by the SMTP server.  Once this
    /// method returns true, the message is considered delivered and the remote sender
    /// will be notified.  
    /// </remarks>
    public interface IMessageSpool : IDisposable
    {
        /// <summary>
        /// Stores the specified message using the appropriate mechanism.  
        /// Individual implementations
        /// may vary.  Once this method returns with a value of true,
        /// the message is considered delivered.
        /// </summary>		
        bool SpoolMessage(SMTPMessage message);

        /// <summary>Returns the oldest message in the spool.</summary>
        SMTPMessage NextMessage();

        /// <summary>Removes any messages from the spool.</summary>
        void ClearSpool();
    }
}
