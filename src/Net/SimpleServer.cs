using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace src.Net
{
	/// <summary>
	/// The delegate that is called when to process a new connection (Socket).
	/// </summary>
	public delegate void ConnectionProcessor( Socket socket );
	
	/// <summary>
	/// This class provides a bare bones implementation
	/// of a Server to allow the SMTPProcessor or POP3Processor
	/// to handle incoming socket connections.
	/// </summary>
	/// <remarks>
	/// This class provides a very simple server implementation that accepts
	/// incoming Socket connections and passes the call to SMTPProcessor or
	/// POP3Processor for processing.  This code is for example/test use only
	/// and should not be considered a production solution.  
	/// </remarks>
	public class SimpleServer
	{
		#region Variables
		
		private bool isRunning = false;
		private TcpListener listener;
		
		private int port;
		private ConnectionProcessor processor;
		
		private static ILog log = LogManager.GetLogger( typeof( SimpleServer ) );
		
		#endregion
				
		#region Constructors
		
		/// <summary>
		/// Creates a new SimpleServer that listens on a specific
		/// port for connections and passes them to the specified delagat
		/// </summary>
		/// <param name="port">The port to listen on.</param>
		/// <param name="processor">The ConnectionProcessor that will handle the incoming connections.</param>
		public SimpleServer( int port, ConnectionProcessor processor )
		{
			this.port = port;
			this.processor = processor;
		}
		
		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// Listens for new connections and starts a new thread to handle each
		/// new connection.  Loops infinitely.
		/// </summary>
		public void Start()
		{
			IPEndPoint endPoint = new IPEndPoint( IPAddress.Any, port );
			listener = new TcpListener( endPoint );
			listener.Start();

			isRunning = true;

			while( isRunning )
			{
				try
				{
					Socket socket = listener.AcceptSocket();
					ConnectionWrapper handler = new ConnectionWrapper( processor, socket );
					new Thread( new ThreadStart( handler.Start ) ).Start();					
				}
				catch {}
			}
		}

		/// <summary>
		/// Stop the server.  This notifies the listener to stop accepting new connections
		/// and that the loop should exit.
		/// </summary>
		public void Stop()
		{
			isRunning = false;
			if( listener != null )
			{
				listener.Stop();
			}
		}
		
		#endregion
	}
	
	/// <summary>
	/// Wraps the ConnectionProcessor and Socket to allow a new thread to be
	/// started that kicks off the ConnectionProcessor's process( Socket) method.
	/// </summary>
	public class ConnectionWrapper
	{
		private ConnectionProcessor processor;
		private Socket socket;
		
		/// <summary>
		/// Create a ConnectionWrapper to allow for a thread start.
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="socket"></param>
		public ConnectionWrapper( ConnectionProcessor processor, Socket socket )
		{
			this.processor = processor;
			this.socket = socket;
		}
		
		/// <summary>
		/// Entry point for the Thread that will handle this Socket connection.
		/// </summary>
		public void Start()
		{
			processor( socket );
		}
	}
}
