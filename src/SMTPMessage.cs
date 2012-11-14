using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace SmtpServer
{
    /// <summary>
    /// Stores an incoming SMTP Message.
    /// </summary>
    public class SMTPMessage
    {
        #region Constants

        private static readonly string DoubleNewline = Environment.NewLine + Environment.NewLine;

        #endregion

        #region Variables

        private readonly StringBuilder _data;
        private readonly List<MailAddress> _recipientAddresses;
        private Hashtable _headerFields;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new message.
        /// </summary>
        public SMTPMessage()
        {
            _recipientAddresses = new List<MailAddress>();
            _data = new StringBuilder();
        }

        #endregion

        #region Properties

        /// <summary>
        /// A hash table of all the Headers in the email message.  They keys
        /// are the header names, and the values are the assoicated values, including
        /// any sub key/value pairs is the header.
        /// </summary>
        public Hashtable Headers
        {
            get { return _headerFields ?? (_headerFields = ParseHeaders(_data.ToString())); }
        }

        /// <summary>
        /// The email address of the person
        /// that sent this email.
        /// </summary>
        public MailAddress FromAddress { get; set; }

        /// <summary>
        /// The addresses that this message will be
        /// delivered to.
        /// </summary>
        public MailAddress[] ToAddresses
        {
            get { return _recipientAddresses.ToArray(); }
        }

        /// <summary>Message data.</summary>
        public string Data
        {
            get { return _data.ToString(); }
        }

        /// <summary>
        /// Parses the message body and creates an Attachment object
        /// for each attachment in the message.
        /// </summary>
        public SMTPMessagePart[] MessageParts
        {
            get { return ParseMessageParts(); }
        }

        /// <summary>Adds an address to the recipient list.</summary>
        public void AddToAddress(MailAddress address)
        {
            _recipientAddresses.Add(address);
        }

        /// <summary>Append data to message data.</summary>
        public void AddData(String data)
        {
            _data.Append(data);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parses an entire message or message part and returns the header entries
        /// as a hashtable.
        /// </summary>
        /// <param name="partData">The raw message or message part data.</param>
        /// <returns>A hashtable of the header keys and values.</returns>
        internal static Hashtable ParseHeaders(string partData)
        {
            var headerFields = new Hashtable();

            string[] parts = Regex.Split(partData, DoubleNewline);
            string headerString = parts[0] + DoubleNewline;

            MatchCollection headerKeyCollectionMatch = Regex.Matches(headerString, @"^(?<key>\S*):",
                                                                     RegexOptions.Multiline);
            foreach (Match headerKeyMatch in headerKeyCollectionMatch)
            {
                string headerKey = headerKeyMatch.Result("${key}");
                Match valueMatch = Regex.Match(headerString, headerKey + @":(?<value>.*?)\r\n[\S\r]",
                                               RegexOptions.Singleline);
                if (valueMatch.Success)
                {
                    string headerValue = valueMatch.Result("${value}").Trim();
                    headerValue = Regex.Replace(headerValue, "\r\n", "");
                    headerValue = Regex.Replace(headerValue, @"\s+", " ");
                    // TODO: Duplicate headers (like Received) will be overwritten by the 'last' value.					
                    headerFields[headerKey] = headerValue;
                }
            }

            return headerFields;
        }

        private SMTPMessagePart[] ParseMessageParts()
        {
            string message = _data.ToString();
            var contentType = (string)Headers["Content-Type"];

            // Check to see if it is a Multipart Messages
            if (contentType != null && Regex.Match(contentType, "multipart/mixed", RegexOptions.IgnoreCase).Success)
            {
                // Message parts are seperated by boundries.  Parse out what the boundry is so we can easily
                // parse the parts out of the message.
                Match boundryMatch = Regex.Match(contentType, "boundary=\"(?<boundry>\\S+)\"", RegexOptions.IgnoreCase);
                if (boundryMatch.Success)
                {
                    string boundry = boundryMatch.Result("${boundry}");

                    var messageParts = new ArrayList();

                    //TODO Improve this Regex.
                    MatchCollection matches = Regex.Matches(message, "--" + boundry + ".*\r\n");

                    int lastIndex = -1;
                    foreach (Match match in matches)
                    {
                        int currentIndex = match.Index;
                        int matchLength = match.Length;

                        if (lastIndex != -1)
                        {
                            string messagePartText = message.Substring(lastIndex, currentIndex - lastIndex);
                            messageParts.Add(new SMTPMessagePart(messagePartText));
                        }
                        lastIndex = currentIndex + matchLength;
                    }
                    return (SMTPMessagePart[])messageParts.ToArray(typeof(SMTPMessagePart));
                }
            }
            return new SMTPMessagePart[0];
        }

        #endregion
    }
}