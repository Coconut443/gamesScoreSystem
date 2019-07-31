//数据库类
using System.IO;
using System.Linq;

namespace gamesScoreSystem
{
    /// <summary>
    /// 数据库类
    /// </summary>
    class DataBase
    {
        /// <summary>
        /// 数据库的标识名
        /// </summary>
        string name;

        /// <summary>
        /// 使用数据库时命令行的标题栏
        /// </summary>
        string title;

        /// <summary>
        /// 数据库包含的实体
        /// </summary>
        Entity[] entities;

        /// <summary>
        /// 数据库级字段
        /// </summary>
        Field[] fields;

        /// <summary>
        /// 对于所有非虚拟字段（即不含有虚拟约束的字段）进行检验
        /// </summary>
        public void Check()
        {
            foreach (var entity in entities)
                entity.Check();
            foreach (var field in fields)
            {
                foreach (var constraint in field.Constraints)
                {
                    if (constraint is VirtualConstraint)
                        break;
                    constraint.Check(field);
                }
            }
        }

        /// <summary>
        /// 对于所有虚拟字段进行计算与检验
        /// </summary>
        public void Calc()
        {
            foreach (var entity in entities)
                entity.Calc();
            foreach (var field in fields)
            {
                bool isVirtual = false;
                foreach (var constraint in field.Constraints)
                {
                    if (constraint is VirtualConstraint)
                        isVirtual = true;
                    else if (!isVirtual)
                        break;
                    constraint.Check(field);
                }
            }
        }

        /// <summary>
        /// 保存数据库到二进制流
        /// </summary>
        /// <param name="writer">二进制流</param>
        public void Save(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(title);
            writer.Write(entities.Length);
            foreach (var entity in entities)
                entity.Save(writer);
            writer.Write(fields.Length);
            foreach (var field in fields)
            {
                writer.Write(field is IntField ? true : false);
                field.Save(writer);
            }

        }

        /// <summary>
        /// 从二进制流加载数据库
        /// </summary>
        /// <param name="reader">二进制流</param>
        public void Load(BinaryReader reader)
        {
            name = reader.ReadString();
            title = reader.ReadString();
            entities = new Entity[reader.ReadInt32()];
            for (int i = 0; i < entities.Length; ++i)
            {
                entities[i] = new Entity("", 0);
                entities[i].Load(reader);
            }

            fields = new Field[reader.ReadInt32()];
            for (int i = 0; i < fields.Length; ++i)
            {
                bool isIntField = reader.ReadBoolean();
                if (isIntField)
                    fields[i] = new IntField("", 1, new Constraint[0]);
                else
                    fields[i] = new CharField("", 1, 1, new Constraint[0]);
                fields[i].Load(reader);
            }
            foreach (var entity in entities)
            {
                foreach (var field in entity.Fields)
                {
                    if (field.RefEntityName != "")
                    {
                        field.Constraints = new Constraint[1];
                        field.Constraints[0] = new ForeignConstraint(new string[1] { field.RefEntityName });
                        var foreignConstraint = field.Constraints[0] as ForeignConstraint;
                        foreignConstraint.RefDataBase = this;
                        foreignConstraint.RefEntity = entities.First(x => x.Name == field.RefEntityName);
                    }
                }
            }
        }

        /// <summary>
        /// 数据库的标识名
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// 使用数据库时命令行的标题栏
        /// </summary>
        public string Title { get => title; set => title = value; }

        /// <summary>
        /// 数据库包含的实体
        /// </summary>
        internal Entity[] Entities { get => entities; set => entities = value; }

        /// <summary>
        /// 数据库级字段
        /// </summary>
        internal Field[] Fields { get => fields; set => fields = value; }
    }
}
