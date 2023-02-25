using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using BinaryOperator;

namespace HNIdesu.Dex
{
    public partial class DexFile
    {
        public List<StringId> StringIdList = new List<StringId>();
        public List<TypeId> TypeIdList = new List<TypeId>();
        public List<ProtoId> ProtoIdList = new List<ProtoId>();
        public List<MethodId> MethodIdList = new List<MethodId>();
        public List<ClassId> ClassIdList = new List<ClassId>();
        
        public static DexFile Parse(string path)
        {
            DexFile file = new DexFile();
            List<StringId> stringIdList = new List<StringId>();
            List<TypeId> typeIdList = new List<TypeId>();
            List<ProtoId> protoIdList = new List<ProtoId>();
            List<MethodId> methodIdList = new List<MethodId>();
            List<ClassId> classIdList = new List<ClassId>();
            file.StringIdList = stringIdList;
            file.TypeIdList = typeIdList;
            file.ProtoIdList = protoIdList;
            file.MethodIdList = methodIdList;
            file.ClassIdList = classIdList;
            using (var fs = System.IO.File.OpenRead(path))
            {
                BinaryOperator.BinaryReader reader = new BinaryOperator.BinaryReader(fs);
                DexHeaderStruct dexHeader = reader.ReadMarshal<DexHeaderStruct>(Marshal.SizeOf(typeof(DexHeaderStruct)));

                //获取string ids
                reader.BaseStream.Seek(dexHeader.stringIdsOff,SeekOrigin.Begin);
                reader.StringReader = new Uleb128StringReader();
                for (int i = 0; i < dexHeader.stringIdsSize; i++)
                {
                    int offset=reader.ReadInt32();
                    reader.SeekAndRead(offset, SeekOrigin.Begin, (br) =>
                    {
                        string str = reader.ReadString();
                        stringIdList.Add(new StringId(str));
                    });
                }

                //获取Type ids
                reader.BaseStream.Seek(dexHeader.typeIdsOff, SeekOrigin.Begin);
                for(int i=0;i< dexHeader.typeIdsSize; i++)
                {
                    int offset=reader.ReadInt32();
                    typeIdList.Add(new TypeId(stringIdList[offset]));
                }

                //获取Proto ids
                reader.BaseStream.Seek(dexHeader.protoIdsOff, SeekOrigin.Begin);
                for (int i = 0; i < dexHeader.protoIdsSize; i++)
                {
                    int a=reader.ReadInt32();
                    int b=reader.ReadInt32();
                    reader.ReadInt32();//跳过
                    protoIdList.Add(new ProtoId(stringIdList[a], typeIdList[b]));
                }

                //获取Method ids
                reader.BaseStream.Seek(dexHeader.methodIdsOff, SeekOrigin.Begin);
                for (int i = 0; i < dexHeader.methodIdsSize; i++)
                {
                    int a = reader.ReadInt16();//class
                    int b = reader.ReadInt16();//proto
                    int c = reader.ReadInt32();//name
                    methodIdList.Add(new MethodId(typeIdList[a], stringIdList[c], protoIdList[b]));

                }

                //获取Class ids
                reader.BaseStream.Seek(dexHeader.classDefsOff, SeekOrigin.Begin);
                for (int i = 0; i < dexHeader.classDefsSize; i++)
                {
                    ClassId cid = new ClassId();
                    int class_idx= reader.ReadInt32();
                    if (class_idx == 3660)
                        Console.WriteLine();
                    int access_flags=reader.ReadInt32();
                    int superclass_ids=reader.ReadInt32();
                    int interfaces_off=reader.ReadInt32();
                    int source_file_idx=reader.ReadInt32();
                    reader.ReadBytes(4);//跳过
                    int class_data_off = reader.ReadInt32();
                    int static_values_off = reader.ReadInt32();

                    cid.SuperClass = typeIdList[superclass_ids];
                    cid.Class = typeIdList[class_idx];
                    if (source_file_idx != -1)
                        cid.SourceFileName = stringIdList[source_file_idx];
                    classIdList.Add(cid);
                    if (class_data_off != 0) {
                        reader.SeekAndRead(class_data_off, SeekOrigin.Begin, (br) => //读取ClassData
                        {
                            
                            ClassData classData = new ClassData();
                            dynamic x,y;
                            x=new ClassData.ClassDataHeader();
                            x.StaticFieldsSize = br.ReadUleb128();
                            x.InstanceFieldsSize = br.ReadUleb128();
                            x.DirectMethodsSize = br.ReadUleb128();
                            x.VirtualMethodsSize = br.ReadUleb128();
                            classData.Header = x;
                            //顺序 静态字段->实例字段->直接方法->虚方法
                            // 静态字段
                            for (int i = 0; i < x.StaticFieldsSize; i++)
                            {
                                br.ReadUleb128();
                                br.ReadUleb128();
                            }
                            //实例字段
                            for (int i = 0; i < x.InstanceFieldsSize; i++)
                            {
                                br.ReadUleb128();
                                br.ReadUleb128();
                            }
                            //直接方法
                            int baseAddr=0;
                            for (int i = 0; i < x.DirectMethodsSize; i++)
                            {
                                y = new ClassData.ClassMethod();
                                if (i == 0)
                                {
                                    baseAddr = reader.ReadUleb128();
                                    y.MethodId = methodIdList[baseAddr];
                                }
                                else
                                    y.MethodId = methodIdList[reader.ReadUleb128() + baseAddr];
                                    
                                y.AccessFlag=(AccessFlags)br.ReadUleb128();
                                int codeOff=br.ReadUleb128();
                                if (codeOff != 0)
                                {                                
                                    //读取CodeItem对象
                                    br.SeekAndRead(codeOff, SeekOrigin.Begin, (br1) =>
                                    {
                                        ClassData.CodeItem codeItem = new ClassData.CodeItem();
                                        codeItem.InsAdd = br1.BaseStream.Position;
                                        codeItem.RegCount = br1.ReadUInt16();
                                        codeItem.ArgCount = br1.ReadUInt16();
                                        codeItem.OutRegCount = br1.ReadUInt16();
                                        codeItem.TryCount = br1.ReadUInt16();
                                        codeItem._DbgInfoOffset = br1.ReadUInt32();
                                        codeItem.InsnsSize = br1.ReadUInt32();
                                        
                                        for (int j = 0; j < codeItem.InsnsSize; j++)
                                            codeItem.InsnsList.Add(br1.ReadUInt16());
                                        y.CodeItem = codeItem;
                                    });
                                    
                                }
                                classData.DirectMethods.Add(y);
                            }
                            for (int i = 0; i < x.VirtualMethodsSize; i++)
                            {
                                y = new ClassData.ClassMethod();
                                if (i == 0)
                                {
                                    baseAddr = reader.ReadUleb128();
                                    y.MethodId = methodIdList[baseAddr];
                                }
                                else
                                    y.MethodId = methodIdList[reader.ReadUleb128() + baseAddr];

                                y.AccessFlag = (AccessFlags)br.ReadUleb128();
                                int codeOff = br.ReadUleb128();
                                if (codeOff != 0)
                                {
                                    //读取CodeItem对象
                                    br.SeekAndRead(codeOff, SeekOrigin.Begin, (br1) =>
                                    {

                                        ClassData.CodeItem codeItem = new ClassData.CodeItem();
                                        codeItem.InsAdd = br1.BaseStream.Position;
                                        codeItem.RegCount = br1.ReadUInt16();
                                        codeItem.ArgCount = br1.ReadUInt16();
                                        codeItem.OutRegCount = br1.ReadUInt16();
                                        codeItem.TryCount = br1.ReadUInt16();
                                        codeItem._DbgInfoOffset = br1.ReadUInt32();
                                        codeItem.InsnsSize = br1.ReadUInt32();
                                        
                                        for (int j = 0; j < codeItem.InsnsSize; j++)
                                            codeItem.InsnsList.Add(br1.ReadUInt16());
                                        y.CodeItem = codeItem;
                                    });
                                    
                                }
                            
                                classData.VirtualMethods.Add(y);
                            }

                            cid.ClassData = classData;
                        });
                    }
                    

                }


            }

            return file;
        }
        
    }
}
