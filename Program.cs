namespace HNIdesu.Dex
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: input.dex ins1.bin ins2.bin ins3.bin...");
                return;
            }
            string dexFileName = args[0];
            if (!File.Exists(dexFileName))
            {
                Console.WriteLine($"error:输入文件{dexFileName}不存在");
                return;
            }
            string dexFileDirectory = Path.GetDirectoryName(dexFileName) ?? Environment.CurrentDirectory;
            DexFile dexFile = DexFile.Parse(dexFileName);
            var codeItemSet = new HashSet<CodeItem>();
            for (int i = 1; i < args.Length; i++)
            {
                string searchPattern = args[i];
                string dirName = Path.GetDirectoryName(searchPattern) ?? Environment.CurrentDirectory;
                string fileName = Path.GetFileName(searchPattern);
                IEnumerable<string> fileList;
                if (searchPattern.Contains('*') ||
                    searchPattern.Contains('?'))
                    fileList = Directory.EnumerateFiles(dirName, fileName);
                else
                {
                    if (!File.Exists(searchPattern))
                    {
                        Console.WriteLine($"can not find file {searchPattern}");
                        continue;
                    }
                    fileList = new string[] { searchPattern };
                }
                foreach (var file in fileList)
                {
                    var codeItemList = CodeItemParser.Parse(file);
                    foreach (var codeItem in codeItemList)
                    {
                        if (codeItemSet.TryGetValue(codeItem, out var innerItem))
                        {
                            if (!codeItem.Equals(innerItem))
                            {
                                Console.WriteLine("find different code item but have same hash value");
                                Console.WriteLine(codeItem.ToString());
                                Console.WriteLine(innerItem.ToString());
                            }
                        }
                        else
                            codeItemSet.Add(codeItem);
                    }
                }
            }

            string newFilePath = Path.Combine(dexFileDirectory, Path.GetFileNameWithoutExtension(dexFileName) + "_out.dex");
            File.Copy(dexFileName, newFilePath, true);
            using (FileStream fs = File.OpenWrite(newFilePath))
            {
                foreach (var codeItem in codeItemSet)
                {
                    long address;
                    try
                    {
                        string className = $"L{codeItem.ClassName.Replace(".", "/")};";
                        var methods = (from cid in dexFile.ClassIdList where cid.Class.Name.Value == className select cid.ClassData.VirtualMethods).First().Concat((from cid in dexFile.ClassIdList where cid.Class.Name.Value == className select cid.ClassData.DirectMethods).First());
                        address = (from m in methods where ReferenceEquals(m.MethodId, dexFile.MethodIdList[codeItem.MethodId]) select m.CodeItem.InsAdd).First();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    fs.Seek(address, SeekOrigin.Begin);
                    fs.Write(codeItem.Data);
                    Console.WriteLine($"codeitem {codeItem} has been applied");
                }
            }
            Console.WriteLine("完成");


        }
    }
}