namespace HNIdesu.Dex
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: input.dex ins1.bin ins2.bin ins3.bin...");
                return;
            }
            string inputFileName = args[0];
            if(!File.Exists(inputFileName))
            {
                Console.WriteLine($"error:输入文件{inputFileName}不存在");
                return;
            }
            DexFile file= DexFile.Parse(inputFileName);
            IEnumerable<CodeItem> codeItems = new List<CodeItem>();
            for (int i = 1; i < args.Length; i++)
            {
                string fileName = args[i];
                if(!File.Exists(fileName))
                {
                    Console.WriteLine($"warn:找不到文件{fileName}");
                    continue;
                }
                codeItems=codeItems.Union(CodeItemParser.Parse(fileName));
            }
            string newFilePath = Path.Combine(Path.GetDirectoryName(inputFileName),Path.GetFileNameWithoutExtension(inputFileName) + "_out.dex");
            File.Copy(inputFileName, newFilePath,true);

            using (FileStream fs = File.OpenWrite(newFilePath))
            {         
                foreach (var codeItem in codeItems)
                {
                    long address;
                    try{
                        string cm = codeItem.ClassName;
                        cm = $"L{cm.Replace(".", "/")};";
                        var methods = ( from cid in file.ClassIdList where cid.Class.Name.Value == cm select cid.ClassData.VirtualMethods ).First().Union(( from cid in file.ClassIdList where cid.Class.Name.Value == cm select cid.ClassData.DirectMethods ).First());
                        address = ( from m in methods where ReferenceEquals(m.MethodId, file.MethodIdList[codeItem.MethodId]) select m.CodeItem.InsAdd ).First();
                    }catch (Exception ex)
                    {
                        Console.WriteLine($"找不到class:{codeItem.ClassName}的方法:{file.MethodIdList[codeItem.MethodId].Name.Value}。");
                        continue;
                    }
                    fs.Seek(address, SeekOrigin.Begin);
                    fs.Write(codeItem.Data);
                }
            }
            Console.WriteLine("完成");
                
            
        }
    }
}