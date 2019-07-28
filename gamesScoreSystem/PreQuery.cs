using System;
using System.Collections.Generic;
using System.Linq;

namespace gamesScoreSystem
{
    class PreQuery
    {
        enum SortOrder
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
        private SortedSet<int> resultInt;
        private SortedSet<string> resultString;
        private int resultNum = 0;
        public PreQuery(Session session)
        {
            this.session = session;
        }

        public void Init(Function function)
        {
            if (function.Params != null && session.ExecFunction(function))
                return;
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
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam) || !(function.Params[1] is StringParam))
                        throw new Exception("eq函数参数应为(field,<num>/\"string\")");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.eq, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "gt":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam))
                        throw new Exception("gt函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.gt, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "lt":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam))
                        throw new Exception("lt函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.lt, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "ne":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam))
                        throw new Exception("ne函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.ne, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "gte":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam))
                        throw new Exception("gte函数参数应为(field,<num>)");
                    queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.gte, GetField(function.Params[0] as IdParam), function.Params[1]));
                    break;
                case "lte":
                    if (function.Params.Count != 2 || !(function.Params[0] is IdParam) || !(function.Params[1] is NumParam) || !(function.Params[1] is QueryParam))
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

        //TODO:完成执行模块
        public void Exec()
        {
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
                else if (isIdField && query.entity != entity)
                    throw new Exception("不同实体的id不能进行比较");
                //无论如何都会执行
                query.Exec();
                switch (queryType)
                {
                    case QueryFilterType.id:
                        if(isNum)
                        {
                            resultIndex.Clear();
                            if (resultIndex.Contains(num))
                                resultIndex.Add(num);
                        }else
                        {
                            resultIndex.RemoveWhere(x => !query.resultIndex.Contains(x));
                        }
                        break;
                    case QueryFilterType.eq:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x]) == num));
                        else if (isString)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => charField[x] == str));
                        else
                        {
                            if (isIdField)
                                resultIndex.IntersectWith(query.resultIndex);
                            else if (isIntField)
                                resultIndex = new SortedSet<int>(resultIndex.Where(x => query.resultInt.Contains(intField.Data[x])));
                            else
                                resultIndex = new SortedSet<int>(resultIndex.Where(x => query.resultString.Contains(charField[x])));
                        }
                        break;
                    case QueryFilterType.gt:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x]) > num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x <= query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x] <= query.resultInt.Min);
                        }
                        break;
                    case QueryFilterType.lt:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x]) < num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x >= query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x] >= query.resultInt.Min);
                        }
                        break;
                    case QueryFilterType.ne:
                        if (isNum)
                            resultIndex.RemoveWhere(x => (isIdField ? x : intField.Data[x]) == num);
                        else if (isString)
                            resultIndex.RemoveWhere(x => charField[x] == str);
                        else
                        {
                            if (isIdField)
                                resultIndex.ExceptWith(query.resultIndex);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => query.resultInt.Contains(intField.Data[x]));
                            else
                                resultIndex.RemoveWhere(x => query.resultString.Contains(charField[x]));
                        }
                        break;
                    case QueryFilterType.gte:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x]) >= num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x < query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x] < query.resultInt.Min);
                        }
                        break;
                    case QueryFilterType.lte:
                        if (isNum)
                            resultIndex = new SortedSet<int>(resultIndex.Where(x => (isIdField ? x : intField.Data[x]) <= num));
                        else
                        {
                            if (isIdField)
                                resultIndex.RemoveWhere(x => x > query.resultIndex.Min);
                            else if (isIntField)
                                resultIndex.RemoveWhere(x => intField.Data[x] > query.resultInt.Min);
                        }
                        break;
                    case QueryFilterType.contain:
                        break;
                    case QueryFilterType.regex:
                        break;
                }
            }
            //第二阶段，排序
            //第三阶段，skip和limit
            //第四阶段，计算
            //第五阶段，综合
            if(this.fields.Count == 1)
            {
                if(this.fields[0] is IntField)
                {
                    var intField = (this.fields[0] as IntField);
                    resultInt = new SortedSet<int>();
                    foreach (var index in resultIndex)
                        resultInt.Add(intField.Data[index]);
                }else if(this.fields[0] is CharField)
                {
                    var charField = (this.fields[0] as CharField);
                    resultInt = new SortedSet<int>();
                    foreach (var index in resultIndex)
                        resultString.Add(charField[index]);
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
            this.value = value;
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
}
