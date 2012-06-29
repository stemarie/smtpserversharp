using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using src.Common;
using src.Properties;

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
        
        // Regular Expressions
        private static readonly Regex ADDRESS_REGEX = new Regex("<.+@.+>", RegexOptions.IgnoreCase);

        #endregion

        #region Variables

        /// <summary>Default Logger</summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SMTPProcessor));

        /// <summary>Incoming Message spool</summary>
        private readonly IMessageSpool messageSpool;

        /// <summary>Determines which recipients to accept for delivery.</summary>
        private readonly IRecipientFilter recipientFilter;

        /// <summary>
        /// Every connection will be assigned a unique id to 
        /// provide consistent log output and tracking.
        /// </summary>
        private long connectionId;

        /// <summary>Domain name for this server.</summary>
        private string domain;

        /// <summary>The response to the HELO command.</summary>
        private string heloResponse;

        /// <summary>The message to display to the client when they first connect.</summary>
        private string welcomeMessage;

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
        public SMTPProcessor(string domain)
        {
            Initialize(domain);

            // Initialize default Interface implementations.
            recipientFilter = new LocalRecipientFilter(domain);
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
        public SMTPProcessor(string domain, IRecipientFilter recipientFilter)
        {
            Initialize(domain);

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
        public SMTPProcessor(string domain, IMessageSpool messageSpool)
        {
            Initialize(domain);

            recipientFilter = new LocalRecipientFilter(domain);
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
        public SMTPProcessor(string domain, IRecipientFilter recipientFilter, IMessageSpool messageSpool)
        {
            Initialize(domain);

            this.recipientFilter = recipientFilter;
            this.messageSpool = messageSpool;
        }

        /// <summary>
        /// Provides common initialization logic for the constructors.
        /// </summary>
        private void Initialize(string domain)
        {
            // Initialize the connectionId counter
            connectionId = 1;

            this.domain = domain;

            // Initialize default messages
            welcomeMessage = String.Format(Resources.Protocol_220_0_Welcome_to_Eric_Daugherty_s_C_SMTP_Server, domain);
            heloResponse = String.Format(Resources.Protocol_MESSAGE_DEFAULT_HELO_RESPONSE_250_0, domain);
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
            get { return welcomeMessage; }
            set { welcomeMessage = String.Format(value, domain); }
        }

        /// <summary>
        /// The response to the HELO command.  This response should
        /// include the local server's domain name.  Please refer to RFC 821
        /// for more details.
        /// </summary>
        public virtual string HeloResponse
        {
            get { return heloResponse; }
            set { heloResponse = String.Format(value, domain); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ProcessConnection handles a connected TCP Client
        /// and performs all necessary interaction with this
        /// client to comply with RFC821.  This method is thread 
        /// safe.
        /// </summary>
        public void ProcessConnection(Socket socket)
        {
            long currentConnectionId;
            // Really only need to lock on the long, but that is not
            // allowed.  Is there a better way to do this?
            lock (this)
            {
                currentConnectionId = connectionId++;
            }

            var context = new SMTPContext(currentConnectionId, socket);

            try
            {
                SendWelcomeMessage(context);

                ProcessCommands(context);
            }
            catch (Exception exception)
            {
                log.Error(String.Format(Resources.Log_ProcessConnection_Connection_0_Error_1, context.ConnectionId, exception), exception);
            }
        }

        #endregion

        #region Private Handler Methods

        /// <summary>
        /// Sends the welcome greeting to the client.
        /// </summary>
        private void SendWelcomeMessage(SMTPContext context)
        {
            context.WriteLine(WelcomeMessage);
        }

        /// <summary>
        /// Handles the command input from the client.  This
        /// message returns when the client issues the quit command.
        /// </summary>
        private void ProcessCommands(SMTPContext context)
        {
            bool isRunning = true;

            // Loop until the client quits.
            while (isRunning)
            {
                try
                {
                    String inputLine = context.ReadLine();
                    if (inputLine == null)
                    {
                        isRunning = false;
                        context.Close();
                        continue;
                    }

                    log.Debug(string.Format(Resources.Log_ProcessCommands_ProcessCommands_Read_0, inputLine));
                    String[] inputs = inputLine.Split(" ".ToCharArray());

                    var messageUnknownCommand = Resources.Protocol_MESSAGE_UNKNOWN_COMMAND_500_Command_Unrecognized;
                    switch (inputs[0].ToLower())
                    {
                        case "helo":
                            Helo(context, inputs);
                            break;
                        case "rset":
                            Rset(context);
                            break;
                        case "noop":
                            context.WriteLine(Resources.Protocol_MESSAGE_OK_250_OK);
                            break;
                        case "quit":
                            isRunning = false;
                            context.WriteLine(Resources.Protocol_MESSAGE_GOODBYE_221_Goodbye);
                            context.Close();
                            break;
                        case "mail":
                            if (inputs[1].ToLower().StartsWith("from"))
                            {
                                Mail(context, inputLine.Substring(inputLine.IndexOf(" ", StringComparison.Ordinal)));
                                break;
                            }
                            context.WriteLine(messageUnknownCommand);
                            break;
                        case "rcpt":
                            if (inputs[1].ToLower().StartsWith("to"))
                            {
                                Rcpt(context, inputLine.Substring(inputLine.IndexOf(" ", StringComparison.Ordinal)));
                                break;
                            }
                            context.WriteLine(messageUnknownCommand);
                            break;
                        case "data":
                            Data(context);
                            break;
                        default:
                            context.WriteLine(messageUnknownCommand);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    log.Error(
                        String.Format(Resources.Log_ProcessCommands_Connection_0_Exception_occured_while_processing_commands_1,
                                      context.ConnectionId, exception), exception);
                    context.WriteLine(Resources.Protocol_MESSAGE_SYSTEM_ERROR_554_Transaction_failed);
                }
            }
        }

        /// <summary>
        /// Handles the HELO command.
        /// </summary>
        private void Helo(SMTPContext context, String[] inputs)
        {
            if (context.LastCommand == -1)
            {
                if (inputs.Length == 2)
                {
                    context.ClientDomain = inputs[1];
                    context.LastCommand = COMMAND_HELO;
                    context.WriteLine(HeloResponse);
                }
                else
                {
                    context.WriteLine(Resources.Protocol_MESSAGE_INVALID_ARGUMENT_COUNT__501_Incorrect_number_of_arguments);
                }
            }
            else
            {
                context.WriteLine(Resources.Protocol_MESSAGE_INVALID_COMMAND_ORDER_503_Command_not_allowed_here);
            }
        }

        /// <summary>
        /// Reset the connection state.
        /// </summary>
        private void Rset(SMTPContext context)
        {
            if (context.LastCommand != -1)
            {
                // Dump the message and reset the context.
                context.Reset();
                context.WriteLine(Resources.Protocol_MESSAGE_OK_250_OK);
            }
            else
            {
                context.WriteLine(Resources.Protocol_MESSAGE_INVALID_COMMAND_ORDER_503_Command_not_allowed_here);
            }
        }

        /// <summary>
        /// Handle the MAIL FROM:&lt;address&gt; command.
        /// </summary>
        private void Mail(SMTPContext context, string argument)
        {
            bool addressValid = false;
            if (context.LastCommand == COMMAND_HELO)
            {
                string address = ParseAddress(argument);
                if (address != null)
                {
                    try
                    {
                        var emailAddress = new EmailAddress(address);
                        context.Message.FromAddress = emailAddress;
                        context.LastCommand = COMMAND_MAIL;
                        addressValid = true;
                        context.WriteLine(Resources.Protocol_MESSAGE_OK_250_OK);
                        if (log.IsDebugEnabled)
                            log.Debug(String.Format(Resources.Log_Mail_Connection_0_MailFrom_address_1_accepted,
                                                    context.ConnectionId, address));
                    }
                    catch (InvalidEmailAddressException)
                    {
                        // This is fine, just fall through.
                    }
                }

                // If the address is invalid, inform the client.
                if (!addressValid)
                {
                    if (log.IsDebugEnabled)
                        log.Debug(
                            String.Format(
                                Resources.Log_Connection_0_MailFrom_argument_1_rejected_Should_be_from_username_domain_com,
                                context.ConnectionId, argument));
                    context.WriteLine(Resources.Protocol_MESSAGE_INVALID_ADDRESS_451_Address_is_invalid);
                }
            }
            else
            {
                context.WriteLine(Resources.Protocol_MESSAGE_INVALID_COMMAND_ORDER_503_Command_not_allowed_here);
            }
        }

        /// <summary>
        /// Handle the RCPT TO:&lt;address&gt; command.
        /// </summary>
        private void Rcpt(SMTPContext context, string argument)
        {
            if (context.LastCommand == COMMAND_MAIL || context.LastCommand == COMMAND_RCPT)
            {
                string address = ParseAddress(argument);
                var messageInvalidAddress = Resources.Protocol_MESSAGE_INVALID_ADDRESS_451_Address_is_invalid;
                if (address != null)
                {
                    try
                    {
                        var emailAddress = new EmailAddress(address);

                        // Check to make sure we want to accept this message.
                        if (recipientFilter.AcceptRecipient(context, emailAddress))
                        {
                            context.Message.AddToAddress(emailAddress);
                            context.LastCommand = COMMAND_RCPT;
                            context.WriteLine(Resources.Protocol_MESSAGE_OK_250_OK);
                            if (log.IsDebugEnabled)
                                log.Debug(String.Format(Resources.Log_Connection_0_RcptTo_address_1_accepted,
                                                        context.ConnectionId, address));
                        }
                        else
                        {
                            context.WriteLine(Resources.Protocol_MESSAGE_UNKNOWN_USER_550_User_does_not_exist);
                            if (log.IsDebugEnabled)
                                log.Debug(
                                    String.Format(
                                        Resources.Log_Connection_0_RcptTo_address_1_rejected_Did_not_pass_Address_Filter,
                                        context.ConnectionId, address));
                        }
                    }
                    catch (InvalidEmailAddressException)
                    {
                        if (log.IsDebugEnabled)
                            log.Debug(
                                String.Format(
                                    Resources.Log_Connection_0_RcptTo_argument_1_rejected_Should_be_from_username_domain_com,
                                    context.ConnectionId, argument));
                        context.WriteLine(messageInvalidAddress);
                    }
                }
                else
                {
                    if (log.IsDebugEnabled)
                        log.Debug(
                            String.Format(
                                Resources.Log_Connection_0_RcptTo_argument_1_rejected_Should_be_from_username_domain_com,
                                context.ConnectionId, argument));
                    context.WriteLine(messageInvalidAddress);
                }
            }
            else
            {
                context.WriteLine(Resources.Protocol_MESSAGE_INVALID_COMMAND_ORDER_503_Command_not_allowed_here);
            }
        }

        private void Data(SMTPContext context)
        {
            context.WriteLine(Resources.Protocol_MESSAGE_START_DATA__354_Start_mail_input_end_with_CRLF_CRLF);

            SMTPMessage message = context.Message;
            var clientEndPoint = (IPEndPoint)context.Socket.RemoteEndPoint;
            var header = new StringBuilder();
            header.AppendFormat(Resources.EMAIL_Received_from_0_0_1, context.ClientDomain, clientEndPoint.Address);
            header.AppendLine();
            header.AppendFormat(Resources.EMAIL_by_0_Eric_Daugherty_s_C_Email_Server, domain);
            header.AppendLine();
            header.Append("     " + DateTime.Now);
            header.AppendLine();

            message.AddData(header.ToString());

            String line = context.ReadLine();
            while (!line.Equals("."))
            {
                message.AddData(line);
                message.AddData("\r\n");
                line = context.ReadLine();
            }

            // Spool the message
            messageSpool.SpoolMessage(message);
            context.WriteLine(Resources.Protocol_MESSAGE_OK_250_OK);

            // Reset the connection.
            context.Reset();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Parses a valid email address out of the input string and return it.
        /// Null is returned if no address is found.
        /// </summary>
        private string ParseAddress(string input)
        {
            Match match = ADDRESS_REGEX.Match(input);
            if (match.Success)
            {
                string matchText = match.Value;

                // Trim off the :< chars
                matchText = matchText.Remove(0, 1);
                // trim off the . char.
                matchText = matchText.Remove(matchText.Length - 1, 1);

                return matchText;
            }
            return null;
        }

        #endregion
    }
}