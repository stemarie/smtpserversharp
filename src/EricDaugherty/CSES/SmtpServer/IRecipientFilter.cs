using System;
using EricDaugherty.CSES.Common;

namespace EricDaugherty.CSES.SmtpServer
{
	/// <summary>
	/// Allows filtering of incoming email by recipient address.
	/// </summary>
	/// <remarks>
	/// The IRecipientFilter defines the interface that classes must implement
	/// if they wish to provide custom filtering of inbound messages.  If this
	/// method returns false, the message will be rejected by the SMTP server.  The 
	/// context (SMTPContext) parameter provides information about the sender 
	/// (including the FROM email address)
	/// while the recipient parameter provides the TO email address.
	/// </remarks>
	public interface IRecipientFilter
	{
		/// <summary>
		/// Tests the recipient EmailAddress to determine if
		/// it should be accepted as a valid address.
		/// </summary>
		/// <returns>True if the address should be accepted.</returns>
		bool AcceptRecipient( SMTPContext context, EmailAddress recipient );
	}
}
