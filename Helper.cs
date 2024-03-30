namespace HNIdesu.Dex
{
    public static class Helper
    {
        public static int ReadUleb128(this Stream stream)
        {
            int length = 0;
            int flag = 0;
            while (true)
            {
                int b = stream.ReadByte();
                length |= (b & 0x7F) << (7 * flag++);
                if (b < 0x80)
                    break;
            }
            return length;
        }

        public static int ReadUleb128(this IO.BinaryReader stream)
        {
            int length = 0;
            int flag = 0;
            while (true)
            {
                int b = stream.ReadByte();
                length |= (b & 0x7F) <<( 7 * flag++);
                if (b < 0x80)
                    break;
            }
            return length;
        }
    }
}
