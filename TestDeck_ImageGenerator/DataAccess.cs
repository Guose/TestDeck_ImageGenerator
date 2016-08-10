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

        static DataAccess()
        {

        }

        public DataAccess()
        {
            this.VotePositionTable = new DataTable();
            this.OvalDataTable = new DataTable();
        }

        public static DataAccess Instance { get { return instance; } }
    }
}
