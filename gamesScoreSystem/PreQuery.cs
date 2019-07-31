//预查询类以及字段中数据的比较符
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace gamesScoreSystem
{
    /// <summary>
    /// 预查询类
    /// </summary>
    class PreQuery
    {
        /// <summary>
        /// 排序方向
        /// </summary>
        public enum SortOrder
        {
            /// <summary>
            /// 升序
            /// </summary>
            Asc,
            /// <summary>
            /// 降序
            /// </summary>
            Desc
        }

        /// <summary>
        /// 计算类型
        /// </summary>
        enum CalcType
        {
            /// <summary>
            /// 无
            /// </summary>
            None,
            /// <summary>
            /// 总和
            /// </summary>
            Sum,
            /// <summary>
            /// 计数
            /// </summary>
            Count,
            /// <summary>
            /// 求排名
            /// </summary>
            Rank,
            /// <summary>
            /// 求计数的排名
            /// </summary>
            CountRank,
            /// <summary>
            /// 求总和的排名
            /// </summary>
            SumRank
        }

        /// <summary>
        /// 输出流的类型
        /// </summary>
        enum StreamType
        {
            /// <summary>
            /// 无（控制台输出）
            /// </summary>
            None,
            /// <summary>
            /// 按照csv格式输出到文件
            /// </summary>
            Csv
        }

        /// <summary>
        /// 筛选条件类型
        /// </summary>
        enum QueryFilterType
        {
            /// <summary>
            /// 按照id筛选
            /// </summary>
            id,
            /// <summary>
            /// 按照相等条件筛选
            /// </summary>
            eq,
            /// <summary>
            /// 按照大于给出值的条件筛选
            /// </summary>
            gt,
            /// <summary>
            /// 按照小于给出值的条件筛选
            /// </summary>
            lt,
            /// <summary>
            /// 按照不等于给出值的条件筛选
            /// </summary>
            ne,
            /// <summary>
            /// 按照大于等于给出值的条件筛选
            /// </summary>
            gte,
            /// <summary>
            /// 按照小于等于给出值的条件筛选
            /// </summary>
            lte,
            /// <summary>
            /// 按照包含条件筛选
            /// </summary>
            contain,
            /// <summary>
            /// 按照匹配正则表达式的条件筛选
            /// </summary>
            regex
        }

        /// <summary>
        /// 预查询属于的会话
        /// </summary>
        private Session session;

        /// <summary>
        /// 预查询的主实体名称
        /// </summary>
        private string subject;

        /// <summary>
        /// 预查询的投影字段
        /// </summary>
        private List<Field> fields = new List<Field>();

        /// <summary>
        /// 预查询的筛选条件
        /// </summary>
        private List<Tuple<QueryFilterType, Field, Param>> queryFilters = new List<Tuple<QueryFilterType, Field, Param>>();

        /// <summary>
        /// 预查询的排序因子
        /// </summary>
        private List<Tuple<SortOrder, Field>> SortFactors = new List<Tuple<SortOrder, Field>>();

        /// <summary>
        /// 预查询的限制行数，负数表示没有限制
        /// </summary>
        private int limitNum = -1;

        /// <summary>
        /// 预查询的跳过行数
        /// </summary>
        private int skipNum = 0;

        /// <summary>
        /// 预查询的计算类型
        /// </summary>
        private CalcType calcType = CalcType.None;

        /// <summary>
        /// 预查询的输出流类型
        /// </summary>
        private StreamType streamType = StreamType.None;

        /// <summary>
        /// 预查询的主语实体
        /// </summary>
        private Entity entity;

        /// <summary>
        /// 用索引表示的查询结果集合
        /// </summary>
        private SortedSet<int> resultIndex;

        /// <summary>
        /// 用数值表示的查询结果列表
        /// </summary>
        private List<int> resultInt;

        /// <summary>
        /// 用字符串表示的查询结果列表
        /// </summary>
        private List<string> resultString;

        /// <summary>
        /// 用数值表示的单个查询结果
        /// </summary>
        private int resultNum = 0;

        /// <summary>
        /// 标识此条预查询是否应当执行与输出（如为全局函数，则不需要执行）
        /// </summary>
        private bool shouldExec = true;

        /// <summary>
        /// 如果有，表示输出流的路径
        /// </summary>
        private string path;

        /// <summary>
        /// 用数值表示的查询结果列表
        /// </summary>
        public List<int> ResultInt { get => resultInt; set => resultInt = value; }

        /// <summary>
        /// 用字符串表示的查询结果列表
        /// </summary>
        public List<string> ResultString { get => resultString; set => resultString = value; }

        /// <summary>
        /// 用数值表示的单个查询结果
        /// </summary>
        public int ResultNum { get => resultNum; set => resultNum = value; }

        /// <summary>
        /// 预查询的构造函数
        /// </summary>
        /// <param name="session">预查询属于的会话</param>
        public PreQuery(Session session)
        {
            this.session = session;
        }

        /// <summary>
        /// 使用主语来初始化预查询
        /// </summary>
        /// <param name="function">主语（一个全局函数或者主实体+投影字段）</param>
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
                if (database == null)
                    throw new Exception("未指定数据库");
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

        /// <summary>
        /// 按照标识符获得相应的字段
        /// </summary>
        /// <param name="param">标识符参数</param>
        /// <returns>字段</returns>
        private Field GetField(IdParam param)
        {
            if (param.Value == "id")
                return null;
            var field = Array.Find(entity.Fields, x => x.Name == param.Value);
            if (field == null)
                throw new Exception("实体" + entity.Name + "中不含字段" + param.Value);
            return field;
        }

        /// <summary>
        /// 使用修饰函数来修改预查询的状态
        /// </summary>
        /// <param name="function">修饰函数，包括筛选，排序，计算，输出流等</param>
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
                    path = (function.Params[0] as StringParam).Value;
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
                    if (calcType == CalcType.Count)
                        calcType = CalcType.CountRank;
                    else if (calcType == CalcType.Sum)
                        calcType = CalcType.SumRank;
                    else calcType = CalcType.Rank;
                    break;
                default:
                    throw new Exception("不支持的函数");
            }
        }

        /// <summary>
        /// 计算查询
        /// </summary>
        public void Exec()
        {
            if (!shouldExec)
                return;
            //第一阶段，进行多轮筛选
            resultIndex = new SortedSet<int>();
            for (int i = 1; i <= entity.Length; ++i)
                resultIndex.Add(i);
            foreach (var filter in queryFilters)
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
                else if (query != null && isIdField && (query.entity != entity && !(query.fields.Count == 1 && Array.Exists(query.fields[0].Constraints, x => (x is ForeignConstraint) && ((x as ForeignConstraint).RefEntity == entity)))))
                    throw new Exception("不同实体的id不能进行比较");

                //无论如何都会执行
                if (query != null)
                    query.Exec();
                switch (queryType)
                {
                    case QueryFilterType.id:
                        if (isNum)
                        {
                            var ex = resultIndex.Contains(num);
                            resultIndex.Clear();
                            if (ex)
                                resultIndex.Add(num);
                        }
                        else
                        {
                            if (query.fields.Count != 1)
                                resultIndex.RemoveWhere(x => !query.resultIndex.Contains(x));
                            else
                                resultIndex.RemoveWhere(x => !query.resultInt.Contains(x));

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
            resultIndex = new SortedSet<int>(resultIndex, comparer);
            //第三阶段，skip和limit
            var newSet = new SortedSet<int>(comparer);
            int cnt = 0;
            foreach (var index in resultIndex.Skip(skipNum))
            {
                newSet.Add(index);
                if (limitNum > 0 && ++cnt >= limitNum)
                    break;
            }
            resultIndex = newSet;
            //第四阶段，综合
            if (this.fields.Count == 1)
            {
                if (this.fields[0] == null)
                {
                    resultInt = new List<int>(resultIndex);
                }
                else if (this.fields[0] is IntField)
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

                }
                else if (this.fields[0] is CharField)
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
                    if (resultInt != null)
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
                    else
                    {
                        var tempIndex = new List<int>();
                        for (int i = 1; i <= entity.Length; ++i)
                            tempIndex.Add(i);
                        resultNum = tempIndex.Count(x => comparer.ValueCompare(x, resultIndex.First()) < 0) + 1;
                    }
                    break;
                case CalcType.CountRank:

                    var currentNum = resultIndex.Count;

                    calcType = CalcType.Count;

                    var rankcnt = 1;

                    var filterIndex = -1;
                    ForeignConstraint foreignConstraint = null;

                    for (int j = 0; j < queryFilters.Count; ++j)
                    {
                        var filter = queryFilters[j];
                        foreignConstraint = Array.Find(filter.Item2.Constraints, x => x is ForeignConstraint) as ForeignConstraint;
                        if (foreignConstraint != null)
                        {
                            filterIndex = j;
                            break;
                        }
                    }
                    if (filterIndex == -1 || foreignConstraint == null)
                    {
                        resultNum = 1;
                        return;
                    }
                    for (int i = 1; i <= foreignConstraint.RefEntity.Length; ++i)
                    {
                        queryFilters[filterIndex] = new Tuple<QueryFilterType, Field, Param>(queryFilters[filterIndex].Item1, queryFilters[filterIndex].Item2, new NumParam(i));
                        resultNum = 0;
                        queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.id, null, new NumParam(i)));
                        this.Exec();
                        if (resultNum > currentNum)
                            ++rankcnt;
                    }

                    calcType = CalcType.CountRank;

                    resultNum = rankcnt;
                    break;
                case CalcType.SumRank:
                    if (resultInt != null)
                    {
                        var intField = this.fields[0] as IntField;
                        foreach (var index in resultIndex)
                            resultNum += intField.Data[index - 1];
                    }
                    currentNum = resultNum;

                    calcType = CalcType.Sum;

                    rankcnt = 1;
                    filterIndex = -1;
                    foreignConstraint = null;

                    for (int j = 0; j < queryFilters.Count; ++j)
                    {
                        var filter = queryFilters[j];
                        foreignConstraint = Array.Find(filter.Item2.Constraints, x => x is ForeignConstraint) as ForeignConstraint;
                        if (foreignConstraint != null)
                        {
                            filterIndex = j;
                            break;
                        }
                    }
                    if (filterIndex == -1 || foreignConstraint == null)
                    {
                        resultNum = 1;
                        return;
                    }
                    for (int i = 1; i <= foreignConstraint.RefEntity.Length; ++i)
                    {
                        queryFilters[filterIndex] = new Tuple<QueryFilterType, Field, Param>(queryFilters[filterIndex].Item1, queryFilters[filterIndex].Item2, new NumParam(i));
                        resultNum = 0;
                        queryFilters.Add(new Tuple<QueryFilterType, Field, Param>(QueryFilterType.id, null, new NumParam(i)));
                        this.Exec();
                        if (resultNum > currentNum)
                            ++rankcnt;
                    }

                    calcType = CalcType.SumRank;

                    resultNum = rankcnt;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 按照输出流的状态对查询结果进行输出
        /// </summary>
        public void Output()
        {
            if (!shouldExec)
                return;
            if (streamType == StreamType.None)
            {
                if (calcType != CalcType.None)
                {
                    Session.OutputData("result", ResultNum.ToString());
                }
                else if (fields.Count == 1 && fields[0] is IntField && resultInt != null)
                {
                    var constraint = fields[0].Constraints.FirstOrDefault(x => x is ForeignConstraint);
                    if (constraint != null)
                    {
                        var foreignConstraint = constraint as ForeignConstraint;
                        fields = new List<Field>(foreignConstraint.RefEntity.Fields);
                        var nameList = new string[] { "id" }.Concat(from field in fields where field != null select field.Name);
                        var data = from index in resultInt
                                   select new string[] { index.ToString() }.Concat(
                                     from field in fields
                                     where field != null
                                     select field[index]
                                   );
                        Session.OutputData(nameList, data);
                    }
                    else Session.OutputData("results", resultInt.Cast<string>());
                }
                else if (fields.Count == 1 && fields[0] is CharField && resultString != null)
                {
                    Session.OutputData("results", resultString);
                }
                else
                {
                    if (fields.Count == 0 || (fields.Count == 1 && fields[0] == null))
                        fields.AddRange(entity.Fields);
                    var nameList = new string[] { "id" }.Concat(from field in fields where field != null select field.Name);
                    var data = from index in resultIndex
                               select new string[] { index.ToString() }.Concat(
                                 from field in fields
                                 where field != null
                                 select field[index]
                               );
                    Session.OutputData(nameList, data);
                }
            }
            else if (streamType == StreamType.Csv)
            {
                StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create));
                if (calcType != CalcType.None)
                    writer.WriteLine("result\n" + ResultNum.ToString());
                else if (fields.Count == 1 && fields[0] is IntField && resultInt != null)
                {
                    var constraint = fields[0].Constraints.FirstOrDefault(x => x is ForeignConstraint);
                    if (constraint != null)
                    {
                        var foreignConstraint = constraint as ForeignConstraint;
                        fields = new List<Field>(foreignConstraint.RefEntity.Fields);
                        writer.WriteLine("id," + String.Join(",", from field in fields where field != null select field.Name));
                        writer.WriteLine(String.Join("\n", from index in resultInt
                                                           select String.Join(",", new string[] { index.ToString() }.Concat(
                                                             from field in fields
                                                             where field != null
                                                             select field[index]
                                                           ))));
                    }
                    else writer.WriteLine("results\n" + String.Join("\n", resultInt.Cast<string>()));
                }
                else if (fields.Count == 1 && fields[0] is CharField && resultString != null)
                {
                    writer.WriteLine("results\n" + String.Join("\n", resultString));
                }
                else
                {
                    if (fields.Count == 0 || (fields.Count == 1 && fields[0] == null))
                        fields.AddRange(entity.Fields);
                    writer.WriteLine(String.Join(",", new string[] { "id" }.Concat(from field in fields where field != null select field.Name)));
                    writer.WriteLine(String.Join("\n", from index in resultIndex
                                                       select String.Join(",", new string[] { index.ToString() }.Concat(
                                                         from field in fields
                                                         where field != null
                                                         select field[index]
                                                       ))));
                }
                writer.Close();
            }
        }
    }


    /// <summary>
    /// Id的排序类
    /// </summary>
    class IndexComparer : IComparer<int>
    {
        /// <summary>
        /// 排序因子，包含排序方向和排序字段信息
        /// </summary>
        private List<Tuple<PreQuery.SortOrder, Field>> sortFactors;

        /// <summary>
        /// Id排序类的构造函数
        /// </summary>
        /// <param name="sortFactors">排序因子</param>
        public IndexComparer(List<Tuple<PreQuery.SortOrder, Field>> sortFactors)
        {
            this.sortFactors = sortFactors;
            sortFactors.Reverse();
        }

        /// <summary>
        /// Id的强制比较函数
        /// </summary>
        /// <param name="lhs">Id1</param>
        /// <param name="rhs">Id2</param>
        /// <returns>比较结果，只有Id相等时才为0</returns>
        public int Compare(int lhs, int rhs)
        {
            int result = 0;
            foreach (var factor in sortFactors)
            {
                var sortOrder = factor.Item1;
                var field = factor.Item2;
                if (field == null)
                    result = sortOrder == PreQuery.SortOrder.Asc ? lhs - rhs : rhs - lhs;
                else if (field is IntField)
                {
                    var intField = field as IntField;
                    result = sortOrder == PreQuery.SortOrder.Asc ? intField.Data[lhs - 1] - intField.Data[rhs - 1] : intField.Data[rhs - 1] - intField.Data[lhs - 1];
                }
                else if (field is CharField)
                {
                    var charField = field as CharField;
                    result = sortOrder == PreQuery.SortOrder.Asc ? String.Compare(charField[lhs], charField[rhs]) : String.Compare(charField[rhs], charField[lhs]);
                }
                else result = 0;
                if (result != 0)
                    return result;
            }
            return lhs - rhs;
        }

        /// <summary>
        /// Id的非强制比较函数
        /// </summary>
        /// <param name="lhs">Id1</param>
        /// <param name="rhs">Id2</param>
        /// <returns>比较结果，值相等时也为0</returns>
        public int ValueCompare(int lhs, int rhs)
        {
            int result = 0;
            foreach (var factor in sortFactors)
            {
                var sortOrder = factor.Item1;
                var field = factor.Item2;
                if (field == null)
                    result = sortOrder == PreQuery.SortOrder.Asc ? lhs - rhs : rhs - lhs;
                else if (field is IntField)
                {
                    var intField = field as IntField;
                    result = sortOrder == PreQuery.SortOrder.Asc ? intField.Data[lhs - 1] - intField.Data[rhs - 1] : intField.Data[rhs - 1] - intField.Data[lhs - 1];
                }
                else if (field is CharField)
                {
                    var charField = field as CharField;
                    result = sortOrder == PreQuery.SortOrder.Asc ? String.Compare(charField[lhs], charField[rhs]) : String.Compare(charField[rhs], charField[lhs]);
                }
                else result = 0;
                if (result != 0)
                    return result;
            }
            return result;
        }
    }
}
