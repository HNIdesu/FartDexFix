using System.Text;

namespace HNIdesu.Dex
{
    
    internal class Uleb128StringReader : BinaryOperator.StringReader
    {      
        public override string ReadString(Stream stream, Encoding encoding)
        {
            stream.ReadUleb128();
            List<byte> buffer=new List<byte>();
            for(int b = stream.ReadByte(); b != 0; b = stream.ReadByte())
                buffer.Add((byte)b);
            return MUTF8Encoding.Instance.GetString(buffer.ToArray());
        }

    }
}
