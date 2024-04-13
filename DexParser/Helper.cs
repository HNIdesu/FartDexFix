using System;
using System.Text.Json;

namespace HNIdesu.Dex
{
    internal static class Helper
    {
        public static Dictionary<string,byte> GetOperationDictionary()
        {
            Dictionary<string, byte> dic=new Dictionary<string, byte> ();
            JsonDocument doc= JsonDocument.Parse(Resource.OperationList);
            foreach(var item in doc.RootElement.EnumerateObject())
                dic.Add(item.Name,Convert.ToByte(item.Value.GetString(), 16));
            return dic;
        }

        public static int ReadUleb128(this HNIdesu.IO.BinaryReaderEx br)
        {
            var steam = br.BaseStream;
            int result = 0;
            int index = 0;
            while (true)
            {
                int b = steam.ReadByte();
                result |= (b & 0x7F) << (7 * index++);
                if (b < 0x80)
                    break;
            }
            return result;
        }
    }
}
