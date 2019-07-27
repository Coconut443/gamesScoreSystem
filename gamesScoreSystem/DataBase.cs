using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    class DataBase
    {
        string name;
        string title;
        Entity[] entities;
        Field[] fields;

        public void Check()
        {
            foreach (var entity in entities)
                entity.Check();
            foreach (var field in fields)
            {
                foreach (var constraint in field.Constraints)
                {
                    if (constraint is VirtualConstraint)
                        break;
                    constraint.Check(field);
                }
            }
        }

        public void Calc()
        {
            foreach (var entity in entities)
                entity.Calc();
            foreach (var field in fields)
            {
                bool isVirtual = false;
                foreach (var constraint in field.Constraints)
                {
                    if (constraint is VirtualConstraint)
                        isVirtual = true;
                    else if (!isVirtual)
                        break;
                    constraint.Check(field);
                }
            }
        }

        public string Name { get => name; set => name = value; }
        public string Title { get => title; set => title = value; }
        internal Entity[] Entities { get => entities; set => entities = value; }
        internal Field[] Fields { get => fields; set => fields = value; }
    }
}
