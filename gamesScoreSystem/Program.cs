//程序的入口
using System;

namespace gamesScoreSystem
{
    /// <summary>
    /// 程序的入口类
    /// </summary>
    class Program
    {
        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args">启动传参</param>
        static void Main(string[] args)
        {
            //启动一个会话
            Session session = new Session(args);
            session.Start();
            //退出
            Console.ReadKey();
        }
    }
}
