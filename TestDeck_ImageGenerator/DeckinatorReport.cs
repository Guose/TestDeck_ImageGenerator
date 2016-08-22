using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TestDeck_ImageGenerator
{
    public class DeckinatorReport : IDisposable
    {
        public DeckinatorReport(bool writeIns, int ballots) : this()
        {
            IncludeWriteIn = writeIns;
            TotalBallotColumn = ballots;
        }
        public DeckinatorReport()
        {
            SetDeckReportDataTable();
        }

        private DataTable _deckReport = new DataTable();
        private bool _includeWriteIn;
        private int _totalBallots;

        public DataTable DeckReport
        {
            get { return _deckReport; }
            set { _deckReport = value; }
        }
        public bool IncludeWriteIn
        {
            get { return _includeWriteIn; }
            set { _includeWriteIn = value; }
        }
        public int TotalBallotColumn
        {
            get { return _totalBallots; }
            set { _totalBallots = value; }
        }

        public DataTable QueryDeckReportDTByImageName(string imageName)
        {
            DataTable byImageNameDt = new DataTable();

            IEnumerable<DataRow> queryImageName = from d in DeckReport.AsEnumerable()
                                                  where d.Field<string>("BallotImageFront") == imageName
                                                  select d;

            byImageNameDt = queryImageName.CopyToDataTable();

            return byImageNameDt;
        }

        public void SetDataAccessDeckinatorDT(int rotation, DataTable dtImages)
        {
            int totalVotes = 0;            
            int sequenceNum = 1 + DataAccess.Instance.DeckinatorTable.Rows.Count;
            int row = DataAccess.Instance.DeckinatorTable.Rows.Count;
            int index = 0;
            int raceId = 0;
            int raceCount = 0;
            
            // Sort Images by the total number of votes Ascending
            DataView dv = dtImages.DefaultView;
            dv.Sort = "TotalVotes";
            dtImages = dv.ToTable();

            DataTable dtGreaterThanOne = new DataTable();
            DataTable dtEqualToOne = new DataTable();
            DataTable dtRaceId = new DataTable();
            DataTable temp = new DataTable();

            dtEqualToOne = QueryTotalVotesEqualToOne(dtImages);            

            while(index != dtImages.Rows.Count)
            {
                int loops = 1;
                totalVotes = Convert.ToInt32(dtImages.Rows[index]["TotalVotes"]);

                if (totalVotes == 1)
                {
                    foreach (DataRow dr in dtEqualToOne.Rows)
                    {
                        DataAccess.Instance.DeckinatorTable.Rows.Add(dr.ItemArray);
                        DataAccess.Instance.DeckinatorTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                        sequenceNum++;
                        row++;
                        index++;
                    }
                }
                else
                {                    
                    temp = QueryTotalVotesGreaterThanOne(dtImages);

                    IEnumerable<DataRow> queryRaceId = from race in temp.AsEnumerable()
                                                       group race by race.Field<int>("RaceID") into grp
                                                       select grp.First();
                    dtRaceId = queryRaceId.CopyToDataTable();

                    raceId = Convert.ToInt32(dtRaceId.Rows[raceCount]["RaceID"]);

                    IEnumerable<DataRow> queryUniqueRaceId = from rid in temp.AsEnumerable()
                                                             where rid.Field<int>("RaceID") == raceId
                                                             select rid;
                    dtGreaterThanOne = queryUniqueRaceId.CopyToDataTable();

                    foreach (DataRow item in dtGreaterThanOne.Rows)
                    {
                        if (loops == 1 || rotation < 2)
                        {
                            DataAccess.Instance.DeckinatorTable.Rows.Add(item.ItemArray);
                            DataAccess.Instance.DeckinatorTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                            loops++;
                            sequenceNum++;
                            row++;
                            index++;
                        }
                        else
                        {
                            if (loops <= rotation)
                            {
                                for (int j = 0; j < loops; j++)
                                {
                                    DataAccess.Instance.DeckinatorTable.Rows.Add(item.ItemArray);
                                    DataAccess.Instance.DeckinatorTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                                    sequenceNum++;
                                    row++;
                                }
                                loops++;
                                index++;

                                if (loops > rotation)
                                    loops = 1;
                            }
                        }
                    }
                    raceCount++;
                }                
            }
        }


        private DataTable QueryTotalVotesEqualToOne(DataTable dt)
        {
            DataTable queryDt = new DataTable();
            IEnumerable<DataRow> queryVotes = from v in dt.AsEnumerable()
                                              where v.Field<int>("TotalVotes") == 1
                                              select v;

            if (queryVotes.Any())
            {
                queryDt = queryVotes.CopyToDataTable();
            }

            return queryDt;
        }

        private DataTable QueryTotalVotesGreaterThanOne(DataTable dt)
        {
            DataTable queryDt = new DataTable();
            IEnumerable<DataRow> queryVotes = from v in dt.AsEnumerable()
                                              where v.Field<int>("TotalVotes") > 1
                                              select v;

            if (queryVotes.Any())
            {
                queryDt = queryVotes.CopyToDataTable();
            }          

            return queryDt;
        }

        private void PopulateDeckReportDT()
        {
            for (int i = 0; i < DataAccess.Instance.OvalDataTable.Rows.Count; i++)
            {
                string precinctId = DataAccess.Instance.OvalDataTable.Rows[i]["PrecinctID"].ToString();

                DeckReport.Rows.Add();
                DeckReport.Rows[i]["CardStyle"] = DataAccess.Instance.OvalDataTable.Rows[i]["CardStyle"];
                DeckReport.Rows[i]["PrecinctID"] = precinctId.PadLeft(8, '0');
                DeckReport.Rows[i]["Candidate"] = DataAccess.Instance.OvalDataTable.Rows[i]["Candidate"];
                DeckReport.Rows[i]["BallotImageFront"] = DataAccess.Instance.OvalDataTable.Rows[i]["BallotImageFront"];
                DeckReport.Rows[i]["BallotImageBack"] = DataAccess.Instance.OvalDataTable.Rows[i]["BallotImageBack"];
                DeckReport.Rows[i]["Race"] = DataAccess.Instance.OvalDataTable.Rows[i]["Race"];
                DeckReport.Rows[i]["Party"] = DataAccess.Instance.OvalDataTable.Rows[i]["Party"];
                DeckReport.Rows[i]["RaceID"] = DataAccess.Instance.OvalDataTable.Rows[i]["RaceID"];
                DeckReport.Rows[i]["ReportSequence"] = DataAccess.Instance.OvalDataTable.Rows[i]["ReportSequence"];
                DeckReport.Rows[i]["OvalPosn"] = DataAccess.Instance.OvalDataTable.Rows[i]["OvalPosition"];
                DeckReport.Rows[i]["TotalVotes"] = DataAccess.Instance.OvalDataTable.Rows[i]["TotalVotes"];
                DeckReport.Rows[i]["WriteIn"] = DataAccess.Instance.OvalDataTable.Rows[i]["IsWriteIn"];

            }
        }
        private void SetDeckReportDataTable()
        {
            _deckReport.Columns.Add("CardStyle", typeof(int));
            _deckReport.Columns.Add("PrecinctID");
            _deckReport.Columns.Add("Candidate");
            _deckReport.Columns.Add("BallotImageFront");
            _deckReport.Columns.Add("BallotImageBack");
            _deckReport.Columns.Add("Race");
            _deckReport.Columns.Add("Party");
            _deckReport.Columns.Add("RaceID", typeof(int));
            _deckReport.Columns.Add("ReportSequence", typeof(int));
            _deckReport.Columns.Add("OvalPosn", typeof(int));
            _deckReport.Columns.Add("TotalVotes", typeof(int));
            _deckReport.Columns.Add("WriteIn", typeof(int));
            _deckReport.Columns.Add("Sequence", typeof(int));
            _deckReport.Columns.Add("TotalBallots", typeof(int));

            PopulateDeckReportDT();

            if (DataAccess.Instance.DeckinatorTable.Rows.Count < 1)
            {
                DataAccess.Instance.DeckinatorTable = DeckReport.Clone();
            }            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    DeckReport = null;
                    _deckReport = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeckinatorReport() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
