using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TestDeck_ImageGenerator
{
    public sealed class DataAccess
    {
        static readonly DataAccess instance = new DataAccess();

        public DataTable VotePositionTable { get; set; }
        public DataTable OvalDataTable { get; set; }
        public DataTable DeckinatorTable { get; set; }

        static DataAccess()
        {

        }

        public DataAccess()
        {
            VotePositionTable = new DataTable();
            OvalDataTable = new DataTable();
            DeckinatorTable = new DataTable();
        }

        public static DataAccess Instance { get { return instance; } }
    }
}
