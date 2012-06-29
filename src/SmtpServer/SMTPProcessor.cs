using src.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace src.SmtpServer
{
    /// <summary>
	/// SMTPProcessor handles a single SMTP client connection.  This
	/// class provides an implementation of the RFC821 specification.
	/// </summary>
	/// <remarks>
	/// 	Created by: Eric Daugherty
	/// </remarks>
	public class SMTPProcessor
	{
		#region Constants
		
		// Command codes
		/// <summary>HELO Command</summary>
		public const int COMMAND_HELO = 0;
		/// <summary>RSET Command</summary>
	    public const int COMMAND_RSET = 1;
		/// <summary>NOOP Command</summary>
	    public const int COMMAND_NOOP = 2;
		/// <summary>QUIT Command</summary>
	    public const int COMMAND_QUIT = 3;
		/// <summary>MAIL FROM Command</summary>
	    public const int COMMAND_MAIL = 4;
		/// <summary>RCPT TO Command</summary>
	    public const int COMMAND_RCPT = 5;
		/// <summary>DATA Comand</summary>
	    public const int COMMAND_DATA = 6;

		// Messages
		private const string MESSAGE_DEFAULT_WELCOME = "220 {0} Welcome to Eric Daugherty's C# SMTP Server.";
		private const string MESSAGE_DEFAULT_HELO_RESPONSE = "250 {0}";
		private const string MESSAGE_OK = "250 OK";
		private const string MESSAGE_START_DATA = "354 Start mail input; end with <CRLF>.<CRLF>";
		private const string MESSAGE_GOODBYE = "221 Goodbye.";

		private const string MESSAGE_UNKNOWN_COMMAND = "500 Command Unrecognized.";
		private const string MESSAGE_INVALID_COMMAND_ORDER = "503 Command not allowed here.";
		private const string MESSAGE_INVALID_ARGUMENT_COUNT = "501 Incorrect number of arguments.";
		
		private const string MESSAGE_INVALID_ADDRESS = "451 Address is invalid.";
		private const string MESSAGE_UNKNOWN_USER = "550 User does not exist.";
		
		private const string MESSAGE_SYSTEM_ERROR = "554 Transaction failed.";
		
		// Regular Expressions
		private static readonly Regex ADDRESS_REGEX = new Regex( "<.+@.+>", RegexOptions.IgnoreCase );
		
		#endregion
		
		#region Variables
		
		/// <summary>
		/// Every connection will be assigned a unique id to 
		/// provide consistent log output and tracking.
		/// </summary>
		private long connectionId;
		
		/// <summary>Determines which recipients to accept for delivery.</summary>
		private IRecipientFilter recipientFilter;
		
		/// <summary>Incoming Message spool</summary>
		private IMessageSpool messageSpool;

		/// <summary>Domain name for this server.</summary>
		private string domain;

		/// <summary>The message to display to the client when they first connect.</summary>
		private string welcomeMessage;
		
		/// <summary>The response to the HELO command.</summary>
		private string heloResponse;		
		
		/// <summary>Default Logger</summary>
		private static ILog log = LogManager.GetLogger( typeof( SMTPProcessor ) );
		
		#endregion
		
		#region Constructors
				
		/// <summary>
		/// Initializes the SMTPProcessor with the appropriate 
		/// interface implementations.  This allows the relay and
		/// delivery behaviour of the SMTPProcessor to be defined
		/// by the specific server.
		/// </summary>
		/// <param name="domain">
		/// The domain name this server handles mail for.  This does not have to
		/// be a valid domain name, but it will be included in the Welcome Message
		/// and HELO response.
		/// </param>
		public SMTPProcessor( string domain )
		{
			Initialize( domain );
			
			// Initialize default Interface implementations.
			recipientFilter = new LocalRecipientFilter( domain );
			messageSpool = new MemoryMessageSpool();
		}

		/// <summary>
		/// Initializes the SMTPProcessor with the appropriate 
		/// interface implementations.  This allows the relay and
		/// delivery behaviour of the SMTPProcessor to be defined
		/// by the specific server.
		/// </summary>
		/// <param name="domain">
		/// The domain name this server handles mail for.  This does not have to
		/// be a valid domain name, but it will be included in the Welcome Message
		/// and HELO response.
		/// </param>
		/// <param name="recipientFilter">
		/// The IRecipientFilter implementation is responsible for 
		/// filtering the recipient addresses to determine which ones
		/// to accept for delivery.
		/// </param>
		public SMTPProcessor( string domain, IRecipientFilter recipientFilter )
		{
			Initialize( domain );
						
			this.recipientFilter = recipientFilter;
			messageSpool = new MemoryMessageSpool();
		}
		
		/// <summary>
		/// Initializes the SMTPProcessor with the appropriate 
		/// interface implementations.  This allows the relay and
		/// delivery behaviour of the SMTPProcessor to be defined
		/// by the specific server.
		/// </summary>
		/// <param name="domain">
		/// The domain name this server handles mail for.  This does not have to
		/// be a valid domain name, but it will be included in the Welcome Message
		/// and HELO response.
		/// </param>
		/// <param name="messageSpool">
		/// The IRecipientFilter implementation is responsible for 
		/// filtering the recipient addresses to determine which ones
		/// to accept for delivery.
		/// </param>
		public SMTPProcessor( string domain, IMessageSpool messageSpool )
		{
			Initialize( domain );
						
			recipientFilter = new LocalRecipientFilter( domain );
			this.messageSpool = messageSpool;
		}		

		/// <summary>
		/// Initializes the SMTPProcessor with the appropriate 
		/// interface implementations.  This allows the relay and
		/// delivery behaviour of the SMTPProcessor to be defined
		/// by the specific server.
		/// </summary>
		/// <param name="domain">
		/// The domain name this server handles mail for.  This does not have to
		/// be a valid domain name, but it will be included in the Welcome Message
		/// and HELO response.
		/// </param>
		/// <param name="recipientFilter">
		/// The IRecipientFilter implementation is responsible for 
		/// filtering the recipient addresses to determine which ones
		/// to accept for delivery.
		/// </param>
		/// <param name="messageSpool">
		/// The IMessageSpool implementation is responsible for 
		/// spooling the inbound message once it has been recieved from the sender.
		/// </param>
		public SMTPProcessor( string domain, IRecipientFilter recipientFilter, IMessageSpool messageSpool )
		{
			Initialize( domain );
						
			this.recipientFilter = recipientFilter;
			this.messageSpool = messageSpool;
		}
		
		/// <summary>
		/// Provides common initialization logic for the constructors.
		/// </summary>
		private void Initialize( string domain )
		{
			// Initialize the connectionId counter
			connectionId = 1;
			
			this.domain = domain;
			
			// Initialize default messages
			welcomeMessage = String.Format( MESSAGE_DEFAULT_WELCOME, domain );
			heloResponse = String.Format( MESSAGE_DEFAULT_HELO_RESPONSE, domain );		
		}
		
		#endregion
		
		#region Properties
		
		#endregion
				
		#region User Messages (Overridable)
		
		/// <summary>
		/// Returns the welcome message to display to new client connections.
		/// This method can be overridden to allow for user defined welcome messages.
		/// Please refer to RFC 821 for the rules on acceptable welcome messages.
		/// </summary>
		public virtual string WelcomeMessage
		{
			get
			{
				return welcomeMessage;
			}
			set
			{
				welcomeMessage = String.Format( value, domain );
			}
		}
		
		/// <summary>
		/// The response to the HELO command.  This response should
		/// include the local server's domain name.  Please refer to RFC 821
		/// for more details.
		/// </summary>
		public virtual string HeloResponse
		{
			get
			{
				return heloResponse;
			}
			set
			{
				heloResponse = String.Format( value, domain );
			}
		}
		
		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// ProcessConnection handles a connected TCP Client
		/// and performs all necessary interaction with this
		/// client to comply with RFC821.  This method is thread 
		/// safe.
		/// </summary>
		public void ProcessConnection( Socket socket )
		{
			long currentConnectionId = 0;
			// Really only need to lock on the long, but that is not
			// allowed.  Is there a better way to do this?
			lock( this )
			{
				currentConnectionId = connectionId++;
			}
			
			SMTPContext context = new SMTPContext( currentConnectionId, socket );
			
			try 
			{
				SendWelcomeMessage( context );
				
				ProcessCommands( context );
			}
			catch( Exception exception )
			{
				log.Error( String.Format( "Connection {0}: Error: {1}", context.ConnectionId, exception ), exception );
			}
		}
		
		#endregion		
		
		#region Private Handler Methods
		
		/// <summary>
		/// Sends the welcome greeting to the client.
		/// </summary>
		private void SendWelcomeMessage( SMTPContext context )
		{
			context.WriteLine( WelcomeMessage );
		}
		
		/// <summary>
		/// Handles the command input from the client.  This
		/// message returns when the client issues the quit command.
		/// </summary>
		private void ProcessCommands( SMTPContext context )
		{
			bool isRunning = true;
			String inputLine;
			
			// Loop until the client quits.
			while( isRunning )
			{
				try
				{
					inputLine = context.ReadLine();
					if( inputLine == null )
					{
						isRunning = false;
						context.Close();
						continue;
					}

					log.Debug( "ProcessCommands Read: " + inputLine );
					String[] inputs = inputLine.Split( " ".ToCharArray() );
					
					switch( inputs[0].ToLower() )
					{
						case "helo":
							Helo( context, inputs );
							break;
						case "rset":
							Rset( context );
							break;
						case "noop":
							context.WriteLine( MESSAGE_OK );
							break;
						case "quit":
							isRunning = false;
							context.WriteLine( MESSAGE_GOODBYE );
							context.Close();
							break;
						case "mail":
							if( inputs[1].ToLower().StartsWith( "from" ) )
							{
								Mail( context, inputLine.Substring( inputLine.IndexOf( " " ) ) );
								break;
							}
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
						case "rcpt":
							if( inputs[1].ToLower().StartsWith( "to" ) ) 							
							{
								Rcpt( context, inputLine.Substring( inputLine.IndexOf( " " ) ) );
								break;
							}
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
						case "data":
							Data( context );
							break;
						default:
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
					}				
				}
				catch( Exception exception )
				{
					log.Error( String.Format( "Connection {0}: Exception occured while processing commands: {1}", context.ConnectionId, exception ), exception );
					context.WriteLine( MESSAGE_SYSTEM_ERROR );
				}
			}
		}

		/// <summary>
		/// Handles the HELO command.
		/// </summary>
		private void Helo( SMTPContext context, String[] inputs )
		{
			if( context.LastCommand == -1 )
			{
				if( inputs.Length == 2 )
				{
					context.ClientDomain = inputs[1];
					context.LastCommand = COMMAND_HELO;
					context.WriteLine( HeloResponse );				
				}
				else
				{
					context.WriteLine( MESSAGE_INVALID_ARGUMENT_COUNT );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		/// <summary>
		/// Reset the connection state.
		/// </summary>
		private void Rset( SMTPContext context )
		{
			if( context.LastCommand != -1 )
			{
				// Dump the message and reset the context.
				context.Reset();
				context.WriteLine( MESSAGE_OK );
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		/// <summary>
		/// Handle the MAIL FROM:&lt;address&gt; command.
		/// </summary>
		private void Mail( SMTPContext context, string argument )
		{
			bool addressValid = false;
			if( context.LastCommand == COMMAND_HELO )
			{
				string address = ParseAddress( argument );
				if( address != null )
				{
					try
					{
						EmailAddress emailAddress = new EmailAddress( address );
						context.Message.FromAddress = emailAddress;
						context.LastCommand = COMMAND_MAIL;
						addressValid = true;
						context.WriteLine( MESSAGE_OK );
						if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: MailFrom address: {1} accepted.", context.ConnectionId, address ) );
					}
					catch( InvalidEmailAddressException )
					{
						// This is fine, just fall through.
					}
				}
				
				// If the address is invalid, inform the client.
				if( !addressValid )
				{
					if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: MailFrom argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument ) );
					context.WriteLine( MESSAGE_INVALID_ADDRESS );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		/// <summary>
		/// Handle the RCPT TO:&lt;address&gt; command.
		/// </summary>
		private void Rcpt( SMTPContext context, string argument )
		{
			if( context.LastCommand == COMMAND_MAIL || context.LastCommand == COMMAND_RCPT )
			{				
				string address = ParseAddress( argument );
				if( address != null )
				{
					try
					{
						EmailAddress emailAddress = new EmailAddress( address );
						
						// Check to make sure we want to accept this message.
						if( recipientFilter.AcceptRecipient( context, emailAddress ) )
						{						
							context.Message.AddToAddress( emailAddress );
							context.LastCommand = COMMAND_RCPT;							
							context.WriteLine( MESSAGE_OK );
							if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: RcptTo address: {1} accepted.", context.ConnectionId, address ) );
						}
						else
						{
							context.WriteLine( MESSAGE_UNKNOWN_USER );
							if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: RcptTo address: {1} rejected.  Did not pass Address Filter.", context.ConnectionId, address ) );
						}
					}
					catch( InvalidEmailAddressException )
					{
						if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: RcptTo argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument ) );
						context.WriteLine( MESSAGE_INVALID_ADDRESS );
					}
				}
				else
				{
					if( log.IsDebugEnabled ) log.Debug( String.Format( "Connection {0}: RcptTo argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument ) );
					context.WriteLine( MESSAGE_INVALID_ADDRESS );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		private void Data( SMTPContext context )
		{
			context.WriteLine( MESSAGE_START_DATA );
			
			SMTPMessage message = context.Message;
			IPEndPoint clientEndPoint = (IPEndPoint) context.Socket.RemoteEndPoint;
			StringBuilder header = new StringBuilder();
			header.Append( String.Format( "Received: from {0} ({0} [{1}])", context.ClientDomain, clientEndPoint.Address ) );
			header.Append( "\r\n" );
			header.Append( String.Format( "     by {0} (Eric Daugherty's C# Email Server)", domain ) );
			header.Append( "\r\n" );
			header.Append( "     " + System.DateTime.Now );
			header.Append( "\r\n" );
			
			message.AddData( header.ToString() );
			
			String line = context.ReadLine();
			while( !line.Equals( "." ) )
			{
				message.AddData( line );
				message.AddData( "\r\n" );
				line = context.ReadLine();
			}
			
			// Spool the message
			messageSpool.SpoolMessage( message );
			context.WriteLine( MESSAGE_OK );
			
			// Reset the connection.
			context.Reset();
		}

		#endregion
		
		#region Private Helper Methods
		
		/// <summary>
		/// Parses a valid email address out of the input string and return it.
		/// Null is returned if no address is found.
		/// </summary>
		private string ParseAddress( string input )
		{
			Match match = ADDRESS_REGEX.Match( input );
			string matchText;
			if( match.Success )
			{
				matchText = match.Value;
				
				// Trim off the :< chars
				matchText = matchText.Remove( 0, 1 );
				// trim off the . char.
				matchText = matchText.Remove( matchText.Length - 1, 1 );
				
				return matchText;
			}
			return null;
		}
		
		#endregion
				
	}
}
