using HNIdesu.Collection;

using System.IO;
using System.Text;

namespace HNIdesu.Dex
{
    internal sealed class Uleb128StringReader : HNIdesu.IO.StringReader
    {
        public override string ReadString(HNIdesu.IO.BinaryReaderEx br)
        {
            int charCount=br.ReadUleb128();
            if (charCount == 0)
            {
                br.ReadByte();
                return "";
            }
            var buffer = new ArrayList<byte>(charCount*3);
            for (int b = br.ReadByte(); b != 0; b = br.ReadByte())
                buffer.Add((byte)b);
            return MUTF8Encoding.Instance.GetString(buffer.ToSpan());
        }
    }
}
