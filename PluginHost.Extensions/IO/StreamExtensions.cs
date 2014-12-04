using System.IO;
using System.Text;

namespace PluginHost.Extensions.IO
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var bytes  = reader.ReadBytes((int) stream.Length);
                return bytes;
            }
        }

        public static string ReadAsText(this Stream stream)
        {
            var bytes = ReadAllBytes(stream);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
