using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace HNIdesu.IO
{
    internal abstract class StringReader
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public abstract string ReadString(BinaryReaderEx reader);
    }
    internal sealed class BinaryReaderEx(Stream stream) : BinaryReader(stream)
    {
        private StringReader? _StringReader = null;
        public StringReader? StringReader {
            get=>_StringReader;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _StringReader = value;
            }
        }

        public new string ReadString()
        {
            if (StringReader == null)
                return base.ReadString();
            else
                return StringReader.ReadString(this);
        }

        /// <summary>
        /// 读取出定长结构体
        /// </summary>
        /// <typeparam name="T">结构体的类型</typeparam>
        /// <param name="size">结构体的大小</param>
        /// <returns></returns>
        public T ReadMarshal<T>() where T : struct
        {
            int size = Marshal.SizeOf<T>();
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

    }
}
