using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        public void Save(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(entities.Length);
            foreach (var entity in entities)
                entity.Save(writer);
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
            entities = new Entity[reader.ReadInt32()];
            for (int i = 0; i < entities.Length; ++i)
            {
                entities[i] = new Entity("", 0);
                entities[i].Load(reader);
            }
                
            fields = new Field[reader.ReadInt32()];
            for(int i = 0; i < fields.Length; ++i)
            {
                bool isIntField = reader.ReadBoolean();
                if (isIntField)
                    fields[i] = new IntField("", 1, new Constraint[0]);
                else
                    fields[i] = new CharField("", 1, 1, new Constraint[0]);
                fields[i].Load(reader);
            }
            foreach(var entity in entities)
            {
                foreach(var field in entity.Fields)
                {
                    if (field.RefEntityName != "")
                    {
                        field.Constraints = new Constraint[1];
                        field.Constraints[0] = new ForeignConstraint(new string[1] { field.RefEntityName });
                        var foreignConstraint = field.Constraints[0] as ForeignConstraint;
                        foreignConstraint.RefDataBase = this;
                        foreignConstraint.RefEntity = entities.First(x => x.Name == field.RefEntityName);
                    }
                }
            }
        }

        public string Name { get => name; set => name = value; }
        public string Title { get => title; set => title = value; }
        internal Entity[] Entities { get => entities; set => entities = value; }
        internal Field[] Fields { get => fields; set => fields = value; }
    }
}
