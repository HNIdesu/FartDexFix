using System.Buffers.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace HNIdesu.Dex
{
    internal class CodeItemParser
    {
        public static List<CodeItem> Parse(string fileName)
        {
            List<CodeItem> list = new List<CodeItem>();
            string text = File.ReadAllText(fileName);         
            foreach(string item in text.Split(";"))
            {
                if (item.Trim() == "")
                    continue;
                dynamic x = Regex.Match(item, "(?<={)[^}]+").Value.Trim();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach(string value in x.Split(","))
                    dic.Add(value.Trim().Split(':')[0].Trim(), value.Trim().Split(':')[1].Trim());
                CodeItem codeItem = new CodeItem();
                byte[] buffer = new byte[int.Parse(dic["code_item_len"])];
                Base64.DecodeFromUtf8(Encoding.UTF8.GetBytes(dic["ins"]), buffer,out int a,out int b);
                codeItem.Data = buffer;
                codeItem.ClassName = Regex.Match(dic["name"], ".+(?=->)").Value;
                codeItem.MethodId = int.Parse(dic["method_idx"]);
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
