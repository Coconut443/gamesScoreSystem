//实体类
using System.IO;

namespace gamesScoreSystem
{
    /// <summary>
    /// 实体类
    /// </summary>
    class Entity
    {
        /// <summary>
        /// 实体名
        /// </summary>
        string name;

        /// <summary>
        /// 实体数量
        /// </summary>
        int length;

        /// <summary>
        /// 实体包含的字段
        /// </summary>
        Field[] fields;

        /// <summary>
        /// 实体构造函数
        /// </summary>
        /// <param name="name">实体名称</param>
        /// <param name="length">实体数量</param>
        public Entity(string name, int length)
        {
            this.name = name;
            this.length = length;
        }

        /// <summary>
        /// 校验所有字段
        /// </summary>
        public void Check()
        {
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
        /// 计算所有字段
        /// </summary>
        public void Calc()
        {
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
        /// 保存到二进制流
        /// </summary>
        /// <param name="writer">二进制流</param>
        public void Save(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(length);
            writer.Write(fields.Length);
            foreach (var field in fields)
            {
                writer.Write(field is IntField ? true : false);
                field.Save(writer);
            }
        }

        /// <summary>
        /// 加载到二进制流
        /// </summary>
        /// <param name="reader">二进制流</param>
        public void Load(BinaryReader reader)
        {
            name = reader.ReadString();
            length = reader.ReadInt32();
            var fieldsLength = reader.ReadInt32();
            fields = new Field[fieldsLength];
            for (int i = 0; i < fieldsLength; ++i)
            {
                bool isIntField = reader.ReadBoolean();
                if (isIntField)
                    fields[i] = new IntField("", length, new Constraint[0]);
                else
                    fields[i] = new CharField("", length, 1, new Constraint[0]);
                fields[i].Load(reader);
            }
        }

        /// <summary>
        /// 实体名
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// 实体包含的字段
        /// </summary>
        internal Field[] Fields { get => fields; set => fields = value; }

        /// <summary>
        /// 实体数量
        /// </summary>
        public int Length { get => length; }
    }
}
