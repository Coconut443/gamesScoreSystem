using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace gamesScoreSystem
{
    class Session
    {
        Dictionary<string, string> dataBasePath;
        public QueryInterpreter queryInterpreter;
        Stopwatch stopwatch = new Stopwatch();
        DataBase dataBase;

        bool isCleard = false;

        internal DataBase DataBase { get => dataBase; set => dataBase = value; }

        public void Start()
        {
            Welcome();
            LoadSettings();
            queryInterpreter = new QueryInterpreter(this);
            //TODO:删除下行（用于方便测试）
            Load("../../../gameScore.xml");
            while (true)
            {
                Cycle();
            }
        }

        public void Cycle()
        {
            Console.Write(DateTime.Now.ToShortTimeString() + " > ");
            while (true)
            {
                string input = Console.ReadLine();
                stopwatch.Reset();
                stopwatch.Start();
                if (queryInterpreter.Interpret(input))
                {
                    stopwatch.Stop();
                    if (isCleard)
                        isCleard = false;
                    else
                        Console.WriteLine(String.Format("执行时间{0:0.00}ms", stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
                    break;
                }
                Console.Write("      > ");
            }
            
        }

        private void LoadSettings()
        {
            dataBasePath = new Dictionary<string, string>();
            //TODO:添加加载/创建文件的操作
        }

        //加载XML文档
        //TODO:适当优化当前的混沌写法，使之更具有可读性
        //TODO:为XML文档编写验证文件或方法
        //TODO:添加关键词过滤器，不允许与关键词一致
        public void Load(string path)
        {
            //检验文件路径的合法性
            XmlReader reader = XmlReader.Create(path);
            //检查XML中的数据库是否与已有数据库重名
            reader.ReadToFollowing("database");
            string dataBaseName = reader.GetAttribute("name");
            if (dataBasePath.ContainsKey(dataBaseName))
            {
                throw new Exception("已有与导入的数据库" + dataBaseName + "重名的数据库");
            }
            else
            {
                //TODO:创建数据文件，etc.
                dataBasePath[dataBaseName] = "";
            }
            //遍历XML文件两次，第一次读取各实体的字段数与数量，第二次读取数据
            Dictionary<string, int> entityFieldNums = new Dictionary<string, int>();
            Dictionary<string, int> entityNums = new Dictionary<string, int>();
            //读取实体的字段数
            reader.ReadToFollowing("entities");
            reader.Read();
            while (!reader.EOF && reader.Name != "entities")
            {
                do
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                } while (reader.Read());
                string entityName = reader.GetAttribute("name");
                int count = 0;
                reader.Read();
                while (reader.Name != "entity")
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "field")
                    {
                        ++count;
                    }
                    reader.Read();
                }
                reader.Read();
                entityFieldNums[entityName] = count;
                do
                {
                    if (reader.Name != "")
                        break;
                } while (reader.Read());
            }
            //读取数据库级字段的数量
            reader.ReadToFollowing("fields");
            entityFieldNums["database"] = 0;
            while(!reader.EOF && reader.Name != "fields")
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "field")
                    entityFieldNums["database"]++;
            }
            //读取实体的数量
            reader.ReadToFollowing("entitydata");
            reader.Read();
            while (!reader.EOF && reader.Name != "entitydata")
            {
                do
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                } while (reader.Read());
                string entityName = reader.Name.Substring(0, reader.Name.Length - 1);
                int count = 0;
                reader.Read();
                while (reader.Name != entityName + "s") {
                    if(reader.NodeType == XmlNodeType.Element &&  reader.Name == entityName)
                    {
                        ++count;
                    }
                    reader.Read();
                }
                reader.Read();
                entityNums[entityName] = count;
                do
                {
                    if (reader.Name != "")
                        break;
                } while (reader.Read());
            }
            reader.Dispose();
            //第二次读取
            Dictionary<string, string> settings = new Dictionary<string, string>();
            reader = XmlReader.Create(path);
            //读取设置
            reader.ReadToFollowing("settings");
            while (reader.Read())
            {
                if(reader.Name == "settings")
                {
                    break;
                }
                if(reader.NodeType == XmlNodeType.Element)
                {
                    settings.Add(reader.Name, reader.Value);
                }
            }
            DataBase dataBase = new DataBase();
            //读取实体与字段
            reader.ReadToFollowing("entities");
            int entityCnt = entityNums.Count;
            dataBase.Entities = new Entity[entityCnt];
            int entityIndex = 0;
            for(int i = 0; i < entityCnt; ++i)
            {
                reader.ReadToFollowing("entity");
                string entityName = reader.GetAttribute("name");
                int fieldNum = entityFieldNums[entityName];
                int entityNum = entityNums[entityName];
                Entity entity = new Entity(entityName, entityNum);
                Field[] fields = new Field[fieldNum];
                int fieldIndex = 0;
                for(int j = 0; j < fieldNum; ++j)
                {
                    string name = "";
                    string type = "";
                    Constraint[] constraints = null;
                    
                    reader.ReadToFollowing("field");
                    
                    while(reader.Read() && !reader.EOF && reader.Name != "field")
                    {
                        if (reader.Name == "name" && reader.NodeType == XmlNodeType.Element)
                            name = reader.ReadInnerXml();
                        else if (reader.Name == "type" && reader.NodeType == XmlNodeType.Element)
                            type = reader.ReadInnerXml();
                        else if(reader.Name == "constraints" && reader.NodeType == XmlNodeType.Element)
                        {
                            //几种XML方法的混写主要是由于完全不熟练（
                            XElement element = XNode.ReadFrom(reader) as XElement;
                            if (element == null)
                                constraints = new Constraint[0];
                            else
                            {
                                XElement[] constraintNodes = element.Descendants("constraint").ToArray();
                                constraints = new Constraint[constraintNodes.Length];
                                int k = 0;
                                foreach(var node in constraintNodes)
                                {
                                    string constraintType = node.Attribute("type").Value;
                                    string[] data = node.Descendants("data").Select(x => x.Value).ToArray();
                                    var constraint = ConstraintFactory.Create(constraintType,data);
                                    if(constraint is VirtualConstraint)
                                    {
                                        var virtualConstraint = constraint as VirtualConstraint;
                                        virtualConstraint.RefDataBase = dataBase;
                                    }
                                    else if (constraint is ForeignConstraint)
                                    {
                                        var foreignConstraint = constraint as ForeignConstraint;
                                        foreignConstraint.RefDataBase = dataBase;
                                    }
                                    constraints[k++] = constraint;
                                }
                            }
                        }
                    }
                    if(constraints == null)
                    {
                        constraints = new Constraint[0];
                    }
                    fields[fieldIndex++] = FieldFactory.Create(name, type, entityNum, constraints);
                }
                entity.Fields = fields;
                dataBase.Entities[entityIndex++] = entity;
            }
            //读取数据库级字段
            reader.ReadToFollowing("fields");
            XElement fieldsElement = XNode.ReadFrom(reader) as XElement;
            var fieldNodes = fieldsElement.Descendants("field").ToArray();
            int dbFieldNum = fieldNodes.Length;
            dataBase.Fields = new Field[dbFieldNum];
            int dbFieldIndex = 0;
            foreach(var fieldNode in fieldNodes)
            {
                var constraintNodes = fieldNode.Descendants("constraint").ToArray();
                var constraints = new Constraint[constraintNodes.Length];
                int constraintsIndex = 0;
                foreach(var node in constraintNodes)
                {
                    string constraintType = node.Attribute("type").Value;
                    string[] data = node.Descendants("data").Select(x => x.Value).ToArray();
                    var constraint = ConstraintFactory.Create(constraintType, data);
                    if (constraint is VirtualConstraint)
                    {
                        var virtualConstraint = constraint as VirtualConstraint;
                        virtualConstraint.RefDataBase = dataBase;
                    }
                    constraints[constraintsIndex++] = constraint;
                }
                dataBase.Fields[dbFieldIndex++] = FieldFactory.Create(fieldNode.Descendants("name").First().Value, fieldNode.Descendants("type").First().Value, 1, constraints);
            }
            //读取数据
            reader.ReadToFollowing("entitydata");
            foreach(Entity entity in dataBase.Entities) {
                for(int i = 1; i <= entity.Length; ++i)
                {
                    foreach(Field field in entity.Fields)
                    {
                        if(!Array.Exists(field.Constraints,x=>x is VirtualConstraint))
                        {
                            reader.ReadToFollowing(field.Name);
                            field[i] = reader.ReadInnerXml();
                        }
                    }

                }
            }
            reader.ReadToFollowing("fielddata");
            foreach(Field field in dataBase.Fields)
            {
                if(!Array.Exists(field.Constraints, x=>x is VirtualConstraint))
                {
                    reader.ReadToFollowing(field.Name);
                    field[1] = reader.ReadInnerXml();
                }
            }
            this.dataBase = dataBase;
            dataBase.Check();
        }

        public void Clear()
        {
            Console.Clear();
            isCleard = true;
        }

        public void Exit()
        {
            Console.WriteLine("bye");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public void Welcome()
        {
            Console.WriteLine("Welcome to Games Score System. " +
                "Commands end with endline.\n" +
                "Your can only open one instance.\n" +
                "System version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n" +
                "\nCopyright (c) 2019, xyqlx. All rights reserved.\n" +
                "\nType 'help' for help. Type 'clear' to clear the screen.\n");
        }

        //TODO:补全帮助函数
        public void Help()
        {
            Console.WriteLine("这里是全部的帮助信息");
        }

        public void Help(string cmd)
        {
            Console.WriteLine("这里是"+cmd+"的帮助信息");
        }

        //TODO:补全Show，Use，Drop函数
        public void Show(Param param)
        {

        }

        public void Use(string databaseName)
        {

        }

        public void Drop(string databaseName)
        {

        }

        public bool ExecFunction(Function function)
        {
            switch (function.functionName)
            {
                case "load":
                    if(function.Params.Count != 1 || !(function.Params[0] is StringParam))
                        throw new Exception("load函数参数错误，应为load(\"path\")");
                    Load((function.Params[0] as StringParam).Value);
                    break;
                case "clear":
                    if (function.Params.Count != 0)
                        throw new Exception("clear函数参数错误，应为clear()");
                    Clear();
                    break;
                case "help":
                    if (function.Params.Count == 0)
                        Help();
                    else if (function.Params.Count == 1 && function.Params[0] is IdParam)
                        Help((function.Params[0] as IdParam).Value);
                    else throw new Exception("help函数参数错误，应为help()或help(cmd)");
                    break;
                case "exit":
                    if (function.Params.Count != 0)
                        throw new Exception("exit函数参数错误，应为exit()");
                    Exit();
                    break;
                case "show":
                    if (function.Params.Count != 1)
                        throw new Exception("show函数参数错误，应为show(...)");
                    Show(function.Params[0]);
                    break;
                case "use":
                    if(function.Params.Count != 1 || !(function.Params[0] is IdParam))
                        throw new Exception("use函数参数错误，应为use(database)");
                    Use((function.Params[0] as IdParam).Value);
                    break;
                case "drop":
                    if (function.Params.Count != 1 || !(function.Params[0] is IdParam))
                        throw new Exception("drop函数参数错误，应为drop(database)");
                    Drop((function.Params[0] as IdParam).Value);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
