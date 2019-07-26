﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


namespace gamesScoreSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            PublicFunction.Welcome();

            TestLoad();
            
            Console.ReadKey();
        }

        static void TestLoad()
        {
            PublicFunction.Load("../../../gameScore.xml");
        }

        static void TestFieldType()
        {
            Field fieldType = FieldFactory.Create("charTest","char(10)", 10,new Constraint[0]);
            try
            {
                var charType = fieldType as CharField;
                charType[1] = "char";
                Console.WriteLine(charType[1]);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void TestGrammar()
        {
            string input = "{1,2,{34},{4,5}}";

            //新建一个输入流
            var stream = new AntlrInputStream(input);
            //新建一个词法分析器
            var lexer = new TestLexer(stream);
            //新建一个词法符号缓冲区
            var tokens = new CommonTokenStream(lexer);
            //新建一个语法分析器
            var parser = new TestParser(tokens);
            //针对xxx规则，开始分析
            var tree = parser.init();

            var testListener = new MyTestListener();
            var vistor = new MyGrammarVistor();

            var walker = new ParseTreeWalker();
            walker.Walk(testListener, tree);

            var result = vistor.Visit(tree);

            Console.WriteLine(tree.ToStringTree(parser));
            //Console.WriteLine(result);

        }

        
    }
}
