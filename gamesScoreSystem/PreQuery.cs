using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Session session;
        private string subject;
        private List<Field> fields = new List<Field>();
        private List<QueryFilter> queryFilters = new List<QueryFilter>();
        private List<Tuple<SortOrder, Field>> SortFactors = new List<Tuple<SortOrder, Field>>();
        private int limitNum = -1;
        private int skipNum = 0;
        private CalcType calcType = CalcType.None;
        private StreamType streamType = StreamType.None;

        public PreQuery(Session session)
        {
            this.session = session;
        }

        //TODO:填写Init和AddFunction的处理过程
        public void Init(Function function)
        {
            if(function.Params == null)
            {

            }
            else
            {
                if (session.ExecFunction(function))
                    return;
            }
            
        }
        public void AddFunction(Function function)
        {
            
        }
    }

    class Function
    {
        public string functionName;
        public List<Param> Params;
    }

    class Param
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

    class IntParam : Param
    {
        int value;

        public IntParam(int value)
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

    class ParamFactory
    {
        public static Param Create(int value)
        {
            return new IntParam(value);
        }
        
        public static Param Create(string value, bool isId)
        {
            if (isId)
                return new IdParam(value);
            else
                return new StringParam(value);
        }
        
        public static Param Create(PreQuery value)
        {
            return new QueryParam(value);
        }
    }
}
