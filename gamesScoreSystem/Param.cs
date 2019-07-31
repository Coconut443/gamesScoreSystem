//参数类
namespace gamesScoreSystem
{
    /// <summary>
    /// 参数基类
    /// </summary>
    abstract class Param
    {
    }

    /// <summary>
    /// 标识符参数类
    /// </summary>
    class IdParam : Param
    {
        /// <summary>
        /// 标识符的值
        /// </summary>
        string value;

        /// <summary>
        /// 标识符参数类的构造函数
        /// </summary>
        /// <param name="value">标识符的值</param>
        public IdParam(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// 标识符的值
        /// </summary>
        public string Value { get => value; set => this.value = value; }
    }

    /// <summary>
    /// 数字参数类
    /// </summary>
    class NumParam : Param
    {
        /// <summary>
        /// 数值
        /// </summary>
        int value;

        /// <summary>
        /// 数字参数类的构造函数
        /// </summary>
        /// <param name="value">数值</param>
        public NumParam(int value)
        {
            this.value = value;
        }

        /// <summary>
        /// 数值
        /// </summary>
        public int Value { get => value; set => this.value = value; }
    }

    /// <summary>
    /// 字符串参数类
    /// </summary>
    class StringParam : Param
    {
        /// <summary>
        /// 字符串的值
        /// </summary>
        string value;

        /// <summary>
        /// 字符串参数类的构造函数
        /// </summary>
        /// <param name="value">字符串的值（两边可以带有双引号）</param>
        public StringParam(string value)
        {
            //也许会导致末尾以引号结尾的字符串出现异常
            this.value = value.Trim('\"', '\'');
        }

        /// <summary>
        /// 字符串的值
        /// </summary>
        public string Value { get => value; set => this.value = value; }
    }

    /// <summary>
    /// 子查询参数类
    /// </summary>
    class QueryParam : Param
    {
        /// <summary>
        /// 子查询的值
        /// </summary>
        PreQuery value;

        /// <summary>
        /// 子查询参数类的构造函数
        /// </summary>
        /// <param name="value">子查询的值</param>
        public QueryParam(PreQuery value)
        {
            this.value = value;
        }

        /// <summary>
        /// 子查询的值
        /// </summary>
        internal PreQuery Value { get => value; set => this.value = value; }
    }

    /// <summary>
    /// Id.Id格式的参数（已弃用）
    /// </summary>
    class IdIdParam : Param
    {
        /// <summary>
        /// string[]形式的值
        /// </summary>
        string[] value;

        /// <summary>
        /// lhs.rhs格式参数的构造函数
        /// </summary>
        /// <param name="lhs">.左边的标识符</param>
        /// <param name="rhs">.右边的标识符</param>
        public IdIdParam(string lhs, string rhs)
        {
            this.value = new string[2] { lhs, rhs };
        }

        /// <summary>
        /// string[]形式的值
        /// </summary>
        public string[] Value { get => value; set => this.value = value; }
    }
}
