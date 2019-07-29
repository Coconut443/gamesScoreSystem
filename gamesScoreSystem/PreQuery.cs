using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gamesScoreSystem
{
    class PreQuery
    {
        public enum SortOrder
        {
            Asc,
            Desc
        }
        enum CalcType
        {
            None,
            Sum,
            Count,
            Rank
        }
        enum StreamType
        {
            None,
            Csv
        }
        enum QueryFilterType
        {
            id, eq, gt, lt, ne, gte, lte, contain, regex
        }
        private Session session;
        private string subject;
        private List<Field> fields = new List<Field>();
        private List<Tuple<QueryFilterType, Field, Param>> queryFilters = new List<Tuple<QueryFilterType, Field, Param>>();
        private List<Tuple<SortOrder, Field>> SortFactors = new List<Tuple<SortOrder, Field>>();
        private int limitNum = -1;
        private int skipNum = 0;
        private CalcType calcType = CalcType.None;
        private StreamType streamType = StreamType.None;
        private Entity entity;
        private SortedSet<int> resultIndex;
        private List<int> resultInt;
        private List<string> resultString;
        private int resultNum = 0;
        private bool shouldExec = true;

        public List<int> ResultInt { get => resultInt; set => resultInt = value; }
        public List<string> ResultString { get => resultString; set => resultString = value; }
        public int ResultNum { get => resultNum; set => resultNum = value; }

        public PreQuery(Session session)
        {
            this.session = session;
        }

        public void Init(Function function)
        {
            //注：以下分支仅仅是为了迎合后续代码格式
            if (function.Params == null)
                function.Params = new List<Param>();
            if (session.ExecFunction(function))
            {
                shouldExec = false;
                return;
            }
            else
            {
                subject = function.functionName;
                var database = session.DataBase;
                entity = database.Entities.FirstOrDefault(x => x.Name == subject);
                if (entity == null)
                    throw new Exception("没有名为" + subject + "的实体");
                foreach (IdParam param in function.Params)
                {
                    fields.Add(GetField(param));
                }
                if (fields.Count == 0)
                    fields.Add(null);
            }
        }

        private Field GetField(IdParam param)
        {
            if (param.Value == "id")
                return null;
            var field = Array.Find(entity.Fields, x => x.Name == param.Value);
            if (field == null)
                throw new Exception("实体" + entity.Name + "中不含字段" + param.Value);
            return field;
        }

        public void AddFunction(Function function)
        {
            switch (function.functionName)
            {
                case "id":
                    if (function.Params.Count != 1 || function.Params[0] is StringParam || function.Params[0] is IdParam || function.Params[0] is IdIdParam)
                        throw new Exception("id函数有且仅有单个int型的参数");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.id, null, function.Params[0]));
                    break;
                case "eq":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam || function.Params[1] is QueryParam || function.Params[1] is StringParam))
                        throw new Exception("eq函数参数应为(field,<num>/\"string\")");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.eq, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "gt":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam || function.Params[1] is QueryParam))
                        throw new Exception("gt函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.gt, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "lt":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam || function.Params[1] is QueryParam))
                        throw new Exception("lt函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.lt, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "ne":
                    if (function.Params.Count != 2 || !(function.Params[1] is NumParam || function.Params[1] is QueryParam || function.Params[1] is StringParam))
                        throw new Exception("ne函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.ne, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "gte":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam || function.Params[1] is QueryParam))
                        throw new Exception("gte函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.gte, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "lte":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam || function.Params[1] is QueryParam))
                        throw new Exception("lte函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.lte, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "contain":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is StringParam))
                        throw new Exception("contain函数参数应为(field,\"string\")");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.contain, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "regex":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is StringParam))
                        throw new Exception("regex函数参数应为(field,\"string\")");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.regex, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "asc":
                    if (function.Params.Count != 1 || !(function.Params[0] is IdParam))
                        throw new Exception("asc函数参数应为(field)");
                    SortFactors.Add(new Tuple<SortOrder, Field>(SortOrder.Asc, GetField(function.Params[0] as IdParam)));
                    break;
                case "desc":
                    if (function.Params.Count != 1 || !(function.Params[0] is IdParam))
                        throw new Exception("desc函数参数应为(field)");
                    SortFactors.Add(new Tuple<SortOrder, Field>(SortOrder.Desc, GetField(function.Params[0] as IdParam)));
                    break;
                case "skip":
                    if (function.Params.Count != 1 || !(function.Params[0] is NumParam))
                        throw new Exception("skip函数参数应为(<num>)");
                    skipNum = (function.Params[0] as NumParam).Value;
                    break;
                case "limit":
                    if (function.Params.Count != 1 || !(function.Params[0] is NumParam))
                        throw new Exception("limit函数参数应为(<num>)");
                    limitNum = (function.Params[0] as NumParam).Value;
                    break;
                case "csv":
                    if (function.Params.Count != 1 || !(function.Params[0] is StringParam))
                        throw new Exception("csv函数参数应为(\"path\")");
                    streamType = StreamType.Csv;
                    break;
                case "sum":
                    if (function.Params.Count != 0)
                        throw new Exception("sum函数无参数");
                    calcType = CalcType.Sum;
                    break;
                case "count":
                    if (function.Params.Count != 0)
                        throw new Exception("count函数无参数");
                    calcType = CalcType.Count;
                    break;
                case "rank":
                    if (function.Params.Count != 0)
                        throw new Exception("rank函数无参数");
                    calcType = CalcType.Rank;
                    break;
                default:
                    throw new Exception("不支持的函数");
            }
        }

        public void Exec()
        {
            if (!shouldExec)
                return;
            //第一阶段，进行多轮筛选
            resultIndex = new SortedSet<int>();
            for (int i = 1; i <= entity.Length; ++i)
                resultIndex.Add(i);
            foreach(var filter in queryFilters)
            {
                var queryType = filter.Item1;
                var field = filter.Item2;
                var param = filter.Item3;
                bool isNum = (param is NumParam);
                bool isString = (param is StringParam);
                bool isIdField = (field == null);
                bool isIntField = (field is IntField);
                bool isCharField = (field is CharField);
                int num = isNum ? (param as NumParam).Value : 0;
                string str = isString ? (param as StringParam).Value : "";
                PreQuery query = (isNum || isString) ? null : (param as QueryParam).Value;
                var intField = isIntField ? (field as IntField) : null;
                var charField = isCharField ? (field as CharField) : null;
                if (isNum && !(isIntField || isIdField))
                    throw new Exception("int型的字段必须提供int参数");
                else if (isString && !isCharField)
                    throw new Exception("char(n)型的字段必须提供字符串参数");
                else if (query != null && isIdField && query.entity != entity)
                    throw new Exception("不同实体的id不能进行比较");
                //无论如何都会执行
                if(query != null)
                    query.Exec();
                switch (queryType)
                {
                    case QueryFilterType.id:
                        if(isNum)
                        {
                            var ex = resultIndex.Contains(num);
                            resultIndex.Clear();
                            if (ex)
                                resultIndex.Add(num);
                        }else
                        {
                            resultIndex.RemoveWhere(x => !query.resultIndex.Contains(x));
                        }
                        break;
                    case QueryFilterType.eq:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x - 1]) == num));
                        else if (isString)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => charField[x] == str));
                        else
                        {
                            if (isIdField)
                                resultIndex.IntersectWith(query.resultIndex);
                            else if (isIntField)
                                resultIndex = new SortedSet<int>(resultIndex.Where(x => query.resultInt.Contains(intField.Data[x - 1])));
                            else
                                resultIndex = new SortedSet<int>(resultIndex.Where(x => query.resultString.Contains(charField[x])));
                        }
                        break;
                    case QueryFilterType.gt:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x - 1]) > num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x <= query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x - 1] <= query.resultInt.Min());
                        }
                        break;
                    case QueryFilterType.lt:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x - 1]) < num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x >= query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x - 1] >= query.resultInt.Min());
                        }
                        break;
                    case QueryFilterType.ne:
                        if (isNum)
                            resultIndex.RemoveWhere(x => (isIdField ? x : intField.Data[x - 1]) == num);
                        else if (isString)
                            resultIndex.RemoveWhere(x => charField[x] == str);
                        else
                        {
                            if (isIdField)
                                resultIndex.ExceptWith(query.resultIndex);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => query.resultInt.Contains(intField.Data[x - 1]));
                            else
                                resultIndex.RemoveWhere(x => query.resultString.Contains(charField[x]));
                        }
                        break;
                    case QueryFilterType.gte:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x - 1]) >= num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x < query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x - 1] < query.resultInt.Min());
                        }
                        break;
                    case QueryFilterType.lte:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x - 1]) <= num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x > query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x - 1] > query.resultInt.Min());
                        }
                        break;
                    case QueryFilterType.contain:
                        resultIndex.RemoveWhere(x => !charField[x].Contains(str));
                        break;
                    case QueryFilterType.regex:
                        var regex = new Regex(str);
                        resultIndex.RemoveWhere(x => !regex.IsMatch(charField[x]));
                        break;
                }
            }
            //第二阶段，排序
            var comparer = new IndexComparer(SortFactors);
            resultIndex = new SortedSet<int>(resultIndex,comparer);
            //第三阶段，skip和limit
            var newSet = new SortedSet<int>(comparer);
            int cnt = 0;
            foreach(var index in resultIndex.Skip(skipNum))
            {
                newSet.Add(index);
                if (limitNum > 0 && ++cnt >= limitNum)
                    break;
            }
            resultIndex = newSet;
            //第四阶段，综合
            if (this.fields.Count == 1)
            {
                if(this.fields[0] == null)
                {
                    resultInt = new List<int>(resultIndex);
                }
                else if(this.fields[0] is IntField)
                {
                    var intField = (this.fields[0] as IntField);
                    resultInt = new List<int>();
                    //if(Array.Exists(intField.Constraints,x=>x is ForeignConstraint))
                    var removeDuplicate = new HashSet<int>();
                    foreach (var index in resultIndex)
                    {
                        var value = intField.Data[index - 1];
                        if (!removeDuplicate.Contains(value))
                        {
                            removeDuplicate.Add(value);
                            resultInt.Add(value);
                        }
                    }
                    
                }else if(this.fields[0] is CharField)
                {
                    var charField = (this.fields[0] as CharField);
                    resultString = new List<string>();
                    var removeDuplicate = new HashSet<string>();
                    foreach (var index in resultIndex)
                    {
                        var value = charField[index];
                        if (!removeDuplicate.Contains(value))
                        {
                            removeDuplicate.Add(value);
                            resultString.Add(value);
                        }
                    }
                }
            }
            //第五阶段，计算
            switch (calcType)
            {
                case CalcType.Sum:
                    if(resultInt != null)
                    {
                        var intField = this.fields[0] as IntField;
                        foreach (var index in resultIndex)
                            resultNum += intField.Data[index - 1];
                    }
                    break;
                case CalcType.Count:
                    resultNum = resultIndex.Count;
                    break;
                case CalcType.Rank:
                    if (resultIndex.Count == 0)
                        throw new Exception("由于筛选结果为空，无法计算排名");
                    else if (resultIndex.Count > 1)
                        throw new Exception("由于筛选结果多于一条，无法计算排名");
                    else resultNum = resultIndex.Count(x => comparer.Compare(x, resultIndex.First()) < 0);
                    break;
                default:
                    break;
            }
        }

        public void Output()
        {
            if (!shouldExec)
                return;
            if(streamType == StreamType.None)
            {
                if(calcType != CalcType.None)
                {
                    Console.WriteLine(resultNum);
                }
                else if(fields.Count == 1 && fields[0] is IntField && resultInt != null)
                {
                    var constraint = fields[0].Constraints.FirstOrDefault(x => x is ForeignConstraint);
                    if (constraint != null)
                    {
                        var foreignConstraint = constraint as ForeignConstraint;
                        fields = new List<Field>(foreignConstraint.RefEntity.Fields);
                        foreach (var index in resultInt)
                        {
                            Console.Write("id:" + index);
                            foreach (var field in fields)
                            {
                                if (field != null)
                                    Console.Write(", " + field.Name + ":" + field[index]);
                            }
                            Console.WriteLine();
                        }
                    }
                    else Console.WriteLine(String.Join<int>(",", resultInt));
                }
                else if(fields.Count == 1 && fields[0] is CharField && resultString != null)
                {
                    Console.WriteLine(string.Join(",", resultString));
                }
                else
                {
                    if (fields.Count == 0 || (fields.Count == 1 && fields[0] == null))
                        fields.AddRange(entity.Fields);
                    foreach(var index in resultIndex)
                    {
                        Console.Write("id:" + index);
                        foreach(var field in fields)
                        {
                            if(field != null)
                                Console.Write(", " + field.Name + ":" + field[index]);
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }

    class Function
    {
        public string functionName;
        public List<Param> Params;
    }

    abstract class Param
    {
    }

    class IdParam : Param
    {
        string value;

        public IdParam(string value)
        {
            this.value = value;
        }

        public string Value { get => value; set => this.value = value; }
    }

    class NumParam : Param
    {
        int value;

        public NumParam(int value)
        {
            this.value = value;
        }

        public int Value { get => value; set => this.value = value; }
    }

    class StringParam : Param
    {
        string value;

        public StringParam(string value)
        {
            this.value = value.Trim('\"','\'');
        }

        public string Value { get => value; set => this.value = value; }
    }

    class QueryParam : Param
    {
        PreQuery value;

        public QueryParam(PreQuery value)
        {
            this.value = value;
        }

        internal PreQuery Value { get => value; set => this.value = value; }
    }

    class IdIdParam : Param
    {
        string[] value;

        public IdIdParam(string lhs, string rhs)
        {
            this.value = new string[2] { lhs, rhs };
        }

        public string[] Value { get => value; set => this.value = value; }
    }

    class IndexComparer : IComparer<int>
    {
        private List<Tuple<PreQuery.SortOrder, Field>> sortFactors;
        public IndexComparer(List<Tuple<PreQuery.SortOrder, Field>> sortFactors)
        {
            this.sortFactors = sortFactors;
            sortFactors.Reverse();
        }
        public int Compare(int lhs, int rhs)
        {
            foreach (var factor in sortFactors)
            {
                var sortOrder = factor.Item1;
                var field = factor.Item2;
                if (field == null)
                    return sortOrder == PreQuery.SortOrder.Asc ? lhs - rhs : rhs - lhs;
                else if (field is IntField)
                {
                    var intField = field as IntField;
                    return sortOrder == PreQuery.SortOrder.Asc ?  intField.Data[lhs - 1] - intField.Data[rhs - 1] : intField.Data[rhs - 1] - intField.Data[lhs - 1];
                }
                else if (field is CharField)
                {
                    var charField = field as CharField;
                    return sortOrder == PreQuery.SortOrder.Asc ? String.Compare(charField[lhs], charField[rhs]) : String.Compare(charField[rhs], charField[lhs]);
                }
                else return 0;
            }
            return lhs - rhs;
        }
    }
}
