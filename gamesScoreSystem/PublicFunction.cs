using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace gamesScoreSystem
{
    class PublicFunction
    {
        //加载XML文档
        //TODO:适当优化当前的混沌写法，使之更具有可读性
        //TODO:为XML文档编写验证文件或方法
        public static DataBase Load(string path)
        {
            //检验文件路径的合法性
            XmlReader reader = XmlReader.Create(path);
            //遍历XML文件两次，第一次读取各实体的字段数与数量，第二次读取数据
            Dictionary<string, int> entityFieldNums = new Dictionary<string, int>();
            Dictionary<string, int> entityNums = new Dictionary<string, int>();
            //读取实体的字段数
            reader.ReadToFollowing("metadata");
            reader.Read();
            while (!reader.EOF && reader.Name != "metadata")
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
            reader.ReadToFollowing("metadata");
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
                        Console.WriteLine(reader.Name, reader.Value);
                        if (reader.Name == "name" && reader.NodeType == XmlNodeType.Element)
                            name = reader.ReadInnerXml();
                        else if (reader.Name == "type" && reader.NodeType == XmlNodeType.Element)
                            type = reader.ReadInnerXml();
                        else if(reader.Name == "constraints" && reader.NodeType == XmlNodeType.Element)
                        {
                            //几种XML方法的混写主要是由于完全不熟练（
                            XElement element = XElement.ReadFrom(reader) as XElement;
                            if (element == null)
                                constraints = new Constraint[0];
                            else
                            {
                                XElement[] constraintNodes = element.Descendants("constraint").ToArray();
                                constraints = new Constraint[constraintNodes.Count()];
                                int k = 0;
                                foreach(var node in constraintNodes)
                                {
                                    string constraintType = node.Attribute("type").Value;
                                    string[] data = node.Descendants("data").Select(x => x.Value).ToArray();
                                    constraints[k++] = ConstraintFactory.Create(constraintType,data);
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
                            field[i] = reader.ReadInnerXml(); ;
                        }
                    }

                }
            }
            return dataBase;
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void Exit()
        {
            Console.WriteLine("bye");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void Welcome()
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
