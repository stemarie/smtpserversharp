using System;

namespace EricDaugherty.CSES.Common
{
	/// <summary>
	/// Indicates that an email address is not valid.
	/// </summary>
	/// <remarks>
	/// Thrown by the EmailAddress class when part of all of the email
	/// address being set is invalid.
	/// </remarks>
	public class InvalidEmailAddressException : ApplicationException
	{
		#region Constructors
		
		/// <summary>
		/// Creates a new Exception with a user-displayable message.
		/// </summary>
		public InvalidEmailAddressException( string userMessage ) : base( userMessage )
		{
		}
		
		#endregion
	}
}
