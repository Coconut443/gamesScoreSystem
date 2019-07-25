using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace gamesScoreSystem
{
    class MyTestListener : TestBaseListener
    {
        private int total = 0;
        public override void EnterIntvalue([NotNull] TestParser.IntvalueContext context)
        {
            var intcontext = context.INT();
            int value = intcontext==null ? 0 : int.Parse(intcontext.GetText());
            total += value;
        }
        public override void ExitInit([NotNull] TestParser.InitContext context)
        {
            Console.WriteLine(total);
        }
    }
}
