using System.Net.Sockets;

namespace src.Net
{
    /// <summary>
    /// The delegate that is called when to process a new connection (Socket).
    /// </summary>
    public delegate void ConnectionProcessor(Socket socket);
}