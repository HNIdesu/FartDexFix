using System.Text;

namespace HNIdesu.IO
{
    public abstract class StringReader
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public abstract string ReadString(Stream stream);
    }
}
