using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Welcome();
            Console.ReadKey();
        }
        static void Welcome()
        {
            Console.WriteLine("Welcome to Games Score System. " +
                "Commands end with endline.\n" +
                "Your can only open one instance.\n" +
                "System version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n" +
                "\nCopyright (c) 2019, xyqlx. All rights reserved.\n" +
                "\nType 'help' for help. Type 'clear' to clear the screen.\n");
        }
    }
}
