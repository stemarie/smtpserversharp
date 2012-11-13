using System;
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleServer));
        private readonly int _port;
        private readonly ConnectionProcessor _processor;
        private bool _isRunning;
        private TcpListener _listener;

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
            _port = port;
            _processor = processor;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Listens for new connections and starts a new thread to handle each
        /// new connection.  Loops infinitely.
        /// </summary>
        public void Start()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, _port);
            _listener = new TcpListener(endPoint);
            _listener.Start();

            _isRunning = true;

            while (_isRunning)
            {
                try
                {
                    Socket socket = _listener.AcceptSocket();
                    using (var handler = new ConnectionWrapper(_processor, socket))
                    {
                        new Thread(handler.Start).Start();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Run error", ex);
                }
            }
        }

        /// <summary>
        /// Stop the server.  This notifies the listener to stop accepting new connections
        /// and that the loop should exit.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            if (_listener != null)
            {
                _listener.Stop();
            }
        }

        #endregion
    }
}