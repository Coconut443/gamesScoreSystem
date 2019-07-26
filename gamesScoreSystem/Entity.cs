using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    class Entity
    {
        string name;
        int length;
        public Entity(string name,int length)
        {
            this.name = name;
            this.length = length;
        }
        Field[] fields;
        public void Check()
        {
            foreach(var field in fields)
            {
                foreach(var constraint in field.Constraints)
                {
                    if(!(constraint is VirtualConstraint))
                    {
                        constraint.Check(field);
                    }
                }
            }
        }
        public void Calc()
        {
            foreach (var field in fields)
            {
                foreach (var constraint in field.Constraints)
                {
                    if (constraint is VirtualConstraint)
                    {
                        constraint.Check(field);
                    }
                }
            }
        }

        public string Name { get => name; set => name = value; }
        internal Field[] Fields { get => fields; set => fields = value; }
        public int Length { get => length; }
    }
}
