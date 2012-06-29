using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace src.Net
{
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

        private static ILog log = LogManager.GetLogger(typeof(SimpleServer));
        private readonly int port;
        private readonly ConnectionProcessor processor;
        private bool isRunning;
        private TcpListener listener;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new SimpleServer that listens on a specific
        /// port for connections and passes them to the specified delagat
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="processor">The ConnectionProcessor that will handle the incoming connections.</param>
        public SimpleServer(int port, ConnectionProcessor processor)
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
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(endPoint);
            listener.Start();

            isRunning = true;

            while (isRunning)
            {
                try
                {
                    Socket socket = listener.AcceptSocket();
                    using (var handler = new ConnectionWrapper(processor, socket))
                    {
                        new Thread(handler.Start).Start();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Stop the server.  This notifies the listener to stop accepting new connections
        /// and that the loop should exit.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            if (listener != null)
            {
                listener.Stop();
            }
        }

        #endregion
    }
}