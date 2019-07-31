//字段类与字段工厂
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gamesScoreSystem
{
    /// <summary>
    /// 字段基类
    /// </summary>
    abstract class Field
    {
        /// <summary>
        /// 字段的数据数量
        /// </summary>
        private int length = 0;

        /// <summary>
        /// 字段名称
        /// </summary>
        private string name;

        /// <summary>
        /// 字段包含的约束
        /// </summary>
        private Constraint[] constraints;

        /// <summary>
        /// 如果有虚拟约束，字段参考的实体名称
        /// </summary>
        private string refEntityName = "";

        /// <summary>
        /// 字段的构造函数（供子类调用）
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="length">字段数据数量</param>
        /// <param name="constraints">字段包含的约束</param>
        public Field(string name, int length, Constraint[] constraints)
        {
            if (length > 0 && length <= PublicValue.MaxLen)
            {
                this.length = length;
                this.name = name;
                this.constraints = new Constraint[constraints.Length];
                constraints.CopyTo(this.constraints, 0);
            }
            else throw new ArgumentOutOfRangeException("length", "数量" + length + "超出范围1~" + PublicValue.MaxLen + "，或许你可以在设置中改变最大值");
        }

        /// <summary>
        /// 保存字段到二进制流
        /// </summary>
        /// <param name="writer">二进制流</param>
        abstract public void Save(BinaryWriter writer);

        /// <summary>
        /// 从二进制流加载字段
        /// </summary>
        /// <param name="reader">二进制流</param>
        abstract public void Load(BinaryReader reader);

        /// <summary>
        /// 字符串表示的字段索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>相应位置的数据的字符串表示</returns>
        abstract public string this[int index] { get; set; }

        //属性
        public int Length { get => length; }
        public string Name { get => name; set => name = value; }
        internal Constraint[] Constraints { get => constraints; set => constraints = value; }
        public string RefEntityName { get => refEntityName; set => refEntityName = value; }


    }

    /// <summary>
    /// Int型字段
    /// </summary>
    class IntField : Field
    {
        /// <summary>
        /// 数据区
        /// </summary>
        private int[] data;

        /// <summary>
        /// Int型字段构造函数
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="length">字段数据数量</param>
        /// <param name="constraints">字段的约束</param>
        public IntField(string name, int length, Constraint[] constraints) : base(name, length, constraints)
        {
            data = new int[length];
        }

        /// <summary>
        /// Int型字段的索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>相应位置的数据的字符串表示</returns>
        public override string this[int index]
        {
            get
            {
                if (index > 0 && index <= Length)
                {
                    return Data[index - 1].ToString();
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
            set
            {
                if (index > 0 && index <= Length)
                {
                    try
                    {
                        Data[index - 1] = int.Parse(value);
                    }
                    catch (Exception)
                    {
                        throw new Exception("为int型字段" + Name + "赋的值" + value + "不能被转化为int");
                    }
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
        }

        /// <summary>
        /// 从二进制流加载
        /// </summary>
        /// <param name="reader">二进制流</param>
        public override void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            for (int i = 0; i < data.Length; ++i)
                data[i] = reader.ReadInt32();
            if (reader.ReadBoolean())
                this.RefEntityName = reader.ReadString();
        }

        /// <summary>
        /// 保存到二进制流
        /// </summary>
        /// <param name="writer">二进制流</param>
        public override void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            foreach (var d in data)
                writer.Write(d);
            var foreignConstraint = Array.Find(this.Constraints, x => x is ForeignConstraint);
            if (foreignConstraint != null)
            {
                writer.Write(true);
                writer.Write((foreignConstraint as ForeignConstraint).RefEntity.Name);
            }
            else writer.Write(false);
        }

        /// <summary>
        /// 数据区
        /// </summary>
        public int[] Data { get => data; }
    }

    /// <summary>
    /// Char型字段
    /// </summary>
    class CharField : Field
    {
        /// <summary>
        /// 字符串长度
        /// </summary>
        private int charlen;

        /// <summary>
        /// 数据区
        /// </summary>
        private char[] data;

        /// <summary>
        /// Char型字段的构造函数
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="length">字符串数量</param>
        /// <param name="charlen">字符串长度</param>
        /// <param name="constraints">约束</param>
        public CharField(string name, int length, int charlen, Constraint[] constraints) : base(name, length, constraints)
        {
            if (charlen > 0 && charlen <= PublicValue.MaxCharLen)
            {
                this.charlen = charlen;
                data = new char[length * charlen];
            }
            else throw new ArgumentOutOfRangeException("charlen", "数量" + charlen + "超出范围1~" + PublicValue.MaxCharLen + "，或许你可以在设置中改变最大值");
        }

        /// <summary>
        /// Char字段的索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>相应位置的字符串</returns>
        public override string this[int index]
        {
            get
            {
                if (index > 0 && index <= Length)
                {
                    string str = new string(Data, (index - 1) * charlen, charlen);
                    return str.TrimEnd('\0');
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
            set
            {
                if (index > 0 && index <= Length)
                {
                    if (value.Length > charlen)
                    {
                        throw new ArgumentOutOfRangeException("字符串\"" + value + "\"超出长度" + charlen);
                    }
                    else
                    {
                        int i = (index - 1) * charlen;
                        foreach (char c in value)
                        {
                            Data[i++] = c;
                        }
                    }
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
        }

        /// <summary>
        /// 从二进制流加载字段
        /// </summary>
        /// <param name="reader">二进制流</param>
        public override void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            charlen = reader.ReadInt32();
            data = reader.ReadChars(data.Length * charlen);
        }

        /// <summary>
        /// 保存到二进制流
        /// </summary>
        /// <param name="writer">二进制流</param>
        public override void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(charlen);
            writer.Write(data);
        }

        /// <summary>
        /// 数据区
        /// </summary>
        public char[] Data { get => data; }

        /// <summary>
        /// 字符串长度
        /// </summary>
        public int Charlen { get => charlen; }
    }

    /// <summary>
    /// 字段工厂类
    /// </summary>
    class FieldFactory
    {
        /// <summary>
        /// 创建一个字段
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="type">string标识的字段类型</param>
        /// <param name="length">字段的数据数量</param>
        /// <param name="constraints"字段的约束></param>
        /// <returns>创建的字段</returns>
        public static Field Create(string name, string type, int length, Constraint[] constraints)
        {
            if (type.ToLower() == "int")
            {
                return new IntField(name, length, constraints);
            }
            else if (Regex.IsMatch(type.ToLower(), @"^char\([\d]+\)$"))
            {
                int charlen = int.Parse(type.Substring(5, type.Length - 6));
                return new CharField(name, length, charlen, constraints);
            }
            else throw new ArgumentException("类型" + type + "尚未支持，是否使用int或char(<num>)?");
        }
    }
}
