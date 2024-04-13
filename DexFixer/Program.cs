

namespace HNIdesu.Dex
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var program = Path.GetFileName(Environment.ProcessPath);
                Console.WriteLine($"Usage:{program} input.dex ins1.json ins2.json ins3.json...");
                return;
            }
            var startTime = DateTime.Now;

            string dexFileName = args[0];
            if (!File.Exists(dexFileName))
            {
                Console.WriteLine($"error:输入文件{dexFileName}不存在");
                return;
            }
            string dexFileDirectory = Path.GetDirectoryName(dexFileName) ?? Environment.CurrentDirectory;
            var dexFile = DexFile.Parse(dexFileName);

            var classNameMethodListDict = new Dictionary<string,IEnumerable<DexFile.ClassData.ClassMethod>>();
            foreach(var classId in dexFile.ClassIdList)
            {
                var classData = classId.ClassData;
                if (classData == null)
                    continue;
                classNameMethodListDict.Add(classId.Class.Name.Value, classData.DirectMethods.Concat(classData.VirtualMethods));
            }
                

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
                    fileList = [ searchPattern ];
                }
                foreach (var file in fileList)
                {
                    var codeItemList = CodeItemParser.Parse(File.ReadAllText(file));
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
                    long address=-1;
                    var className = $"L{codeItem.ClassName.Replace(".", "/")};";
                    if (!classNameMethodListDict.ContainsKey(className)) continue;
                    var methods = classNameMethodListDict[className];
                    var targetMethodId = dexFile.MethodIdList[codeItem.MethodId];
                    foreach (var method in methods)
                    {
                        if (ReferenceEquals(method.MethodId, targetMethodId))
                        {
                            address = method.CodeItem?.InsAdd??-1;
                            break;
                        }
                    }
                    if (address != -1)
                    {
                        fs.Seek(address, SeekOrigin.Begin);
                        fs.Write(codeItem.Data);
                        var method = dexFile.MethodIdList[codeItem.MethodId];
                        Console.WriteLine($"codeitem class:{codeItem.ClassName},method:{method.Name.Value}({method.Proto.ShortyId.Value}) has been applied");
                    }
                    
                }
            }
            Console.WriteLine($"完成，用时{(DateTime.Now-startTime).Ticks/TimeSpan.TicksPerMillisecond}ms");


        }
    }
}