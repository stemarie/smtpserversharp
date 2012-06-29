using System;
using EricDaugherty.CSES.Common;

namespace EricDaugherty.CSES.Mailbox
{
	/// <summary>
	/// The folder class defines the interface used to 
	/// interact with a specific folder in a user's mailbox.
	/// </summary>
	/// <remarks>
	/// Incomplete.
	///	</remarks>
	public interface IFolder
	{
		#region Properties

		/// <summary>
		/// The total number of messages in this folder
		/// </summary>
		int MessageCount { get; }

		/// <summary>
		/// The total size of all messages in this folder.
		/// </summary>
		long Size { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Add a message to the current folder.
		/// </summary>
		/// <param name="fromAddress">The FROM address for the email.</param>
		/// <param name="data">The raw message data.</param>
		void addMessage( EmailAddress fromAddress, string data );

		#endregion

		
	}
}
