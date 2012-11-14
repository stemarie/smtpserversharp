namespace SmtpServer.Mailbox
{
    /// <summary>
    /// Mailbox Manager is a singleton class that provides
    /// access to the current mailbox implementation.
    /// </summary>
    /// <remarks>
    /// Incomplete.
    /// </remarks>
    public class MailboxManager
    {
        private static readonly IManager instance = new Manager();

        public static IManager Instance { get { return instance; } }

        #region Constructors

        /// <summary>
        /// Private constructor - Can't directly make an instance
        /// </summary>
        private MailboxManager()
        { }

        #endregion
    }
}
