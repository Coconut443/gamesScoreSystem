using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    class PublicFunction
    {
        public static Entity Load(string path)
        {

            return new Entity("");
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void Exit()
        {
            Console.WriteLine("bye");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
