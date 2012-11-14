using System;
using System.IO;
using System.Linq;
using SmtpServer.MessageSpool.File.Properties;
using SmtpServer.Utility;
using log4net;

namespace SmtpServer.MessageSpool.File
{
    /// <summary>
    /// Stores Spooled SMTPMessages as files.
    /// </summary>
    public class FileMessageSpool : IMessageSpool
    {
        #region Variables

        private readonly object _lock = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(FileMessageSpool));
        private readonly string _directory;

        #endregion

        /// <summary>
        /// Initialize the file spool with the correct directory.
        /// </summary>
        public FileMessageSpool(string directory)
        {
            lock (_lock)
            {
                _directory = directory;
                if (!Directory.Exists(directory))
                {
                    _log.Warn(Resources.Log_FileSpool_directory_does_not_exist_Attempting_to_create);
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception exception)
                    {
                        _log.Error(
                            String.Format(Resources.Log_Error_creating_FileSpool_directory_0_Exception_1, directory,
                                          exception), exception);
                        throw;
                    }
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
            string fileName = DateTime.UtcNow.Ticks.ToString();
            while (System.IO.File.Exists(fileName))
            {
                fileName = DateTime.UtcNow.Ticks.ToString();
            }
            try
            {
                using (StreamWriter s = System.IO.File.CreateText(fileName))
                {
                    var serializer = new Serializer<SMTPMessage>();
                    var data = serializer.Serialize(message);
                    s.Write(data);
                    s.Flush();
                    s.Close();
                    return true;
                }

            }
            catch (Exception ex)
            {
                _log.Error("Can't spool message", ex);
                return false;
            }
        }

        public SMTPMessage NextMessage()
        {
            lock (_lock)
            {
                if (Directory.GetFiles(_directory).Any())
                {
                    string fileName = Directory.GetFiles(_directory).OrderBy(f => f).First();
                    SMTPMessage message;
                    using (StreamReader s = System.IO.File.OpenText(fileName))
                    {
                        Serializer<SMTPMessage> xmlSerializer = new Serializer<SMTPMessage>();
                        message = xmlSerializer.Deserialize(s.ReadToEnd());
                        s.Close();
                    }
                    System.IO.File.Delete(fileName);
                    return message;
                }
                return null;
            }
        }

        public void ClearSpool()
        {
            foreach (string file in Directory.GetFiles(_directory))
            {
                System.IO.File.Delete(file);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        { }
    }
}
