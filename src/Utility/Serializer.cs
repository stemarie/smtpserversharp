using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SmtpServer.Utility
{
    public class Serializer<T>
        where T : class
    {
        private readonly XmlSerializer _serializer;

        public Serializer()
        {
            _serializer = new XmlSerializer(typeof(T));
        }

        public string Serialize(T data)
        {
            using (var ms = new MemoryStream())
            {
                _serializer.Serialize(ms, data);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public T Deserialize(string data)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(Encoding.Default.GetBytes(data), 0, Encoding.Default.GetByteCount(data));
                return _serializer.Deserialize(ms) as T;
            }
        }
    }
}
