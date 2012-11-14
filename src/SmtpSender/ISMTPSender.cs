using System.Collections.Generic;
using System.Net.Mail;

namespace SmtpServer.SmtpSender
{
    public interface ISMTPSender
    {
        /// <summary>
        /// Delivers an email message to the SMTP server for the specified
        /// domain.  All addresses in the array should be for the same domain.
        /// </summary>
        /// <param name="domain">
        /// The internet domain name to use to lookup the SMTP server address.
        /// </param>
        /// <param name="addresses">
        /// A set of addresses that should all be delivered to.
        /// </param>
        /// <param name="data">
        /// The raw message data.
        /// </param>
        void DeliverMessage(string domain, IEnumerable<MailAddress> addresses, string data);
    }
}