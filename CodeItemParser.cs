using System.Text;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HNIdesu.Dex
{
    internal class CodeItemParser
    {
        /*
         原版Fart生成的CodeItem文件不好分析， 
         此函数解析的是自定义的CodeItem。需要根据实际CodeItem格式手动修改。
         */
        public static List<CodeItem> Parse(string fileName)
        {
            List<CodeItem> list = new List<CodeItem>();
            foreach(Match objMatch in Regex.Matches(File.ReadAllText(fileName), "{[^}]+}"))
            {
                JsonElement ele = JsonDocument.Parse(objMatch.Value).RootElement;
                CodeItem codeItem = new CodeItem();
                byte[] buffer = new byte[ele.GetProperty("code_item_len").GetInt32()];
                Base64.DecodeFromUtf8(Encoding.UTF8.GetBytes(ele.GetProperty("data").GetString()), buffer, out int a, out int b);
                codeItem.Data = buffer;
                codeItem.ClassName = ele.GetProperty("classname").GetString();
                codeItem.MethodId = ele.GetProperty("method_idx").GetInt32();
                list.Add(codeItem);
                
            }

            return list;

        }
    }
    internal class CodeItem
    {
        public byte[] Data;
        public int MethodId;
        public string ClassName;
    }


}
