using System;
using System.Text.RegularExpressions;

namespace gamesScoreSystem
{
    abstract class Field
    {
        private int length = 0;
        private string name;
        private Constraint[] constraints;
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
        abstract public string this[int index] { get;set; }
        public int Length { get => length; }
        public string Name { get => name; set => name = value; }
        internal Constraint[] Constraints { get => constraints; set => constraints = value; }
    }

    class IntField : Field
    {
        private int[] data;
        public IntField(string name, int length, Constraint[] constraints) : base(name, length, constraints)
        {
            data = new int[length];
        }
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

        public int[] Data { get => data; }
    }

    class CharField : Field
    {
        private int charlen;
        private char[] data;
        public CharField(string name, int length, int charlen, Constraint[] constraints) : base(name, length, constraints)
        {
            if (charlen > 0 && charlen <= PublicValue.MaxCharLen)
            {
                this.charlen = charlen;
                data = new char[length * charlen];
            }
            else throw new ArgumentOutOfRangeException("charlen", "数量" + charlen + "超出范围1~" + PublicValue.MaxCharLen + "，或许你可以在设置中改变最大值");
        }
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

        public char[] Data { get => data; }
        public int Charlen { get => charlen; }
    }

    class FieldFactory
    {
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
