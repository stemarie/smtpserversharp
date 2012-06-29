using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;
using NUnit.Framework;
using EricDaugherty.CSES.Net;

namespace EricDaugherty.CSES.SmtpServer
{

	[TestFixture]
	/// <summary>Tests the SMTPProcessor class.</summary>
	public class SMTPProcessorTest
	{
		#region Variables
		
		private static readonly IPEndPoint endPoint = new IPEndPoint( IPAddress.Loopback, 9900 );
		private TcpListener listener;
		private MemoryMessageSpool messageSpool;
		
		#endregion
		
		#region Constructor
		
		public SMTPProcessorTest()
		{
			LogManager.ResetConfiguration();
			BasicConfigurator.Configure();
			messageSpool = new MemoryMessageSpool();
		}
		
		#endregion
		
		#region SetUp/TearDown
		
		[SetUp]
		public void Setup()
		{
			messageSpool.ClearSpool();
			Thread listener = new Thread( new ThreadStart( Listener ) );
			listener.IsBackground = true;
			listener.Start();
			// Block for a second to make sure the socket gets started.
			Thread.Sleep( 1000 );
		}

		private void Listener()
		{
			try
			{
				SMTPProcessor processor = new SMTPProcessor( "testdomain.com", messageSpool );
				
				listener = new TcpListener( endPoint );
				listener.Start();
				System.Console.WriteLine( "Socket listener started..." );
				Socket clientSocket = listener.AcceptSocket();				
				processor.ProcessConnection( clientSocket );
			}
			catch( Exception exception )
			{
				System.Console.WriteLine( "Exception in Listener: " + exception );
				System.Console.WriteLine( exception.StackTrace );
			}
		}
		
		[TearDown]
		public void Teardown()
		{
			listener.Stop();
		}
		
		#endregion
		
		
		[Test]
		public void BasicConnectionTest()
		{
			Socket socket = Connect();
			Disconnect( socket );						
		}
		
		[Test]
		public void MailFromAddressParsingTest()
		{
			Socket socket = Connect();
			
			CheckResponse( socket, "mail from:username@domain.com", "451" );
			CheckResponse( socket, "mail from:<user@name@domain.com>", "451" );
			
			CheckResponse( socket, "mail from:<user name@domain123.com>", "250" );
						
			Disconnect( socket );
		}
		
		[Test]
		public void RcptToAddressParsingTest()
		{
			Socket socket = Connect();
			
			CheckResponse( socket, "mail from:<user name@domain123.com>", "250" );
			
			CheckResponse( socket, "rcpt to:username@domain.com", "451" );
			CheckResponse( socket, "rcpt to:<user@name@domain.com>", "451" );
			
			CheckResponse( socket, "rcpt to:<user name@domain123.com>", "550" );
			CheckResponse( socket, "rcpt to:<username@domain.com>", "550" );
			
			CheckResponse( socket, "rcpt to:<username@testdomain.com>", "250" );
			CheckResponse( socket, "rcpt to:<user_100@testdomain.com>", "250" );
			
			Disconnect( socket );	
		}
		
		[Test]
		public void DataTest()
		{
			Socket socket = Connect();
			CheckResponse( socket, "mail from:<user name@domain123.com>", "250" );
			CheckResponse( socket, "rcpt to:<username@testdomain.com>", "250" );
			CheckResponse( socket, "data", "354" );
			
			WriteLine( socket, "Date: Tue, 4 Nov 2003 10:13:27 -0600 (CST)" );
			WriteLine( socket, "Subject: Test" );
			WriteLine( socket, "" );
			WriteLine( socket, "Message Body." );
			
			CheckResponse( socket, ".", "250" );
			
			Disconnect( socket );
			
			SMTPMessage message = messageSpool.NextMessage();
			
			System.Console.WriteLine( "Message Recieved: " );
			System.Console.Write( message.Data );
		}
		
		#region Helper Methods
		
		private Socket Connect()
		{
			System.Console.WriteLine( "Connecting..." );
			Socket socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			socket.Connect( endPoint ); 
			
			// Read Welcome Message
			string line = ReadLine( socket );
			Assertion.Assert( "Welcome Message not recieved.", line.StartsWith( "220" ) );
			
			// Helo
			WriteLine( socket, "helo nunittestdomain.com" );
			line = ReadLine( socket );
			Assertion.Assert( "Helo response incorrect.", line.Equals( "250 testdomain.com" ) );
			
			return socket;
		}
		
		private void Disconnect( Socket socket )
		{			
			// Quit
			WriteLine( socket, "quit" );
			string line = ReadLine( socket );
			Assertion.Assert( "Quit ack incorrect.", line.StartsWith( "221" ) );
			
			socket.Close();
		}
		
		private void CheckResponse( Socket socket, string command, string responseCode )
		{
			String line = WriteAndRead( socket, command );
			Assertion.Assert( command + " did not result in the correct response code: " + responseCode, line.StartsWith( responseCode ) );			
		}
		
		/// <summary>Helper method to combine a write and a read.</summary>
		public string WriteAndRead( Socket socket, string data )
		{
			WriteLine( socket, data );
			return ReadLine( socket );
		}
		
		/// <summary>
		/// Writes the string to the socket as an entire line.  This
		/// method will append the end of line characters, so the data
		/// parameter should not contain them.
		/// </summary>
		/// <param name="socket">The socket to write to.</param>
		/// <param name="data>The data to write the the client.</param>
		public void WriteLine( Socket socket, string data )
		{
			System.Console.WriteLine( "Wrote: " + data );
			socket.Send( Encoding.ASCII.GetBytes( data + "\r\n" ) );
		}
		
		/// <summary>
		/// Reads an entire line from the socket.  This method
		/// will block until an entire line has been read.
		/// </summary>
		/// <param name="socket"></param>
		public String ReadLine( Socket socket )
		{
			byte[] inputBuffer = new byte[80];
			int count;
			StringBuilder inputString = new StringBuilder();
			String currentValue;

			// Read from the socket until an entire line has been read.			
			do
			{
				// Read the input data.
				count = socket.Receive( inputBuffer );
				
				inputString.Append( Encoding.ASCII.GetString( inputBuffer, 0, count ) );
				currentValue = inputString.ToString();				
			}
			while( currentValue.IndexOf( "\r\n" ) == -1 );
						
			// Strip off EOL.
			currentValue = currentValue.Remove( currentValue.IndexOf( "\r\n" ), 2 );
						
			System.Console.WriteLine( "Read Line: " + currentValue );
			return currentValue;
		}

		#endregion
	}
}
