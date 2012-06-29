using System;
using System.Net.Sockets;
using System.Text;
using log4net;
using src.Properties;

namespace src.SmtpServer
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
        private static readonly ILog log = LogManager.GetLogger(typeof(SMTPContext));

        /// <summary>Logs all IO.  Seperate from normal Logger.</summary>
        private static readonly ILog ioLog = LogManager.GetLogger("IO." + typeof(SMTPContext));

        /// <summary>The unique ID assigned to this connection</summary>
        private readonly long connectionId;

        /// <summary>Encoding to use to send/receive data from the socket.</summary>
        private readonly Encoding encoding;

        /// <summary>The socket to the client.</summary>
        private readonly Socket socket;

        /// <summary>
        /// It is possible that more than one line will be in
        /// the queue at any one time, so we need to store any input
        /// that has been read from the socket but not requested by the
        /// ReadLine command yet.
        /// </summary>
        private StringBuilder inputBuffer;

        /// <summary>Last successful command received.</summary>
        private int lastCommand;

        /// <summary>The incoming message.</summary>
        private SMTPMessage message;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize this context for a given socket connection.
        /// </summary>
        public SMTPContext(long connectionId, Socket socket)
        {
            if (log.IsDebugEnabled)
                log.Debug(
                    String.Format(
                        Resources.Log_Connection_0_New_connection_from_client_1,
                        connectionId,
                        socket.RemoteEndPoint));

            this.connectionId = connectionId;
            lastCommand = -1;
            this.socket = socket;
            message = new SMTPMessage();

            // Set the encoding to ASCII.  
            encoding = Encoding.ASCII;

            // Initialize the input buffer
            inputBuffer = new StringBuilder();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The unique connection id.
        /// </summary>
        public long ConnectionId
        {
            get { return connectionId; }
        }

        /// <summary>
        /// Last successful command received.
        /// </summary>
        public int LastCommand
        {
            get { return lastCommand; }
            set { lastCommand = value; }
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
            get { return socket; }
        }

        /// <summary>
        /// The SMTPMessage that is currently being received.
        /// </summary>
        public SMTPMessage Message
        {
            get { return message; }
            set { message = value; }
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
            if (ioLog.IsDebugEnabled) ioLog.Debug(String.Format(Resources.Log_Connection_0_Wrote_Line_1, connectionId, data));
            socket.Send(encoding.GetBytes(data + EOL));
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
                int count = socket.Receive(byteBuffer);

                if (count == 0)
                {
                    log.Debug(Resources.Log_Socket_closed_before_end_of_line_received);
                    return null;
                }

                inputBuffer.Append(encoding.GetString(byteBuffer, 0, count));
                if (ioLog.IsDebugEnabled)
                    ioLog.Debug(String.Format(Resources.Log_Connection_0_Read_1, connectionId, inputBuffer));
            } while ((output = ReadBuffer()) == null);

            // IO Log statement is in ReadBuffer...

            return output;
        }

        /// <summary>
        /// Resets this context for a new message
        /// </summary>
        public void Reset()
        {
            if (log.IsDebugEnabled) log.Debug(String.Format(Resources.Log_Connection_0_Reset, connectionId));
            message = new SMTPMessage();
            lastCommand = SMTPProcessor.COMMAND_HELO;
        }

        /// <summary>
        /// Closes the socket connection to the client and performs any cleanup.
        /// </summary>
        public void Close()
        {
            socket.Close();
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
            if (inputBuffer.Length > 0)
            {
                string buffer = inputBuffer.ToString();
                int eolIndex = buffer.IndexOf(EOL, StringComparison.Ordinal);
                if (eolIndex != -1)
                {
                    string output = buffer.Substring(0, eolIndex);
                    inputBuffer = new StringBuilder(buffer.Substring(eolIndex + 2));
                    if (ioLog.IsDebugEnabled)
                        ioLog.Debug(String.Format(Resources.Log_Connection_0_Read_Line_1, connectionId, output));
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
            socket.Dispose();
        }
    }
}