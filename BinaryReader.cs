using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace HNIdesu.IO
{
    public class BinaryReader:IDisposable
    {
        public Stream BaseStream { get; set; }
        public bool IsBigEndian { get; set; } = false;
        public StringReader StringReader { get; set; }=new DefaultStringReader();

        public BinaryReader(Stream stream)
        {
            BaseStream = stream;
        }
        public int ReadSByte()
        {
            return (sbyte)BaseStream.ReadByte();
        }
        public sealed class DefaultStringReader : StringReader
        {
            public override string ReadString(Stream stream)
            {
                List<byte> bytes = new List<byte>();
                int b;
                while ((b = stream.ReadByte()) != 0)
                    bytes.Add((byte)b);
                return Encoding.GetString(bytes.ToArray());
            }
        }


        public uint ReadUInt32()
        {
            byte[] buffer = new byte[4];
            BaseStream.Read(buffer, 0, 4);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt32(buffer, 0);
        }

        public ulong ReadUInt64()
        {
            byte[] buffer = new byte[8];
            BaseStream.Read(buffer, 0, 8);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            BaseStream.Read(buffer, 0, 4);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt32(buffer, 0);
        }

        public float ReadSingle()
        {
            byte[] buffer = ReadBytes(4);
            if (IsBigEndian)
                buffer = buffer.Reverse().ToArray();
            return BitConverter.ToSingle(buffer);
        }
        public double ReadDouble()
        {
            byte[] buffer = ReadBytes(8);
            if (IsBigEndian)
                buffer = buffer.Reverse().ToArray();
            return BitConverter.ToDouble(buffer);
        }
        public long ReadInt64()
        {
            byte[] buffer = new byte[8];
            BaseStream.Read(buffer, 0, 8);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[2];
            BaseStream.Read(buffer, 0, 2);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt16(buffer, 0);
        }

        public ushort ReadUInt16()
        {
            byte[] buffer = new byte[2];
            BaseStream.Read(buffer, 0, 2);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt16(buffer,0);
        }


        public void Read(Action<BinaryReader> action)
        {
            action(this);
        }
        public T Read<T>(Func<BinaryReader,T> func)
        {
            return func(this);
        }


        public byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            BaseStream.Read(buffer, 0, count);
            return buffer;
        }

        public byte[] ReadBytes(int count,int offset,SeekOrigin origin)
        {
            long mark = BaseStream.Position;
            BaseStream.Seek(offset, origin);
            byte[] buffer = new byte[count];
            BaseStream.Read(buffer, 0, count);
            BaseStream.Position = mark;
            return buffer;
        }


        public int ReadByte()
        {
            return BaseStream.ReadByte();
        }



        /// <summary>
        /// 读取某位置的以空字符结尾字符串
        /// </summary>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public string ReadString()
        {
            return StringReader.ReadString(BaseStream);
        }

        public void SeekAndRead(long offset,SeekOrigin seekOrigin,Action<BinaryReader> action)
        {
            long mark= BaseStream.Position;
            BaseStream.Seek(offset,seekOrigin);
            action(this);
            BaseStream.Position=mark;
            return;
        }

        public void Close()
        {
            BaseStream.Close();          
        }

        /// <summary>
        /// 读取出定长结构体
        /// </summary>
        /// <typeparam name="T">结构体的类型</typeparam>
        /// <param name="size">结构体的大小</param>
        /// <returns></returns>
        public T ReadMarshal<T>(int size) where T : struct
        {
            byte[] buffer = new byte[size];
            int readed= BaseStream.Read(buffer, 0, size);
            if (readed == 0)
                throw new EndOfStreamException();
            IntPtr ptr= Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            T temp=Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return temp;
        }

        /// <summary>
        /// 更自由的读取类的方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void ReadClass<T>(T item) where T : class
        {
            SortedList<int, FieldInfo> sortedList = new();
            foreach(var fieldInfo in item.GetType().GetFields())
            {
                object[] array;
                if ((array=fieldInfo.GetCustomAttributes(typeof(RequiredAttribute), true)).Length != 0)
                {
                    RequiredAttribute attr = (RequiredAttribute)array[0];
                    sortedList.Add(attr.Index,fieldInfo);
                }
            }
            foreach(var fieldInfo in sortedList)
            {
                Type t = fieldInfo.Value.FieldType;
                if (t == typeof(int))
                    fieldInfo.Value.SetValue(item, ReadInt32());
                else if (t == typeof(uint))
                    fieldInfo.Value.SetValue(item, ReadUInt32());
                else if (t == typeof(long))
                    fieldInfo.Value.SetValue(item, ReadInt64());
                else if (t == typeof(ulong))
                    fieldInfo.Value.SetValue(item, ReadUInt64());
                else if (t == typeof(byte))
                    fieldInfo.Value.SetValue(item, ReadByte());
                else if (t == typeof(short))
                    fieldInfo.Value.SetValue(item, ReadInt16());
                else if (t == typeof(ushort))
                    fieldInfo.Value.SetValue(item, ReadUInt16());
                else if (t == typeof(string))
                    fieldInfo.Value.SetValue(item, ReadString());
                else
                    throw new NotSupportedException("不支持的类型");

            }
            return;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
