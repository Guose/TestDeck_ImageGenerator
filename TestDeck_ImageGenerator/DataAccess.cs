using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;

namespace TestDeck_ImageGenerator
{
    public sealed class DataAccess
    {
        static readonly DataAccess instance = new DataAccess();

        public static DataAccess Instance { get { return instance; } }
        public DataTable VotePositionTable { get; set; }
        public DataTable OvalDataTable { get; set; }
        public DataTable DeckinatorTable { get; set; }
        public DataTable MultiVoteTable { get; set; }
        public bool IsMultiVote { get; set; }

        static DataAccess()
        {

        }

        public DataAccess()
        {
            VotePositionTable = new DataTable();
            OvalDataTable = new DataTable();
            DeckinatorTable = new DataTable();
            MultiVoteTable = new DataTable();
        }        
    }
}
