using System.Xml.Serialization;

namespace TcpListenerWeb.Helpers
{
    public class FileHelper<T> where T : new()
    {
        private string _filePath;

        public FileHelper(string filePath)
        {
            _filePath = filePath;
        }

        public void SerializeToFile(T data)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var streamWriter = new StreamWriter(_filePath))
            {
                serializer.Serialize(streamWriter, data);
                streamWriter.Close();
            }
        }

        public T DeserializeFromFile()
        {
            if (!File.Exists(_filePath))
                return new T();

            var serializer = new XmlSerializer(typeof(T));

            using (var streamReader = new StreamReader(_filePath))
            {
                var data = (T)serializer.Deserialize(streamReader);

                return data;
            }
        }
    }
}
