using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace HNIdesu.Dex
{
    public partial class DexFile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct DexHeaderStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] magic;
            public uint checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] signature;
            public uint fileSize;
            public uint headerSize;
            public uint endianTag;
            public uint linkSize;
            public uint linkOff;
            public uint mapOff;
            public uint stringIdsSize;
            public uint stringIdsOff;
            public uint typeIdsSize;
            public uint typeIdsOff;
            public uint protoIdsSize;
            public uint protoIdsOff;
            public uint fieldIdsSize;
            public uint fieldIdsOff;
            public uint methodIdsSize;
            public uint methodIdsOff;
            public uint classDefsSize;
            public uint classDefsOff;
            public uint dataSize;
            public uint dataOff;
        }
        public class StringId(string value)
        {
            public string Value = value;
        }
        public class TypeId(StringId typeName)
        {
            public StringId Name = typeName;
        }

        public class ProtoId(StringId shortyId, TypeId retType)
        {
            public StringId ShortyId=shortyId;
            public TypeId ReturnType=retType;
        }

        public class MethodId(TypeId classType, ProtoId proto, StringId methodName)
        {
            public TypeId Class = classType;
            public StringId Name = methodName;
            public ProtoId Proto = proto;
        }
        public class FieldId(TypeId classType, TypeId fieldType, StringId fieldName)
        {
            public TypeId Class = classType;
            public TypeId Field = fieldType;
            public StringId Name = fieldName;
        }
        public class ClassId(TypeId classType,TypeId superClassType,StringId? sourceFileName,ClassData? classData)
        {
            public TypeId Class = classType;
            public TypeId SuperClass = superClassType;
            public StringId? SourceFileName = sourceFileName;
            public ClassData? ClassData = classData;
        }

        [Flags]
        public enum AccessFlags
        {
            ACC_PUBLIC =        0x0001,
            ACC_PRIVATE =       0x0002,
            ACC_PROTECTED =     0x0004,
            ACC_STATIC =        0x0008,
            ACC_FINAL=          0x0010,
            ACC_VOLATILE=       0x0040,
            ACC_TRANSIENT=      0x0080,
            ACC_CONSTRUCTOR =   0x10000,
            ACC_ENUM=           0x40000
        }
        public class ClassData(
            ClassData.ClassDataHeader classDataHeader
        )
        {
            public class ClassDataHeader(
                int staticFieldsSize, 
                int instanceFieldsSize,
                int directMethodsSize,
                int virtualMethodsSize)
            {
                public int StaticFieldsSize = staticFieldsSize;
                public int InstanceFieldsSize = instanceFieldsSize;
                public int DirectMethodsSize = directMethodsSize;
                public int VirtualMethodsSize = virtualMethodsSize;
            }
            public class ClassMethod(MethodId methodId,AccessFlags accessFlags,CodeItem? codeItem)
            {
                public MethodId MethodId = methodId;
                public AccessFlags AccessFlag = accessFlags;
                public CodeItem? CodeItem = codeItem;
            }

            public class ClassField(FieldId fieldId,AccessFlags accessFlags)
            {
                public FieldId FieldId = fieldId;
                public AccessFlags AccessFlag = accessFlags;
                public override string ToString() => FieldId.ToString()!;
            }
            public class CodeItem(
                ushort regCount,
                ushort argCount,
                ushort outRegCount,
                ushort tryCount,
                uint insnsSize,
                uint dbgInfoOffset,
                long insAdd,
                byte[] insnsData
            )
            {
                public byte[] InsnsData = insnsData;
                public ushort RegCount = regCount;
                public ushort ArgCount = argCount;
                public ushort OutRegCount = outRegCount;
                public uint DbgInfoOffset = dbgInfoOffset;
                public ushort TryCount = tryCount;
                public uint InsnsSize = insnsSize;
                public long InsAdd = insAdd;
            }

            public ClassDataHeader Header = classDataHeader;
            public List<ClassMethod> DirectMethods = new List<ClassMethod>(classDataHeader.DirectMethodsSize);
            public List<ClassMethod> VirtualMethods = new List<ClassMethod>(classDataHeader.VirtualMethodsSize);
            public List<ClassField> StaticFielids = new List<ClassField>(classDataHeader.StaticFieldsSize);
            public List<ClassField> InstanceFields = new List<ClassField>(classDataHeader.InstanceFieldsSize);

        }
    }
}
