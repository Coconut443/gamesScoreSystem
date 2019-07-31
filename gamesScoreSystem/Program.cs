using System;

namespace gamesScoreSystem
{
    class Program
    {
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
