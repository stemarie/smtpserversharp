using System;
using System.Net.Sockets;

namespace src.Net
{
    /// <summary>
    /// Wraps the ConnectionProcessor and Socket to allow a new thread to be
    /// started that kicks off the ConnectionProcessor's process( Socket) method.
    /// </summary>
    public class ConnectionWrapper : IDisposable
    {
        private readonly ConnectionProcessor _processor;
        private readonly Socket _socket;

        /// <summary>
        /// Create a ConnectionWrapper to allow for a thread start.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="socket"></param>
        public ConnectionWrapper(ConnectionProcessor processor, Socket socket)
        {
            _processor = processor;
            _socket = socket;
        }

        /// <summary>
        /// Entry point for the Thread that will handle this Socket connection.
        /// </summary>
        public void Start()
        {
            _processor(_socket);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}