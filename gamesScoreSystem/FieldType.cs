using System;
using System.Text.RegularExpressions;

namespace gamesScoreSystem
{
    class FieldType
    {
        private int length = 0;
        public FieldType(int length)
        {
            if (length > 0 && length <= PublicValue.MaxLen)
            {
                this.length = length;
            }
            else throw new ArgumentOutOfRangeException("length", "数量" + length + "超出范围1~" + PublicValue.MaxLen + "，或许你可以在设置中改变最大值");
        }
        public int Length { get => length; }
    }

    class IntType : FieldType
    {
        private int[] data;
        public IntType(int length):base(length)
        {
            data = new int[length];
        }
        public int this[int index]
        {
            get
            {
                if (index > 0 && index <= Length)
                {
                    return data[index - 1];
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
            set
            {
                if(index > 0 && index <= Length)
                {
                    data[index - 1] = value;
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
        }
    }

    class CharType : FieldType
    {
        private int charlen;
        private char[] data;
        public CharType(int length,int charlen) : base(length)
        {
            if (charlen > 0 && charlen <= PublicValue.MaxCharLen)
            {
                this.charlen = charlen;
                data = new char[length * charlen];
            }
            else throw new ArgumentOutOfRangeException("charlen", "数量" + charlen + "超出范围1~" + PublicValue.MaxCharLen + "，或许你可以在设置中改变最大值");
        }
        public string this[int index]
        {
            get
            {
                if (index > 0 && index <= Length)
                {
                    return new string(data, (index - 1) * charlen, charlen);
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
            set
            {
                if (index > 0 && index <= Length)
                {
                    if(value.Length > charlen)
                    {
                        throw new ArgumentOutOfRangeException("字符串\""+value+"\"超出长度" + charlen);
                    }
                    else
                    {
                        int i = (index - 1) * charlen;
                        foreach(char c in value)
                        {
                            data[i++] = c;
                        }
                    }
                }
                else throw new IndexOutOfRangeException("Id\"" + index + "\"超出范围1~" + Length);
            }
        }
    }

    class FieldTypeFactory
    {
        public static FieldType create(string type, int length)
        {
            if (type.ToLower() == "int")
            {
                return new IntType(length);
            }
            else if (Regex.IsMatch(type.ToLower(), @"^char\([\d]+\)$"))
            {
                int charlen = int.Parse(type.Substring(5, type.Length - 6));
                return new CharType(length, charlen);
            }
            else throw new ArgumentException("类型" + type + "尚未支持，是否使用int或char(<num>)?");
        }
    }
}
