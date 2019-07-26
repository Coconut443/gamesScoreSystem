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

        public string Name { get => name; set => name = value; }
        public string Title { get => title; set => title = value; }
        internal Entity[] Entities { get => entities; set => entities = value; }
    }
}
