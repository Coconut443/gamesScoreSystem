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
        public Entity(string name)
        {
            this.name = name;
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
    }
}
