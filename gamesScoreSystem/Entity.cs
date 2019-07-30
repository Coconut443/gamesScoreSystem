using System;
using System.IO;
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
                    if (constraint is VirtualConstraint)
                        break;
                    constraint.Check(field);
                }
            }
        }
        public void Calc()
        {
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

        public void Save(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(length);
            writer.Write(fields.Length);
            foreach (var field in fields)
            {
                writer.Write(field is IntField ? true : false);
                field.Save(writer);
            }
                
        }
        public void Load(BinaryReader reader)
        {
            name = reader.ReadString();
            length = reader.ReadInt32();
            var fieldsLength = reader.ReadInt32();
            fields = new Field[fieldsLength];
            for(int i = 0; i < fieldsLength; ++i)
            {
                bool isIntField = reader.ReadBoolean();
                if (isIntField)
                    fields[i] = new IntField("", length, new Constraint[0]);
                else
                    fields[i] = new CharField("", length, 1, new Constraint[0]);
                fields[i].Load(reader);
            }
        }

        public string Name { get => name; set => name = value; }
        internal Field[] Fields { get => fields; set => fields = value; }
        public int Length { get => length; }
    }
}
