namespace HNIdesu.IO
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute:Attribute
    {
        public int Index { get; set; }
        /// <summary>
        /// 决定字段写入顺序，从小到大
        /// </summary>
        /// <param name="index"></param>
        public RequiredAttribute(int index)
        {
            Index = index;
        }
    }
}
