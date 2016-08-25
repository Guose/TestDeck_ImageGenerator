using System;
using System.Collections.Generic;
using System.Data;
using XYCoordinates;
using System.Linq;
using System.Windows;

namespace TestDeck_ImageGenerator
{
    public class DataLoader : IDisposable
    {
        public DataLoader(string path, string fileName) : this()
        {
            FilePath = path;
            FileName = fileName;
        }

        public DataLoader()
        {

        }
        public bool IsMultiVote { get; set; }
        public int MaxNumOfCandidate { get; set; }
        public int IsWriteIn { get; set; }
        public DataTable PositionDT { get; set; }
        public DataTable OvalDT { get; set; }
        public string FilePath { get; set; }
        public string OvalFileName { get; set; }
        public string FileName { get; set; }

        public List<string> RetrievePdfFileNames(string path)
        {
            DataTable pdfName = new DataTable();
            List<string> fileList = new List<string>();

            IEnumerable<DataRow> pdfFileList = from pdf in OvalDT.AsEnumerable()
                                                group pdf by pdf.Field<string>("BallotImageFront") into grp
                                                select grp.First();

            pdfName = pdfFileList.CopyToDataTable();

            for (int i = 0; i < pdfName.Rows.Count; i++)
            {
                fileList.Add(path + pdfName.Rows[i]["BallotImageFront"].ToString() + ".pdf");
            }
            return fileList;
        }

        public DataTable GeneratePositionDataTable()
        {
            DataTable dt = new DataTable();
            ArrowCoordinates xyc = new ArrowCoordinates(OvalDT);

            try
            {
                dt = xyc.GetXYCoordinates(FileName);

                //dt.Columns.Add("RaceNbr", typeof(int));
                dt.Columns.Add("RacePosn", typeof(int));
                dt.Columns.Add("TtlRaceOvals", typeof(int));
                dt.Columns.Add("RecNbr", typeof(int));
                dt.Columns.Add("IsWriteIn", typeof(int));
                dt.Columns.Add("MaxVotes", typeof(int));

                dt = UpdatePositionData(dt);
                DataView dv = dt.DefaultView;
                dv.Sort = "RacePosn";
                dt = dv.ToTable();                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN: DataLoader.GeneratePositionDataTable");
                throw;
            }
            return dt;
        }

        private DataTable UpdatePositionData(DataTable dt)
        {
            OvalFileName = dt.Rows[0]["BallotImage"].ToString();

            try
            {

                foreach (DataColumn oval in OvalDT.Columns)
                {
                    if (oval.ColumnName.Contains("BallotImageFront"))
                    {
                        string columnToSearch = oval.ColumnName.ToString();
                        int rowIndex = 0;

                        foreach (DataRow row in OvalDT.Rows)
                        {
                            if (row[columnToSearch].ToString() == OvalFileName)
                            {
                                rowIndex = OvalDT.Rows.IndexOf(row);

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    dt.Rows[i]["RacePosn"] = OvalDT.Rows[rowIndex]["OvalPosition"];
                                    dt.Rows[i]["IsWriteIn"] = OvalDT.Rows[rowIndex]["IsWriteIn"];
                                    dt.Rows[i]["TtlRaceOvals"] = OvalDT.Rows[rowIndex]["TotalVotes"];
                                    dt.Rows[i]["MaxVotes"] = OvalDT.Rows[rowIndex]["MaxVotes"];
                                    dt.Rows[i]["RecNbr"] = i + 1;
                                    rowIndex++;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
                //RetrieveRaceNbr(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN: DataLoader.UpdatePositionData");
                throw;
            }
            return dt;
        }

        private DataTable RetrieveRaceNbr(DataTable dt)
        {
            int totalArrows = 0;
            int racePosn = 0;
            int raceNbr = 1;
            int maxVotes = 0;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    totalArrows = Convert.ToInt32(dt.Rows[i]["TtlRaceOvals"]);
                    racePosn = Convert.ToInt32(dt.Rows[i]["RacePosn"]);
                    maxVotes = Convert.ToInt32(dt.Rows[i]["MaxVotes"]);

                    if (totalArrows > racePosn)
                    {
                        for (int j = i; j < (totalArrows + i) - 1; j++)
                        {
                            dt.Rows[j]["RaceNbr"] = raceNbr;
                        }
                    }
                    else
                    {
                        dt.Rows[i]["RaceNbr"] = raceNbr;
                        raceNbr++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN: DataLoader.RetrieveRaceNbr");
                throw;
            }
            return dt;
        }

        public string RetrieveFileName()
        {            
            string fileNm = string.Empty;

            foreach (DataColumn dc in PositionDT.Columns)
            {
                if (dc.ColumnName == "BallotImage")
                {
                    FileName = PositionDT.Rows[0]["BallotImage"].ToString();
                    fileNm = FileName + ".pdf";
                    break;
                }
            }
            return fileNm;
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
                    PositionDT = null;
                    OvalDT = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DataLoader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
