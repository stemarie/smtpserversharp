using System;
using System.Net.Sockets;
using System.Text;
using SmtpServer.Properties;
using log4net;

namespace SmtpServer
{
    /// <summary>
    /// Maintains the current state for a SMTP client connection.
    /// </summary>
    /// <remarks>
    /// This class is similar to a HTTP Session.  It is used to maintain all
    /// the state information about the current connection.
    /// </remarks>
    public class SMTPContext : IDisposable
    {
        #region Constants

        private const string EOL = "\r\n";

        #endregion

        #region Variables

        /// <summary>Default Logger</summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(SMTPContext));

        /// <summary>Logs all IO.  Seperate from normal Logger.</summary>
        private static readonly ILog IoLog = LogManager.GetLogger("IO." + typeof(SMTPContext));

        /// <summary>The unique ID assigned to this connection</summary>
        private readonly long _connectionId;

        /// <summary>Encoding to use to send/receive data from the socket.</summary>
        private readonly Encoding _encoding;

        /// <summary>The socket to the client.</summary>
        private readonly Socket _socket;

        /// <summary>
        /// It is possible that more than one line will be in
        /// the queue at any one time, so we need to store any input
        /// that has been read from the socket but not requested by the
        /// ReadLine command yet.
        /// </summary>
        private StringBuilder _inputBuffer;

        /// <summary>Last successful command received.</summary>
        private int _lastCommand;

        /// <summary>The incoming message.</summary>
        private SMTPMessage _message;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize this context for a given socket connection.
        /// </summary>
        public SMTPContext(long connectionId, Socket socket)
        {
            if (Log.IsDebugEnabled)
                Log.DebugFormat(
                    Resources.Log_Connection_0_New_connection_from_client_1,
                    connectionId,
                    socket.RemoteEndPoint);

            _connectionId = connectionId;
            _lastCommand = -1;
            _socket = socket;
            _message = new SMTPMessage();

            // Set the encoding to ASCII.  
            _encoding = Encoding.ASCII;

            // Initialize the input buffer
            _inputBuffer = new StringBuilder();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The unique connection id.
        /// </summary>
        public long ConnectionId
        {
            get { return _connectionId; }
        }

        /// <summary>
        /// Last successful command received.
        /// </summary>
        public int LastCommand
        {
            get { return _lastCommand; }
            set { _lastCommand = value; }
        }

        /// <summary>
        /// The client domain, as specified by the helo command.
        /// </summary>
        public string ClientDomain { get; set; }

        /// <summary>
        /// The Socket that is connected to the client.
        /// </summary>
        public Socket Socket
        {
            get { return _socket; }
        }

        /// <summary>
        /// The SMTPMessage that is currently being received.
        /// </summary>
        public SMTPMessage Message
        {
            get { return _message; }
            set { _message = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the string to the socket as an entire line.  This
        /// method will append the end of line characters, so the data
        /// parameter should not contain them.
        /// </summary>
        /// <param name="data">The data to write the the client.</param>
        public void WriteLine(string data)
        {
            if (IoLog.IsDebugEnabled)
                IoLog.DebugFormat(Resources.Log_Connection_0_Wrote_Line_1, _connectionId, data);
            _socket.Send(_encoding.GetBytes(data + EOL));
        }

        /// <summary>
        /// Reads an entire line from the socket.  This method
        /// will block until an entire line has been read.
        /// </summary>
        public String ReadLine()
        {
            // If we already buffered another line, just return
            // from the buffer.			
            string output = ReadBuffer();
            if (output != null)
            {
                return output;
            }

            // Otherwise, read more input.
            var byteBuffer = new byte[80];

            // Read from the socket until an entire line has been read.			
            do
            {
                // Read the input data.
                int count = _socket.Receive(byteBuffer);

                if (count == 0)
                {
                    Log.Debug(Resources.Log_Socket_closed_before_end_of_line_received);
                    return null;
                }

                _inputBuffer.Append(_encoding.GetString(byteBuffer, 0, count));
                if (IoLog.IsDebugEnabled)
                    IoLog.DebugFormat(Resources.Log_Connection_0_Read_1, _connectionId, _inputBuffer);
            } while ((output = ReadBuffer()) == null);

            // IO Log statement is in ReadBuffer...

            return output;
        }

        /// <summary>
        /// Resets this context for a new message
        /// </summary>
        public void Reset()
        {
            if (Log.IsDebugEnabled) 
                Log.DebugFormat(Resources.Log_Connection_0_Reset, _connectionId);
            _message = new SMTPMessage();
            _lastCommand = SMTPProcessor.CommandHelo;
        }

        /// <summary>
        /// Closes the socket connection to the client and performs any cleanup.
        /// </summary>
        public void Close()
        {
            _socket.Close();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method that returns the first full line in
        /// the input buffer, or null if there is no line in the buffer.
        /// If a line is found, it will also be removed from the buffer.
        /// </summary>
        private string ReadBuffer()
        {
            // If the buffer has data, check for a full line.
            if (_inputBuffer.Length > 0)
            {
                string buffer = _inputBuffer.ToString();
                int eolIndex = buffer.IndexOf(EOL, StringComparison.Ordinal);
                if (eolIndex != -1)
                {
                    string output = buffer.Substring(0, eolIndex);
                    _inputBuffer = new StringBuilder(buffer.Substring(eolIndex + 2));
                    if (IoLog.IsDebugEnabled)
                        IoLog.DebugFormat(Resources.Log_Connection_0_Read_Line_1, _connectionId, output);
                    return output;
                }
            }
            return null;
        }

        #endregion

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