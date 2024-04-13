using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HNIdesu.Dex
{
    public partial class DexFile
    {
        public List<StringId> StringIdList { get; private set; }
        public List<TypeId> TypeIdList { get; private set; }
        public List<ProtoId> ProtoIdList { get; private set; }
        public List<MethodId> MethodIdList { get; private set; }
        public List<ClassId> ClassIdList { get; private set; }
        public List<FieldId> FieldIdList { get; private set; }
        private DexFile(
            List<StringId> stringIdList,
            List<TypeId> typeIdList,
            List<ProtoId> protoIdList,
            List<MethodId> methodIdList,
            List<ClassId> classIdList,
            List<FieldId> fieldIdList
        )
        {
            StringIdList = stringIdList;
            TypeIdList = typeIdList;
            ProtoIdList = protoIdList;
            MethodIdList = methodIdList;
            ClassIdList = classIdList;
            FieldIdList = fieldIdList;
        }

        public static DexFile Parse(string path)
        {
            List<StringId> stringIdList;
            List<TypeId> typeIdList;
            List<ProtoId> protoIdList;
            List<MethodId> methodIdList;
            List<ClassId> classIdList;
            List<FieldId> fieldIdList;
            
            using (var reader = new HNIdesu.IO.BinaryReaderEx(File.OpenRead(path)) {
                StringReader= new Uleb128StringReader()
            })
            { 
                DexHeaderStruct dexHeader = reader.ReadMarshal<DexHeaderStruct>();
                stringIdList = new List<StringId>((int)dexHeader.stringIdsSize);
                typeIdList = new List<TypeId>((int)dexHeader.typeIdsSize);
                protoIdList = new List<ProtoId>((int)dexHeader.protoIdsSize);
                methodIdList = new List<MethodId>((int)dexHeader.methodIdsSize);
                classIdList = new List<ClassId>((int)dexHeader.classDefsSize);
                fieldIdList = new List<FieldId>((int)dexHeader.fieldIdsSize);

                #region 获取String ids
                reader.BaseStream.Seek(dexHeader.stringIdsOff,SeekOrigin.Begin);
                var stringIdOffsetList =new int[(int)dexHeader.stringIdsSize];
                for (int i = 0,count=(int)dexHeader.stringIdsSize; i < count; i++)
                    stringIdOffsetList[i]=reader.ReadInt32();
                for (int i = 0, count = (int)dexHeader.stringIdsSize; i < count; i++)
                {
                    int pos = stringIdOffsetList[i];
                    if (pos != reader.BaseStream.Position)
                        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    stringIdList.Add(new StringId(reader.ReadString()));
                }
                #endregion

                #region 获取Type ids
                reader.BaseStream.Seek(dexHeader.typeIdsOff, SeekOrigin.Begin);
                for (int i=0,count=(int)dexHeader.typeIdsSize; i< count; i++)
                {
                    typeIdList.Add(new TypeId(stringIdList[reader.ReadInt32()]));
                }
                #endregion

                #region 获取Proto ids
                reader.BaseStream.Seek(dexHeader.protoIdsOff, SeekOrigin.Begin);
                for (int i = 0,count=(int)dexHeader.protoIdsSize; i < count; i++)
                {
                    protoIdList.Add(new ProtoId(
                        shortyId: stringIdList[reader.ReadInt32()], 
                        retType: typeIdList[reader.ReadInt32()]
                     ));
                    int paramsOffset=reader.ReadInt32();
                }
                #endregion

                #region 获取Field ids
                reader.BaseStream.Seek(dexHeader.fieldIdsOff, SeekOrigin.Begin);
                for (int i = 0,count=(int)dexHeader.fieldIdsSize; i < count; i++)
                {
                    fieldIdList.Add(new FieldId(
                        classType:typeIdList[reader.ReadInt16()],
                        fieldType:typeIdList[reader.ReadInt16()],
                        fieldName: stringIdList[reader.ReadInt32()]
                    ));
                }
                #endregion

                #region 获取Method ids
                reader.BaseStream.Seek(dexHeader.methodIdsOff, SeekOrigin.Begin);
                for (int i = 0,count=(int)dexHeader.methodIdsSize; i < count; i++)
                {
                    methodIdList.Add(new MethodId(
                        classType: typeIdList[reader.ReadInt16()], 
                        proto: protoIdList[reader.ReadInt16()],
                        methodName: stringIdList[reader.ReadInt32()]
                     ));
                }
                #endregion

                #region 获取Class ids
                var classMethodCodeItemOffsetDict = new Dictionary<ClassData.ClassMethod,int>();
                reader.BaseStream.Seek(dexHeader.classDefsOff, SeekOrigin.Begin);
                var classDataOffsetList = new int[dexHeader.classDefsSize];
                for (int i = 0,count=(int)dexHeader.classDefsSize; i < count; i++)
                {
                    var class_idx= reader.ReadInt32();
                    var access_flags =reader.ReadInt32();
                    var superclass_ids =reader.ReadInt32();
                    var interfaces_off =reader.ReadInt32();
                    var source_file_idx =reader.ReadInt32();
                    var annotations_off =reader.ReadInt32();
                    var class_data_off = reader.ReadInt32();
                    var static_values_off = reader.ReadInt32();
                    var superClassType = typeIdList[superclass_ids];
                    var classType= typeIdList[class_idx];
                    StringId? sourceFileName = null;
                    if (source_file_idx != -1)
                        sourceFileName = stringIdList[source_file_idx];
                    classDataOffsetList[i] = class_data_off; 
                    classIdList.Add(new ClassId(
                        classType:classType,
                        superClassType:superClassType,
                        sourceFileName:sourceFileName,
                        classData:null
                    ));
                }
                for(int i = 0, count = classDataOffsetList.Length; i < count; i++)
                {
                    ClassData? classData = null;
                    var classDataOffset = classDataOffsetList[i];
                    if (classDataOffset == 0) continue;
                    if (reader.BaseStream.Position != classDataOffset)
                        reader.BaseStream.Seek(classDataOffset, SeekOrigin.Begin);
                    var classDataHeader = new ClassData.ClassDataHeader(
                            staticFieldsSize: reader.ReadUleb128(),
                            instanceFieldsSize: reader.ReadUleb128(),
                            directMethodsSize: reader.ReadUleb128(),
                            virtualMethodsSize: reader.ReadUleb128()
                        );
                    classData = new ClassData(classDataHeader);
                    // 静态字段
                    for (int j = 0; j < classDataHeader.StaticFieldsSize; j++)
                    {
                        reader.ReadUleb128();
                        reader.ReadUleb128();
                    }
                    //实例字段
                    for (int j = 0; j < classDataHeader.InstanceFieldsSize; j++)
                    {
                        reader.ReadUleb128();
                        reader.ReadUleb128();
                    }
                    //直接方法
                    int baseAddr = 0;
                    for (int j = 0; j < classDataHeader.DirectMethodsSize; j++)
                    {
                        MethodId methodId;
                        if (j == 0)
                            methodId = methodIdList[(baseAddr = reader.ReadUleb128())];
                        else
                            methodId = methodIdList[reader.ReadUleb128() + baseAddr];

                        var accessFlags = (AccessFlags)reader.ReadUleb128();
                        var codeItemOffset = reader.ReadUleb128();
                        var classMethod = new ClassData.ClassMethod(
                            methodId: methodId,
                            accessFlags: accessFlags,
                            codeItem: null
                        );
                        if (codeItemOffset != 0 && codeItemOffset < reader.BaseStream.Length)
                            classMethodCodeItemOffsetDict.Add(classMethod, codeItemOffset);
                        classData.DirectMethods.Add(classMethod);
                    }
                    //虚方法
                    for (int j = 0; j < classDataHeader.VirtualMethodsSize; j++)
                    {
                        MethodId methodId;
                        if (j == 0)
                            methodId = methodIdList[(baseAddr = reader.ReadUleb128())];
                        else
                            methodId = methodIdList[reader.ReadUleb128() + baseAddr];
                        var accessFlags = (AccessFlags)reader.ReadUleb128();
                        var codeItemOffset = reader.ReadUleb128();
                        var classMethod = new ClassData.ClassMethod(
                            methodId: methodId,
                            accessFlags: accessFlags,
                            codeItem: null
                        );
                        if (codeItemOffset != 0 && codeItemOffset < reader.BaseStream.Length)
                            classMethodCodeItemOffsetDict.Add(classMethod, codeItemOffset);
                        classData.VirtualMethods.Add(classMethod);
                    }
                    classIdList[i].ClassData = classData;
                    
                }

                foreach(var pair in classMethodCodeItemOffsetDict)
                {
                    if (reader.BaseStream.Position != pair.Value)
                        reader.BaseStream.Seek(pair.Value, SeekOrigin.Begin);
                    var insAdd = reader.BaseStream.Position;
                    var regCount = reader.ReadUInt16();
                    var argCount = reader.ReadUInt16();
                    var outArgCount = reader.ReadUInt16();
                    var tryCount = reader.ReadUInt16();
                    var dbgInfoOffset = reader.ReadUInt32();
                    var insnsSize = reader.ReadUInt32();
                    var insnsData = reader.ReadBytes((int)(insnsSize * 2));
                    var codeItem = new ClassData.CodeItem(
                        insAdd: insAdd,
                        regCount: regCount,
                        argCount: argCount,
                        outRegCount: outArgCount,
                        tryCount: tryCount,
                        dbgInfoOffset: dbgInfoOffset,
                        insnsSize: insnsSize,
                        insnsData: insnsData
                    );
                    pair.Key.CodeItem = codeItem;
                }
                
                #endregion

            }

            return new DexFile(stringIdList,typeIdList,protoIdList,methodIdList,classIdList,fieldIdList);
        }
        
    }
}
