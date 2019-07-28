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
        }

        public override void EnterExpr([NotNull] InterpreterParser.ExprContext context)
        {
            if (QueryStack.Count == 0)
                QueryStack.Push(rootQuery);
            else
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

        public override void EnterParam([NotNull] InterpreterParser.ParamContext context)
        {
        }

        public override void ExitParam([NotNull] InterpreterParser.ParamContext context)
        {
            ParamStack.Push(CreateParam(context));
            ParamsStack.Peek().Add(ParamStack.Pop());
        }

        public Param CreateParam(InterpreterParser.ParamContext ctx)
        {
            if (ctx.ID().Length == 1)
                return new IdParam(ctx.ID()[0].GetText());
            else if (ctx.ID().Length == 2)
                return new IdIdParam(ctx.ID()[0].GetText(), ctx.ID()[1].GetText());
            else if (ctx.NUM() != null)
                return new NumParam(int.Parse(ctx.NUM().GetText()));
            else if (ctx.STRING() != null)
                return new StringParam(ctx.STRING().GetText());
            else if (ctx.expr() != null)
                return new QueryParam(QueryStack.Peek());
            else throw new Exception("不支持的参数");
        }
    }
}
