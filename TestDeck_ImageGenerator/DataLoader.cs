using System;
using System.Collections.Generic;
using System.Data;
using XYCoordinates;
using System.Linq;
using System.Windows;
using System.IO;

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

            double page = 0;
            for (int j = 0; j < OvalDT.Rows.Count; j++)
            {
                string back = OvalDT.Rows[j]["BallotImageBack"].ToString();
                page = Convert.ToDouble(OvalDT.Rows[j]["Pge"]);

                if (page > 1)
                {
                    OvalDT.Rows[j]["BallotImageFront"] = back;
                }
            }

            IEnumerable<DataRow> pdfFileList = from pdf in OvalDT.AsEnumerable()
                                                group pdf by pdf.Field<string>("BallotImageFront") into grp
                                                select grp.First();

            pdfName = pdfFileList.CopyToDataTable();

            for (int i = 0; i < pdfName.Rows.Count; i++)
            {
                fileList.Add(path + pdfName.Rows[i]["BallotImageFront"].ToString());
            }
            return fileList;
        }

        public DataTable GeneratePositionDataTable()
        {
            DataTable dt = new DataTable();
            string textFile = Path.GetFileNameWithoutExtension(FileName);
            string path = Path.GetDirectoryName(FileName);
            string fullPathTxtFile = path + @"\" + textFile + "_ArrowCoord.txt";


            // TO DO: Query out all other images except those that are the same as front image and page 2

            double page = 0;
            for (int j = 0; j < DataAccess.Instance.OvalDataTable.Rows.Count; j++)
            {
                string back = DataAccess.Instance.OvalDataTable.Rows[j]["BallotImageBack"].ToString();
                page = Convert.ToDouble(DataAccess.Instance.OvalDataTable.Rows[j]["Pge"]);

                if (page > 1)
                {
                    DataAccess.Instance.OvalDataTable.Rows[j]["BallotImageFront"] = back;
                }
            }

            try
            {
                dt = FileDelimeter.DataFromTextFile(fullPathTxtFile, '|');                

                dt.Columns.Add("RaceID", typeof(int));
                dt.Columns.Add("RacePosn", typeof(int));
                dt.Columns.Add("TtlRaceOvals", typeof(int));
                dt.Columns.Add("RecNbr", typeof(int));
                dt.Columns.Add("IsWriteIn", typeof(int));
                dt.Columns.Add("MaxVotes", typeof(int));
                dt.Columns.Add("Page", typeof(double));
                dt.Columns.Add("TtlWriteIns", typeof(int));

                DataTable dt2Sort = dt.Clone();
                dt2Sort.Columns["XCoord"].DataType = Type.GetType("System.Int32");
                dt2Sort.Columns["YCoord"].DataType = Type.GetType("System.Int32");

                foreach (DataRow dr in dt.Rows)
                {
                    dt2Sort.ImportRow(dr);
                }
                dt2Sort.AcceptChanges();

                //Sort Coordinates ascending order
                DataView sort = dt2Sort.DefaultView;
                sort.Sort = "XCoord, YCoord ASC";
                dt2Sort = sort.ToTable();

                dt = UpdatePositionData(dt2Sort);

                DataView dv = dt.DefaultView;
                dv.Sort = "RacePosn, TtlRaceOvals ASC";
                dt = dv.ToTable();                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN: DataLoader.GeneratePositionDataTable");
                //throw;
            }
            return dt;
        }

        private DataTable UpdatePositionData(DataTable dt)
        {
            OvalFileName = dt.Rows[0]["BallotImage"].ToString();
            bool writeInRace = false;
            bool verOneWrtIn = false;

            try
            {
                IEnumerable<DataRow> wrtInDataFile = from data in DataAccess.Instance.OvalDataTable.AsEnumerable()
                                                     where data.Field<string>("RaceWithWriteIn") != "0"
                                                     select data;

                if (!wrtInDataFile.Any())
                    verOneWrtIn = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                foreach (DataColumn oval in DataAccess.Instance.OvalDataTable.Columns)
                {
                    if (oval.ColumnName.Contains("BallotImageFront"))
                    {
                        string columnToSearch = oval.ColumnName.ToString();
                        int rowIndex = 0;

                        foreach (DataRow row in DataAccess.Instance.OvalDataTable.Rows)
                        {
                            if (row[columnToSearch].ToString() == OvalFileName)
                            {
                                rowIndex = DataAccess.Instance.OvalDataTable.Rows.IndexOf(row);

                                if (verOneWrtIn)
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        int totalRaceOvals = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex]["TotalVotes"]);
                                        int racePosn = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex]["OvalPosition"]);

                                        dt.Rows[i]["RaceID"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["RaceID"];
                                        dt.Rows[i]["RacePosn"] = racePosn;
                                        dt.Rows[i]["IsWriteIn"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["IsWriteIn"];
                                        dt.Rows[i]["TtlRaceOvals"] = totalRaceOvals;
                                        dt.Rows[i]["MaxVotes"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["MaxVotes"];
                                        dt.Rows[i]["RecNbr"] = i + 1;
                                        dt.Rows[i]["Page"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["Pge"];
                                        dt.Rows[i]["TtlWriteIns"] = 0;
                                        rowIndex++;
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        int adjustIndex = 0;
                                        int totalRaceOvals = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex]["TotalVotes"]);
                                        int racePosn = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex]["OvalPosition"]);
                                        int raceWithWriteIn = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex]["RaceWithWriteIn"]);
                                        int writeInCand = totalRaceOvals - (racePosn + raceWithWriteIn);

                                        if (writeInRace == false)
                                        {
                                            dt.Rows[i]["RaceID"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["RaceID"];
                                            dt.Rows[i]["RacePosn"] = racePosn;
                                            dt.Rows[i]["IsWriteIn"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["IsWriteIn"];
                                            dt.Rows[i]["TtlRaceOvals"] = totalRaceOvals;
                                            dt.Rows[i]["MaxVotes"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["MaxVotes"];
                                            dt.Rows[i]["RecNbr"] = i + 1;
                                            dt.Rows[i]["Page"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex]["Pge"];
                                            dt.Rows[i]["TtlWriteIns"] = raceWithWriteIn;
                                            rowIndex++;
                                        }
                                        else
                                        {
                                            int loop = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["RaceWithWriteIn"]);
                                            int ttlWriteIns = loop;
                                            do
                                            {
                                                // if candidate is a write in it runs this code.
                                                dt.Rows[i + adjustIndex]["RaceID"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["RaceID"];
                                                dt.Rows[i + adjustIndex]["RacePosn"] = Convert.ToInt32(DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["OvalPosition"]) + (adjustIndex + 1);
                                                dt.Rows[i + adjustIndex]["IsWriteIn"] = 1;
                                                dt.Rows[i + adjustIndex]["TtlRaceOvals"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["TotalVotes"];
                                                dt.Rows[i + adjustIndex]["MaxVotes"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["MaxVotes"];
                                                dt.Rows[i + adjustIndex]["RecNbr"] = i + (adjustIndex + 1);
                                                dt.Rows[i + adjustIndex]["Page"] = DataAccess.Instance.OvalDataTable.Rows[rowIndex - 1]["Pge"];
                                                dt.Rows[i + adjustIndex]["TtlWriteIns"] = ttlWriteIns;
                                                loop--;
                                                adjustIndex++;

                                            } while (loop > 0);

                                            i = (i + adjustIndex) - 1;
                                        }
                                        if (raceWithWriteIn > 0 && writeInCand <= 0)
                                        {
                                            writeInRace = true;
                                        }
                                        else
                                        {
                                            writeInRace = false;
                                        }
                                    }
                                }

                                break;
                            }
                        }
                        break;
                    }
                }

                if (IsMultiVote)
                {
                    IEnumerable<DataRow> queryMultiVote = from m in dt.AsEnumerable()
                                                          where m.Field<int>("MaxVotes") > 1
                                                          select m;
                    if (queryMultiVote.Any())
                    {
                        dt = queryMultiVote.CopyToDataTable();
                    }                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN: DataLoader.UpdatePositionData");
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
