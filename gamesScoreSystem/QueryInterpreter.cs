//查询解释器类
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace gamesScoreSystem
{
    /// <summary>
    /// 查询解释器/监听器类
    /// </summary>
    class QueryInterpreter : InterpreterBaseListener
    {
        /// <summary>
        /// 解释器属于的会话
        /// </summary>
        public Session session;

        /// <summary>
        /// 解释器的根查询
        /// </summary>
        public PreQuery rootQuery;

        /// <summary>
        /// 解释器的查询栈
        /// </summary>
        public Stack<PreQuery> QueryStack = new Stack<PreQuery>();

        /// <summary>
        /// 解释器的参数栈
        /// </summary>
        public Stack<Param> ParamStack = new Stack<Param>();

        /// <summary>
        /// 解释器的参数列表栈
        /// </summary>
        public Stack<List<Param>> ParamsStack = new Stack<List<Param>>();

        /// <summary>
        /// 解释器的函数栈
        /// </summary>
        public Stack<Function> FunctionStack = new Stack<Function>();

        /// <summary>
        /// 记录上一个经过的查询（主要是为了录入参数的方便）
        /// </summary>
        PreQuery lastQuery;

        /// <summary>
        /// 解释器是否应当对会话进行输出
        /// </summary>
        public bool fade = false;

        /// <summary>
        /// 语法分析输入流
        /// </summary>
        AntlrInputStream stream;

        /// <summary>
        /// 词法分析器
        /// </summary>
        InterpreterLexer lexer;

        /// <summary>
        /// 词法符号缓冲区
        /// </summary>
        CommonTokenStream tokens;

        /// <summary>
        /// 语法分析器
        /// </summary>
        InterpreterParser parser;

        /// <summary>
        /// 查询解释器的构造函数
        /// </summary>
        /// <param name="session">属于的会话</param>
        public QueryInterpreter(Session session)
        {
            this.session = session;
        }

        /// <summary>
        /// 解释一个查询程序
        /// </summary>
        /// <param name="input">查询程序代码</param>
        /// <returns>是否运行了语句</returns>
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
            //针对程序规则，开始分析
            var tree = parser.prog();

            //清空栈
            QueryStack.Clear();
            ParamsStack.Clear();
            ParamStack.Clear();
            FunctionStack.Clear();

            //输出解析结果
            //Console.WriteLine(tree.ToStringTree(parser));

            var walker = new ParseTreeWalker();
            //遍历树并触发监听器
            walker.Walk(this, tree);

            return true;
        }

        //以下函数表示进入/退出某一个语法的事件

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
            lastQuery = QueryStack.Pop();
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
            FunctionStack.Peek().functionName = context.ID().GetText();
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

        public override void ExitStat([NotNull] InterpreterParser.StatContext context)
        {
            rootQuery.Exec();
            if (!fade)
                rootQuery.Output();
        }

        /// <summary>
        /// 根据读取到param语法内容创建参数
        /// </summary>
        /// <param name="ctx">读取到的语法内容</param>
        /// <returns>创建的参数</returns>
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
                return new QueryParam(lastQuery);
            else throw new Exception("不支持的参数");
        }
    }
}
