using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    abstract class Constraint
    {
        private string[] data;
        public Constraint(string[] data)
        {
            this.data = new string[data.Length];
            data.CopyTo(this.data,0);
        }
        public string[] Data { get => data; }
        public abstract void Check(Field field);
    }

    class InConstraint : Constraint
    {
        public InConstraint(string[] data) : base(data)
        {
        }

        public override void Check(Field field)
        {
            //选择采用hash表来减少判断时间
            
            if(field is IntField)
            {
                HashSet<int> vs = new HashSet<int>();
                foreach(var s in Data)
                {
                    try
                    {
                        int v = int.Parse(s);
                        vs.Add(v);
                    }catch(Exception)
                    {
                        throw new Exception("字段" + field.Name + ":对于int类型指定的In约束包含无法转换为Int型的数据" + s);
                    }
                }
                var intField = field as IntField;
                if (Array.Exists(intField.Data, x => !vs.Contains(x)))
                {
                    throw new Exception("in约束校验失败：存在不属于字段"+field.Name+"范围内的值");
                }
            }else if(field is CharField)
            {
                HashSet<string> vs = new HashSet<string>(Data);
                
                var charField = field as CharField;
                for(int i = 1; i <= charField.Length; ++i)
                {
                    if (!vs.Contains(charField[i]))
                    {
                        throw new Exception("in约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                    }
                }
            }
            else
            {
                throw new Exception("未定义的类型");
            }
        }
    }

    class BetweenConstraint : Constraint
    {
        public BetweenConstraint(string[] data) : base(data)
        {
            if(data.Length != 2)
            {
                throw new Exception("between约束提供的数据数应为2，但实际提供数为" + data.Length);
            }
        }

        public override void Check(Field field)
        {
            if (field is IntField)
            {
                int l, r;
                try
                {
                    l = int.Parse(Data[0]);
                    r = int.Parse(Data[1]);
                    if(l > r)
                    {
                        int t = l;
                        l = r;
                        r = t;
                    }
                }
                catch (Exception)
                {
                    throw new Exception("字段"+field.Name+":对于int类型指定的between约束包含无法转换为int型的数据");
                }
                var intField = field as IntField;
                if (Array.Exists(intField.Data, x => x < l || x > r))
                {
                    throw new Exception("between约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                }
            }
            else if (field is CharField)
            {
                throw new Exception("char类型的字段不支持between约束");
            }
            else
            {
                throw new Exception("未定义的类型");
            }
        }
    }

    class ForeignConstraint : Constraint
    {
        public ForeignConstraint(string[] data) : base(data)
        {
            if (data.Length != 1)
            {
                throw new Exception("foreign约束提供的数据数应为1，但实际提供数为" + data.Length);
            }
        }

        public override void Check(Field field)
        {
            //TODO:补全外键约束模块

            if (field is IntField)
            {
                HashSet<int> vs = new HashSet<int>();
                foreach (var s in Data)
                {
                    try
                    {
                        int v = int.Parse(s);
                        vs.Add(v);
                    }
                    catch (Exception)
                    {
                        throw new Exception("字段" + field.Name + ":对于int类型指定的In约束包含无法转换为Int型的数据" + s);
                    }
                }
                var intField = field as IntField;
                if (Array.Exists(intField.Data, x => !vs.Contains(x)))
                {
                    throw new Exception("in约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                }
            }
            else if (field is CharField)
            {
                HashSet<string> vs = new HashSet<string>(Data);

                var charField = field as CharField;
                for (int i = 1; i <= charField.Length; ++i)
                {
                    if (!vs.Contains(charField[i]))
                    {
                        throw new Exception("in约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                    }
                }
            }
            else
            {
                throw new Exception("未定义的类型");
            }
        }
    }

    class VirtualConstraint : Constraint
    {
        public VirtualConstraint(string[] data) : base(data)
        {
            if (data.Length != 1)
            {
                throw new Exception("virtual约束提供的数据数应为1，但实际提供数为" + data.Length);
            }
        }

        public override void Check(Field field)
        {
            //TODO:补全虚拟字段约束模块

            if (field is IntField)
            {
                HashSet<int> vs = new HashSet<int>();
                foreach (var s in Data)
                {
                    try
                    {
                        int v = int.Parse(s);
                        vs.Add(v);
                    }
                    catch (Exception)
                    {
                        throw new Exception("字段" + field.Name + ":对于int类型指定的In约束包含无法转换为Int型的数据" + s);
                    }
                }
                var intField = field as IntField;
                if (Array.Exists(intField.Data, x => !vs.Contains(x)))
                {
                    throw new Exception("in约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                }
            }
            else if (field is CharField)
            {
                HashSet<string> vs = new HashSet<string>(Data);

                var charField = field as CharField;
                for (int i = 1; i <= charField.Length; ++i)
                {
                    if (!vs.Contains(charField[i]))
                    {
                        throw new Exception("in约束校验失败：存在不属于字段" + field.Name + "范围内的值");
                    }
                }
            }
            else
            {
                throw new Exception("未定义的类型");
            }
        }
    }

    class ConstraintFactory
    {
        public static Constraint Create(string type,string[] data)
        {
            switch (type.ToLower())
            {
                case "in":
                    return new InConstraint(data);
                case "between":
                    return new BetweenConstraint(data);
                case "foreign":
                    return new ForeignConstraint(data);
                case "virtual":
                    return new VirtualConstraint(data);
                default:
                    throw new Exception("不支持的约束类型" + type);
            }
        }
    }
}
