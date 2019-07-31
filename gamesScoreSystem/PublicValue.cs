//全局变量类
namespace gamesScoreSystem
{
    /// <summary>
    /// 全局变量类
    /// 虽然是为了动态修改的方便而存在的类，但是暂时不能动态修改全局变量
    /// </summary>
    class PublicValue
    {
        /// <summary>
        /// 字段中的最大字符串长度
        /// </summary>
        private static int maxCharLen = 256;

        /// <summary>
        /// 最大字段长度
        /// </summary>
        private static int maxLen = 1000000;

        /// <summary>
        /// 最大输出行数限制
        /// </summary>
        private static int outputLimit = 20;

        /// <summary>
        /// 字段中的最大字符串长度
        /// </summary>
        public static int MaxCharLen { get => maxCharLen; set => maxCharLen = value; }

        /// <summary>
        /// 最大字段长度
        /// </summary>
        public static int MaxLen { get => maxLen; set => maxLen = value; }

        /// <summary>
        /// 最大输出行数限制
        /// </summary>
        public static int OutputLimit { get => outputLimit; set => outputLimit = value; }
    }
}
