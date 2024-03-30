using System.Text;
using System.Buffers.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace HNIdesu.Dex
{
    public sealed class CodeItemParser
    {
        /*
         原版Fart生成的CodeItem文件不好分析， 
         此函数解析的是自定义的CodeItem。需要根据实际CodeItem格式手动修改。
         */
        public static IEnumerable<CodeItem> Parse(string textContent)
        {
            foreach (string line in textContent.Split("\r\n"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var element = JsonNode.Parse(line) as JsonObject;
                if (element == null ||
                    !element.ContainsKey("code_item_len") ||
                    !element.ContainsKey("classname") ||
                    !element.ContainsKey("method_idx") ||
                    !element.ContainsKey("data"))
                {
                    Console.WriteLine($"error parsing line {line} in codeitem");
                    continue;
                }
                var dataLength = element["code_item_len"]!.GetValue<int>();
                var className = element["classname"]!.GetValue<string>();
                var methodId = element["method_idx"]!.GetValue<int>();
                var b64Data = element["data"]!.GetValue<string>();
                byte[] data = new byte[dataLength];
                Base64.DecodeFromUtf8(Encoding.UTF8.GetBytes(b64Data), data, out _, out _);
                CodeItem codeItem = new CodeItem(className,methodId,data);
                yield return codeItem;
            }
        }
    }
    public sealed class CodeItem
    {
        public byte[] Data { get;private set; }
        public int MethodId { get;private set; }
        public string ClassName { get; private set; }
        private int mHash;
        public CodeItem(string className,int methodId, byte[] data)
        {
            Data = data;
            MethodId = methodId;
            ClassName = className;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(MethodId));
                ms.Write(Encoding.UTF8.GetBytes(ClassName));
                mHash = BitConverter.ToInt32(new ReadOnlySpan<byte>(MD5.HashData(ms.ToArray()), 0, 4));
            }
        }
        public override string ToString()
        {
            return $"MethodId:{MethodId},ClassName:{ClassName}";
        }
        public override bool Equals(object? obj)
        {
            var other = obj as CodeItem;
            if (other is null) return false;
            return other.ClassName == ClassName &&
                other.MethodId == MethodId;
        }
        public static bool operator ==(CodeItem? thisObj, CodeItem? other)
        {
            if (thisObj == null) return false;
            return thisObj.Equals(other);
        }
        public static bool operator !=(CodeItem thisObj, CodeItem other)
        {
            return !(thisObj == other);
        }

        public override int GetHashCode() => mHash;
    }



}
