using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace gamesScoreSystem
{
    class QueryInterpreter
    {
        public bool Interpret(string input)
        {
            //新建一个输入流
            var stream = new AntlrInputStream(input + "\n");
            //新建一个词法分析器
            var lexer = new InterpreterLexer(stream);
            //新建一个词法符号缓冲区
            var tokens = new CommonTokenStream(lexer);
            //新建一个语法分析器
            var parser = new InterpreterParser(tokens);
            //针对xxx规则，开始分析
            var tree = parser.prog();

            //var testListener = new MyTestListener();
            //var vistor = new MyGrammarVistor();

            //var walker = new ParseTreeWalker();
            //walker.Walk(testListener, tree);

            //var result = vistor.Visit(tree);

            Console.WriteLine(tree.ToStringTree(parser));
            //Console.WriteLine(result);
            return true;
        }
    }
}
