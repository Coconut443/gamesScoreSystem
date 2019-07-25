using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace gamesScoreSystem
{
    class MyGrammarVistor : TestBaseVisitor<Object>
    {
        public override object VisitInit([NotNull] TestParser.InitContext context)
        {
            return context.GetText();
        }
    }
}
