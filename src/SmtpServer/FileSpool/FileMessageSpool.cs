using System;
using System.IO;
using log4net;
using src.Properties;

namespace src.SmtpServer.FileSpool
{
    /// <summary>
    /// Stores Spooled SMTPMessages as files.
    /// </summary>
    public class FileMessageSpool : IMessageSpool
    {
        #region Variables

        private static readonly ILog log = LogManager.GetLogger(typeof(FileMessageSpool));

        #endregion

        /// <summary>
        /// Initialize the file spool with the correct directory.
        /// </summary>
        public FileMessageSpool(string directory)
        {
            if (!Directory.Exists(directory))
            {
                log.Warn(Resources.Log_FileSpool_directory_does_not_exist_Attempting_to_create);
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception exception)
                {
                    log.Error(String.Format(Resources.Log_Error_creating_FileSpool_directory_0_Exception_1, directory, exception), exception);
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <remarks>
        /// Interface method from IMessageSpool.
        /// </remarks>
        /// <param name='message'>The message to spool.</param>
        public virtual bool SpoolMessage(SMTPMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
