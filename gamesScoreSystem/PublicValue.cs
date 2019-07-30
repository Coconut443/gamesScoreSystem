using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamesScoreSystem
{
    class PublicValue
    {
        private static int maxCharLen = 256;
        private static int maxLen = 1000000;
        private static int outputLimit = 20;

        public static int MaxCharLen { get => maxCharLen; set => maxCharLen = value; }
        public static int MaxLen { get => maxLen; set => maxLen = value; }
        public static int OutputLimit { get => outputLimit; set => outputLimit = value; }
    }
}
