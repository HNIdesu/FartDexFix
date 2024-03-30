using System.Runtime.InteropServices;

namespace HNIdesu.Dex
{
    public partial class DexFile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DexHeaderStruct
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
        public class StringId
        {
            public string Value;
            public StringId(string value)
            {
                Value = value;
            }
        }
        public class TypeId
        {
            public StringId Name;
            public TypeId(StringId a)
            {
                Name = a;
            }
        }

        public class ProtoId
        {
            public StringId ShortyIdx;
            public TypeId ReturnType;
            public ProtoId(StringId a, TypeId b)
            {
                ShortyIdx = a;
                ReturnType = b;
            }

        }

        public class MethodId
        {
            public TypeId Class;
            public StringId Name;
            public ProtoId Proto;
            public MethodId(TypeId c, StringId n, ProtoId p)
            {
                Class = c;
                Name = n;
                Proto = p;
            }

        }
        public class FieldId
        {

        }
        public class ClassId
        {
            public TypeId Class;
            public TypeId SuperClass;
            public StringId SourceFileName;
            public ClassData ClassData;
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
        public class ClassData
        {
            public class ClassDataHeader
            {
                public int StaticFieldsSize; //静态字段个数
                public int InstanceFieldsSize;//实例字段个数
                public int DirectMethodsSize;//直接方法个数
                public int VirtualMethodsSize;//虚方法个数

            }
            public class ClassMethod
            {
                public MethodId MethodId;
                public AccessFlags AccessFlag;
                public CodeItem CodeItem;

            }


            public class ClassField
            {
                public FieldId FieldId;
                public AccessFlags AccessFlag;

            }
            public class CodeItem
            {
                public ushort RegCount;
                public ushort ArgCount;
                public ushort OutRegCount;
                public ushort TryCount;
                public uint _DbgInfoOffset;
                public uint InsnsSize;
                public List<ushort> InsnsList = new List<ushort>();
                public long InsAdd;
            }

            public ClassDataHeader Header;
            public List<ClassMethod> DirectMethods = new List<ClassMethod>();
            public List<ClassMethod> VirtualMethods = new List<ClassMethod>();
            public List<ClassField> StaticFielids = new List<ClassField>();
            public List<ClassField> InstanceFields = new List<ClassField>();

        }
    }
}
