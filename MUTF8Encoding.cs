using System.Text;

namespace HNIdesu.Dex
{
    public sealed class MUTF8Encoding : Encoding
    {
        private static MUTF8Encoding? instance;
        public static MUTF8Encoding Instance
        {
            get
            {
                if (instance == null)
                    instance = new MUTF8Encoding();
                return instance;
            }
        }


        public override int GetByteCount(char[] chars, int index, int count)
        {

            int result = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = chars[i + index];
                if (ch != 0 && ch < 0x7F)
                    result ++;
                else if (0x80<ch&&ch < 0x7FF || ch==0)
                    result += 2;
                else if(ch>0x800 && ch<0xFFFF)
                    result += 3;
                else
                    throw new Exception();
            }
            return result;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int offset = 0;
            for (int i = 0; i < charCount; i++)
            {
                char ch = chars[charIndex + i];
                if (ch == 0)
                {
                    bytes[offset++ + byteIndex] = 0xD0;
                    bytes[offset++ + byteIndex] = 0x80;
                }
                else if (ch > 0 && ch <= 0x7F)
                {
                    bytes[offset++ + byteIndex] = ASCII.GetBytes(ch + "")[0];
                }
                else if (ch > 0x80 && ch < 0x7FF)
                {
                    bytes[offset++ + byteIndex] = (byte)(0xC0 | (ch >> 6));
                    bytes[offset++ + byteIndex] = (byte)(0x80 | (ch & 0x3F));
                }
                else
                {
                    bytes[offset++ + byteIndex] = (byte)(0xE0 | (ch >> 12));
                    bytes[offset++ + byteIndex] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                    bytes[offset++ + byteIndex] = (byte)(0x80 | (ch & 0x3F));
                }

            }
            return offset;

        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            int result = 0;
            for (int i = 0; i < count;)
            {
                byte b = bytes[index + i];
                if (b > 0 && b <= 0x7F)
                    i++;
                else if (b >= 0xC0 && b < 0xE0)
                    i += 2;
                else if (b >= 0xE0 && b<0xF0)
                    i += 3;
                else
                    throw new Exception();
                result++;
            }
            return result;
        }



        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {

            int destOffset = 0;
            for (int sourOffset = 0; sourOffset < byteCount;)
            {
                if (bytes[sourOffset + byteIndex] > 0 && bytes[sourOffset + byteIndex] <= 0x7F)
                {
                    chars[charIndex + destOffset++] = (char)(bytes[sourOffset + byteIndex]);
                    sourOffset++;
                }
                else if (bytes[sourOffset + byteIndex] >= 0xC0 && bytes[sourOffset + byteIndex] < 0xE0)
                {
                    if (bytes[sourOffset + byteIndex] == 0xD0 && bytes[sourOffset + byteIndex + 1] == 0x80)
                        chars[charIndex + destOffset++] = (char)0;
                    else
                        chars[charIndex + destOffset++] = (char)(((bytes[sourOffset + byteIndex] & 0x1F) << 6) | (bytes[sourOffset + byteIndex + 1] & 0x3F));
                    sourOffset += 2;
                }
                else if (bytes[sourOffset + byteIndex] >= 0xE0)
                {
                    chars[charIndex + destOffset++] = (char)(((bytes[sourOffset + byteIndex] & 0x0F) << 12) | ((bytes[sourOffset + byteIndex + 1] & 0x3F) << 6) | (bytes[sourOffset + byteIndex + 2] & 0x3F));
                    sourOffset += 3;
                }
            }
            return destOffset;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 3;//A character obtains 3 bytes at most.
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}
