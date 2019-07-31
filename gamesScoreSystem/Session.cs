//会话类
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace gamesScoreSystem
{
    /// <summary>
    /// 会话类
    /// </summary>
    class Session
    {
        /// <summary>
        /// 数据库路径字典
        /// </summary>
        Dictionary<string, string> dataBasePath;

        /// <summary>
        /// 当前会话设置（未使用）
        /// </summary>
        Dictionary<string, string> settings;

        /// <summary>
        /// 当前会话的主查询解释器
        /// </summary>
        public QueryInterpreter queryInterpreter;

        /// <summary>
        /// 当前会话的主计数器
        /// </summary>
        Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 当前使用的数据库
        /// </summary>
        DataBase dataBase;

        /// <summary>
        /// 程序传入参数
        /// </summary>
        private string[] args;

        /// <summary>
        /// 是否输入了Clear指令
        /// </summary>
        bool isCleard = false;

        /// <summary>
        /// 空构造函数
        /// </summary>
        public Session()
        {
        }

        /// <summary>
        /// 有传入参数的构造函数
        /// </summary>
        /// <param name="args">传入参数</param>
        public Session(string[] args)
        {
            this.args = args;
        }

        /// <summary>
        /// 当前使用的数据库
        /// </summary>
        internal DataBase DataBase { get => dataBase; set => dataBase = value; }

        /// <summary>
        /// 启动会话
        /// </summary>
        public void Start()
        {
            try
            {
                Welcome();
                LoadSettings();
                queryInterpreter = new QueryInterpreter(this);
                if (args != null && args.Length == 1)
                    Load(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (true)
            {
                try
                {
                    Cycle();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

        }

        /// <summary>
        /// 进入查询循环
        /// </summary>
        public void Cycle()
        {
            Console.Write("\n" + DateTime.Now.ToShortTimeString() + " > ");
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
                        Console.WriteLine(String.Format("({0:0}ms)", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
                    break;
                }
                Console.Write("      > ");
            }

        }

        /// <summary>
        /// 加载数据库路径
        /// </summary>
        private void LoadSettings()
        {
            dataBasePath = new Dictionary<string, string>();
            if (File.Exists("settings.ini"))
            {
                FileStream fileStream = new FileStream("settings.ini", FileMode.Open);
                StreamReader streamReader = new StreamReader(fileStream);
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var seg = line.Split('=');
                    if (seg.Length == 2)
                        dataBasePath[seg[0]] = seg[1];
                }
                fileStream.Close();
            }
        }

        /// <summary>
        /// 保存数据库路径
        /// </summary>
        private void SaveSettings()
        {
            FileStream fileStream = new FileStream("settings.ini", FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            foreach (var key in dataBasePath)
                streamWriter.WriteLine(key.Key + "=" + key.Value);
            streamWriter.Close();
        }

        //以下都已经鸽了
        //TODO:适当优化当前的混乱写法，使之更具有可读性
        //TODO:为XML文档编写验证文件或方法
        //TODO:添加关键词过滤器，不允许与关键词一致
        /// <summary>
        /// 加载XML数据库文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public void Load(string path)
        {
            //进行计时
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //检验文件路径的合法性
            XmlReader reader = XmlReader.Create(path);
            //检查XML中的数据库是否与已有数据库重名
            reader.ReadToFollowing("database");
            string dataBaseName = reader.GetAttribute("name");
            if (dataBasePath.ContainsKey(dataBaseName))
                throw new Exception("已有与导入的数据库" + dataBaseName + "重名的数据库");
            string title = reader.GetAttribute("title");
            if (title != null && title != "")
                Console.Title = title;
            //载入文件报时
            Console.WriteLine(String.Format("数据文件发现，耗时{0:0}ms，正在进行载入...", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
            stopwatch.Reset();
            stopwatch.Start();
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
            while (!reader.EOF && reader.Name != "fields")
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
                while (reader.Name != entityName + "s")
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == entityName)
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
                if (reader.Name == "settings")
                {
                    break;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    settings.Add(reader.Name, reader.Value);
                }
            }
            DataBase dataBase = new DataBase();
            dataBase.Name = dataBaseName;
            dataBase.Title = title;
            //读取实体与字段
            reader.ReadToFollowing("entities");
            int entityCnt = entityNums.Count;
            dataBase.Entities = new Entity[entityCnt];
            int entityIndex = 0;
            for (int i = 0; i < entityCnt; ++i)
            {
                reader.ReadToFollowing("entity");
                string entityName = reader.GetAttribute("name");
                int fieldNum = entityFieldNums[entityName];
                int entityNum = entityNums[entityName];
                Entity entity = new Entity(entityName, entityNum);
                Field[] fields = new Field[fieldNum];
                int fieldIndex = 0;
                for (int j = 0; j < fieldNum; ++j)
                {
                    string name = "";
                    string type = "";
                    Constraint[] constraints = null;

                    reader.ReadToFollowing("field");

                    while (reader.Read() && !reader.EOF && reader.Name != "field")
                    {
                        if (reader.Name == "name" && reader.NodeType == XmlNodeType.Element)
                            name = reader.ReadInnerXml();
                        else if (reader.Name == "type" && reader.NodeType == XmlNodeType.Element)
                            type = reader.ReadInnerXml();
                        else if (reader.Name == "constraints" && reader.NodeType == XmlNodeType.Element)
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
                                foreach (var node in constraintNodes)
                                {
                                    string constraintType = node.Attribute("type").Value;
                                    string[] data = node.Descendants("data").Select(x => x.Value).ToArray();
                                    var constraint = ConstraintFactory.Create(constraintType, data);
                                    if (constraint is VirtualConstraint)
                                    {
                                        var virtualConstraint = constraint as VirtualConstraint;
                                        virtualConstraint.RefSession = this;
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
                    if (constraints == null)
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
            foreach (var fieldNode in fieldNodes)
            {
                var constraintNodes = fieldNode.Descendants("constraint").ToArray();
                var constraints = new Constraint[constraintNodes.Length];
                int constraintsIndex = 0;
                foreach (var node in constraintNodes)
                {
                    string constraintType = node.Attribute("type").Value;
                    string[] data = node.Descendants("data").Select(x => x.Value).ToArray();
                    var constraint = ConstraintFactory.Create(constraintType, data);
                    if (constraint is VirtualConstraint)
                    {
                        var virtualConstraint = constraint as VirtualConstraint;
                        virtualConstraint.RefSession = this;
                    }
                    constraints[constraintsIndex++] = constraint;
                }
                dataBase.Fields[dbFieldIndex++] = FieldFactory.Create(fieldNode.Descendants("name").First().Value, fieldNode.Descendants("type").First().Value, 1, constraints);
            }
            //读取数据
            reader.ReadToFollowing("entitydata");
            foreach (Entity entity in dataBase.Entities)
            {
                for (int i = 1; i <= entity.Length; ++i)
                {
                    foreach (Field field in entity.Fields)
                    {
                        if (!Array.Exists(field.Constraints, x => x is VirtualConstraint))
                        {
                            reader.ReadToFollowing(field.Name);
                            field[i] = reader.ReadInnerXml();
                        }
                    }

                }
            }
            reader.ReadToFollowing("fielddata");
            foreach (Field field in dataBase.Fields)
            {
                if (!Array.Exists(field.Constraints, x => x is VirtualConstraint))
                {
                    reader.ReadToFollowing(field.Name);
                    field[1] = reader.ReadInnerXml();
                }
            }
            this.dataBase = dataBase;
            //关闭文件流
            reader.Close();
            //载入文件报时
            Console.WriteLine(String.Format("数据已载入，耗时{0:0}ms", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
            stopwatch.Reset();
            stopwatch.Start();
            //检查数据完整性
            dataBase.Check();
            Console.WriteLine(String.Format("数据完整性检查完成，耗时{0:0}ms，开始计算虚拟列...", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
            stopwatch.Reset();
            stopwatch.Start();
            //生成虚拟列
            dataBase.Calc();
            Console.WriteLine(String.Format("虚拟列计算完成，耗时{0:0}ms，开始转化为数据文件...", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
            stopwatch.Reset();
            stopwatch.Start();
            //转为数据文件
            if (!dataBasePath.ContainsKey(dataBaseName))
            {
                BinaryWriter writer = new BinaryWriter(new FileStream(dataBaseName + ".data", FileMode.Create));
                dataBase.Save(writer);
                writer.Close();

            }
            SaveSettings();
            dataBasePath[dataBaseName] = dataBaseName + ".data";
            Console.WriteLine(String.Format("数据文件保存成功，耗时{0:0}ms", 1000 * stopwatch.ElapsedTicks / (decimal)Stopwatch.Frequency));
        }

        /// <summary>
        /// 清屏
        /// </summary>
        public void Clear()
        {
            Console.Clear();
            isCleard = true;
        }

        /// <summary>
        /// 退出会话
        /// </summary>
        public void Exit()
        {
            Console.WriteLine("bye");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// 输出初始信息
        /// </summary>
        public void Welcome()
        {
            Console.WriteLine("Welcome to Games Score System. " +
                "Commands end with endline.\n" +
                "Your can only open one instance.\n" +
                "System version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n" +
                "\nCopyright (c) 2019, xyqlx. All rights reserved.\n" +
                "\nType 'help' for help. Type 'clear' to clear the screen.\n");
        }

        /// <summary>
        /// 帮助函数（并没有完成）
        /// </summary>
        public void Help()
        {
            Console.WriteLine("这里是全部的帮助信息");
        }

        /// <summary>
        /// 特定于某一指令的帮助函数（并没有完成）
        /// </summary>
        /// <param name="cmd"></param>
        public void Help(string cmd)
        {
            Console.WriteLine("这里是" + cmd + "的帮助信息");
        }

        /// <summary>
        /// 输出数据库、实体、字段等信息
        /// </summary>
        /// <param name="param">信息参数</param>
        public void Show(Param param)
        {
            if (param is IdParam)
            {
                var value = (param as IdParam).Value;
                List<string> results = new List<string>();
                switch (value)
                {
                    case "database":
                    case "databases":
                        OutputData("databases", dataBasePath.Keys);
                        break;
                    case "entity":
                    case "entities":
                        if (dataBase == null)
                            throw new Exception("你还没有加载任何数据库");
                        foreach (var entity in dataBase.Entities)
                            results.Add(entity.Name);
                        OutputData("entities", results);
                        break;
                    default:
                        try
                        {
                            foreach (var field in dataBase.Entities.First(x => x.Name == value).Fields)
                                results.Add(field.Name);
                            OutputData(value + ".fields", results);
                        }
                        catch (Exception)
                        {
                            throw new Exception("不存在名为" + value + "的实体");
                        }
                        break;
                }
            }
            //目前取消Id.Id这一设定
            //}else if(param is IdIdParam)
            //{
            //    var value = (param as IdIdParam).Value;
            //    var entity = dataBase.Entities.FirstOrDefault(x => x.Name == value[0]);
            //    if (entity == null)
            //        throw new Exception("不存在名为" + value + "的实体");
            //    var field = entity.Fields.FirstOrDefault(x => x.Name == value[1]);
            //    if (field == null)
            //        throw new Exception(entity.Name + "不存在名为" + value[1] + "的字段");

            //}
            else Help("show");
        }

        /// <summary>
        /// 格式化输出器
        /// </summary>
        /// <param name="name">表头</param>
        /// <param name="str">单个数据</param>
        public static void OutputData(string name, string str)
        {
            int len = Encoding.GetEncoding("gb2312").GetBytes(name + str).Length;
            string divideLine = "+-" + new string('-', len + 3) + "-+";
            Console.WriteLine(divideLine);
            Console.WriteLine("| " + name + " | " + str + " |");
            Console.WriteLine(divideLine);
        }

        /// <summary>
        /// 格式化输出器
        /// </summary>
        /// <param name="name">表头</param>
        /// <param name="vs">一列数据</param>
        public static void OutputData(string name, IEnumerable<string> vs)
        {
            int cnt = 0;
            List<string> list = new List<string>();
            foreach (var str in vs)
            {
                ++cnt;
                if (cnt > PublicValue.OutputLimit)
                {
                    list.Add("...");
                    break;
                }
                list.Add(str);
            }
            if (cnt == 0)
            {
                Console.WriteLine("no " + name + ".");
                return;
            }
            //注意处理中文占两个长度的问题
            var maxLen = list.Max(x => Encoding.GetEncoding("gb2312").GetBytes(x).Length);
            if (name.Length > maxLen)
                maxLen = name.Length;
            var divideLine = "+-" + new string('-', maxLen) + "-+";
            Console.WriteLine(divideLine);
            Console.WriteLine("| " + name + new string(' ', maxLen - Encoding.GetEncoding("gb2312").GetBytes(name).Length) + " |");
            Console.WriteLine(divideLine);
            foreach (var str in list)
                Console.WriteLine("| " + str + new string(' ', maxLen - Encoding.GetEncoding("gb2312").GetBytes(str).Length) + " |");
            Console.WriteLine(divideLine);
            if (cnt > PublicValue.OutputLimit)
            {
                Console.Write(">");
                --cnt;
            }

            Console.Write(cnt + " rows ");
        }

        /// <summary>
        /// 格式化输出器
        /// </summary>
        /// <param name="nameList">表头</param>
        /// <param name="vss">多行列数据</param>
        public static void OutputData(IEnumerable<string> nameList, IEnumerable<IEnumerable<string>> vss)
        {
            string[] names = nameList.ToArray();
            int[] maxLens = nameList.Select(x => Encoding.GetEncoding("gb2312").GetBytes(x).Length).ToArray();
            List<string[]> lss = new List<string[]>();
            int cnt = 0;
            foreach (var vs in vss)
            {
                ++cnt;
                if (cnt > PublicValue.OutputLimit)
                {
                    Console.WriteLine("...");
                    break;
                }
                lss.Add(vs.ToArray());
            }
            foreach (var ls in lss)
            {
                for (int i = 0; i < maxLens.Length; ++i)
                {
                    var len = Encoding.GetEncoding("gb2312").GetBytes(ls[i]).Length;
                    if (len > maxLens[i])
                        maxLens[i] = len;
                }
            }
            var divideLine = "+" + String.Join("+", maxLens.Select(x => new string('-', x + 2))) + "+";
            Console.WriteLine(divideLine);
            for (int i = 0; i < maxLens.Length; ++i)
                Console.Write("| " + names[i] + new string(' ', maxLens[i] - Encoding.GetEncoding("gb2312").GetBytes(names[i]).Length) + " ");
            Console.WriteLine("|");
            Console.WriteLine(divideLine);
            foreach (var ls in lss)
            {
                for (int i = 0; i < maxLens.Length; ++i)
                    Console.Write("| " + ls[i] + new string(' ', maxLens[i] - Encoding.GetEncoding("gb2312").GetBytes(ls[i]).Length) + " ");
                Console.WriteLine("|");
            }
            Console.WriteLine(divideLine);
            if (cnt > PublicValue.OutputLimit)
            {
                Console.Write(">");
                --cnt;
            }

            Console.Write(cnt + " rows ");
        }

        /// <summary>
        /// 转到某个在数据库路径中的数据库
        /// </summary>
        /// <param name="databaseName">数据库标识名</param>
        public void Use(string databaseName)
        {
            if (!dataBasePath.ContainsKey(databaseName))
                throw new Exception("不存在名为" + databaseName + "的数据库");
            dataBase = new DataBase();
            BinaryReader reader = new BinaryReader(new FileStream(dataBasePath[databaseName], FileMode.Open));
            dataBase.Load(reader);
            if (dataBase.Title != "")
                Console.Title = dataBase.Title;
            reader.Close();
            SaveSettings();
            Console.WriteLine("数据库" + databaseName + "加载完成");
        }

        /// <summary>
        /// 删除某个在数据库路径中的数据库记录及其文件
        /// </summary>
        /// <param name="databaseName"></param>
        public void Drop(string databaseName)
        {
            if (!dataBasePath.ContainsKey(databaseName))
                throw new Exception("不存在名为" + databaseName + "的数据库");
            dataBasePath.Remove(databaseName);
            if (File.Exists(databaseName + ".data"))
                File.Delete(databaseName + ".data");
            Console.WriteLine("数据库" + databaseName + "已被删除");
            dataBase = null;
        }

        /// <summary>
        /// 执行全局函数
        /// </summary>
        /// <param name="function">待执行的函数</param>
        /// <returns>是否执行了全局函数</returns>
        public bool ExecFunction(Function function)
        {
            switch (function.functionName)
            {
                case "load":
                    if (function.Params.Count != 1 || !(function.Params[0] is StringParam))
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
                    if (function.Params.Count != 1 || !(function.Params[0] is IdParam))
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
