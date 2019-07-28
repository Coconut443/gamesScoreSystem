using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace gamesScoreSystem
{
    class QueryInterpreter : InterpreterBaseListener
    {
        public Session session;
        public PreQuery rootQuery;
        public Stack<PreQuery> QueryStack = new Stack<PreQuery>();
        public Stack<Param> ParamStack = new Stack<Param>();
        public Stack<List<Param>> ParamsStack = new Stack<List<Param>>();
        public Stack<Function> FunctionStack = new Stack<Function>();

        AntlrInputStream stream;
        InterpreterLexer lexer;
        CommonTokenStream tokens;
        InterpreterParser parser;

        public QueryInterpreter(Session session)
        {
            this.session = session;
        }

        public bool Interpret(string input)
        {
            //新建一个输入流
            stream = new AntlrInputStream(input + "\n");
            //新建一个词法分析器
            lexer = new InterpreterLexer(stream);
            //新建一个词法符号缓冲区
            tokens = new CommonTokenStream(lexer);
            //新建一个语法分析器
            parser = new InterpreterParser(tokens);
            //针对xxx规则，开始分析
            var tree = parser.prog();

            //var testListener = new MyTestListener();
            //var vistor = new MyGrammarVistor();

            var walker = new ParseTreeWalker();
            walker.Walk(this, tree);

            //var result = vistor.Visit(tree);

            Console.WriteLine(tree.ToStringTree(parser));
            //Console.WriteLine(result);
            return true;
        }

        public override void EnterStat([NotNull] InterpreterParser.StatContext context)
        {
            rootQuery = new PreQuery(session);
            QueryStack.Push(rootQuery);
        }

        public override void EnterExpr([NotNull] InterpreterParser.ExprContext context)
        {
            QueryStack.Push(new PreQuery(session));
        }

        public override void ExitExpr([NotNull] InterpreterParser.ExprContext context)
        {
            QueryStack.Pop();
        }

        public override void EnterSubject([NotNull] InterpreterParser.SubjectContext context)
        {
            FunctionStack.Push(new Function());
            FunctionStack.Peek().functionName = context.ID().GetText();
        }

        public override void ExitSubject([NotNull] InterpreterParser.SubjectContext context)
        {
            QueryStack.Peek().Init(FunctionStack.Pop());
        }

        public override void EnterFunction([NotNull] InterpreterParser.FunctionContext context)
        {
            FunctionStack.Push(new Function());
        }

        public override void ExitFunction([NotNull] InterpreterParser.FunctionContext context)
        {
            QueryStack.Peek().AddFunction(FunctionStack.Pop());
        }

        public override void EnterParams([NotNull] InterpreterParser.ParamsContext context)
        {
            ParamsStack.Push(new List<Param>());
        }

        public override void ExitParams([NotNull] InterpreterParser.ParamsContext context)
        {
            FunctionStack.Peek().Params = ParamsStack.Pop();
        }

        //TODO:增加字段级的判断
        public override void EnterParam([NotNull] InterpreterParser.ParamContext context)
        {
            var type = context.GetText();
            Console.WriteLine(type);
            ParamStack.Push(ParamFactory.Create(1));
        }
        public override void ExitParam([NotNull] InterpreterParser.ParamContext context)
        {
            ParamsStack.Peek().Add(ParamStack.Pop());
        }
    }
}
