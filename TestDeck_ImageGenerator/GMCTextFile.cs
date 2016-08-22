using System;
using System.Text;
using System.Data;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Windows;

namespace TestDeck_ImageGenerator
{
    public class GMCTextFile : IDisposable
    {
        public GMCTextFile(DataTable dt, string path) : this()
        {
            OvalData = dt;
            Path = path;
        }
        public GMCTextFile()
        {
            SetGMCDatatable();
        }

        public int _count = 0;

        #region Properties
        public bool ReportProperty { get; set; }
        public string PdfFileName { get; set; }
        public DataTable TempDt { get; set; }
        public DataTable OvalData { get; set; }
        public string Path { get; set; }
        public int FileBreak { get; set; }
        public int Rotation { get; set; }
        public bool IsLA { get; set; }
        public bool IsWHSE { get; set; }
        public bool IsQC { get; set; }
        public bool IsMULTI { get; set; }
        #endregion Properties

        public void GenerateOutputTextFile(DataTable dt, DataTable ovaldt, int count)
        {
            DataTable drDt = new DataTable();
            DeckinatorReport dr = new DeckinatorReport();

            // Assignment of Rotation property
            if (IsLA == false)
            {
                Rotation = 1;
            }
            OvalData = new DataTable();

            OvalData = ovaldt;
            int sortKey = 1;
            string columnToSearch = "BallotImageFront";

            string ovalFileName = dt.Rows[0]["BallotImage"].ToString();

            foreach (DataColumn oval in OvalData.Columns)
            {
                if (oval.ColumnName.Contains(columnToSearch))
                {                    
                    int rowIndex = 0;

                    foreach (DataRow row in OvalData.Rows)
                    {
                        if (row[columnToSearch].ToString() == ovalFileName)
                        {
                            rowIndex = OvalData.Rows.IndexOf(row);
                            SetDataTableRowsForOutput(count, sortKey, ref rowIndex);
                            RotationGMCOutputDataTable(Rotation, TempDt);
                            break;
                        }
                    }
                    break;
                }
            }

            drDt = dr.QueryDeckReportDTByImageName(ovalFileName);
            dr.SetDataAccessDeckinatorDT(Rotation, drDt);            
        }

        private void SetDataTableRowsForOutput(int count, int sortKey, ref int rowIndex)
        {
            string[] ballotImageBackVariations = new string[] { "NO BACK", "NO BACK.pdf", "NOBACK", "NOBACK.pdf", "NOBACK.PDF", "NO BACK.PDF" };
            int pageBack = 0;

            for (int i = 0; i < count; i++)
            {
                TempDt.Rows.Add();

                TempDt.Rows[i]["SortKey"] = sortKey;
                TempDt.Rows[i]["Card"] = OvalData.Rows[rowIndex]["CardStyle"];
                TempDt.Rows[i]["BallotStyle"] = OvalData.Rows[rowIndex]["BallotStyle"];
                TempDt.Rows[i]["PrecinctID"] = OvalData.Rows[rowIndex]["PrecinctID"];
                TempDt.Rows[i]["BallotImageFront"] = PdfFileName;
                TempDt.Rows[i]["BallotImageBack"] = OvalData.Rows[rowIndex]["BallotImageBack"];
                TempDt.Rows[i]["WatermarkName"] = OvalData.Rows[rowIndex]["WatermarkName"];
                TempDt.Rows[i]["WatermarkColor"] = OvalData.Rows[rowIndex]["WatermarkColor"];
                TempDt.Rows[i]["HeaderLeft1"] = OvalData.Rows[rowIndex]["HeaderLeft1"];
                TempDt.Rows[i]["HeaderLeft2"] = OvalData.Rows[rowIndex]["HeaderLeft2"];
                TempDt.Rows[i]["HeaderLeft3"] = OvalData.Rows[rowIndex]["HeaderLeft3"];
                TempDt.Rows[i]["HeaderRight1"] = OvalData.Rows[rowIndex]["HeaderRight1"];
                TempDt.Rows[i]["HeaderRight2"] = OvalData.Rows[rowIndex]["HeaderRight2"];
                TempDt.Rows[i]["HeaderRight3"] = OvalData.Rows[rowIndex]["HeaderRight3"];
                TempDt.Rows[i]["Party"] = OvalData.Rows[rowIndex]["Party"];
                TempDt.Rows[i]["ILK"] = AddIlkToData();

                foreach (string item in ballotImageBackVariations)
                {
                    if (TempDt.Rows[i]["BallotImageBack"].ToString() == item)
                    {
                        TempDt.Rows[i]["PageFront"] = sortKey;
                        TempDt.Rows[i]["PageBack"] = 0;
                        break;
                    }
                }
                pageBack = Convert.ToInt32(TempDt.Rows[i]["PageBack"]);

                if (pageBack != 0)
                {
                    TempDt.Rows[i]["BallotImageBack"] = PdfFileName;

                    if (sortKey == 1)
                    {
                        TempDt.Rows[i]["PageFront"] = sortKey;
                        TempDt.Rows[i]["PageBack"] = sortKey + 1;
                    }
                    else
                    {
                        TempDt.Rows[i]["PageFront"] = sortKey + 1;
                        TempDt.Rows[i]["PageBack"] = sortKey;
                    }
                }

                sortKey++;

                if (DataAccess.Instance.VotePositionTable.Rows.Count < 1)
                {
                    DataAccess.Instance.VotePositionTable = TempDt.Clone();
                }
            }            
        }

        private void RotationGMCOutputDataTable(int rotation, DataTable dt) // x equals rotation i.e. LA5 or LA2
        {
            int loops = 1;
            int sequenceNum = 1 + DataAccess.Instance.VotePositionTable.Rows.Count;
            int row = DataAccess.Instance.VotePositionTable.Rows.Count;            

            foreach (DataRow item in dt.Rows)
            {
                if (loops == 1 || rotation < 2)
                {
                    DataAccess.Instance.VotePositionTable.Rows.Add(item.ItemArray);
                    DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                    loops++; sequenceNum++; row++;
                }
                else
                {
                    if (loops <= rotation)
                    {
                        for (int i = 0; i < loops; i++)
                        {
                            DataAccess.Instance.VotePositionTable.Rows.Add(item.ItemArray);
                            DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                            sequenceNum++;
                            row++;
                        }
                        loops++; 

                        if (loops > rotation)
                            loops = 1;
                    }                  
                }                
            }
        }

        private string AddIlkToData()
        {
            // this is flagged by radiobutton from WPF
            string testdeckIlk = string.Empty;

            if (IsLA)
                testdeckIlk = "L&A";
            else if (IsQC)
                testdeckIlk = "QC";
            else if (IsWHSE)
                testdeckIlk = "WHSE";
            else if (IsMULTI)
                testdeckIlk = "MULTI";

            return testdeckIlk;
        }

        private void SetGMCDatatable()
        {
            TempDt = new DataTable();

            if (TempDt.Columns.Count == 0)
            {
                TempDt.Columns.Add("SortKey", typeof(int));
                TempDt.Columns.Add("Sequence", typeof(int));
                TempDt.Columns.Add("Card", typeof(int));
                TempDt.Columns.Add("BallotStyle");
                TempDt.Columns.Add("PrecinctID");
                TempDt.Columns.Add("BallotImageFront");
                TempDt.Columns.Add("PageFront", typeof(int));
                TempDt.Columns.Add("BallotImageBack");
                TempDt.Columns.Add("PageBack", typeof(int));
                TempDt.Columns.Add("WatermarkName");
                TempDt.Columns.Add("WatermarkColor");
                TempDt.Columns.Add("HeaderLeft1");
                TempDt.Columns.Add("HeaderLeft2");
                TempDt.Columns.Add("HeaderLeft3");
                TempDt.Columns.Add("HeaderRight1");
                TempDt.Columns.Add("HeaderRight2");
                TempDt.Columns.Add("HeaderRight3");
                TempDt.Columns.Add("Party");
                TempDt.Columns.Add("ILK");
            }
        }
        public void SaveDeckReportTextFile(string path, string countyID)
        {
            string fileName = path + @"\" + countyID + "-" + AddIlkToData() + "_DECKREPORT.txt";
            var result = new StringBuilder();

            foreach (DataRow dr in DataAccess.Instance.DeckinatorTable.Rows)
            {
                for (int i = 0; i < DataAccess.Instance.DeckinatorTable.Columns.Count; i++)
                {
                    result.Append(dr[i].ToString());
                    result.Append(i == DataAccess.Instance.DeckinatorTable.Columns.Count - 1 ? "" : "|");
                }
                result.AppendLine();
            }
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(result.ToString());
                sw.Close();
            }

            IsLA = false;
            IsQC = false;
            IsWHSE = false;
            IsMULTI = false;

            var lines = File.ReadAllLines(fileName);
            File.WriteAllLines(fileName, lines.Take(lines.Length - 1).ToArray());
            
        }

        public void GenerateGMCTextFile(string path, DataTable dt, string countyId)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents (.txt)|*.txt";
                
                string fileName = path + @"\" + countyId + "-" + AddIlkToData() + "_TESTDECK_GMC.txt";
                decimal tempBox = dt.Rows.Count / FileBreak;
                int boxSplit = Convert.ToInt32(Math.Ceiling(tempBox + 1));
                string[] fileArray = new string[dt.Rows.Count + boxSplit + 1];
                int count =1;
                int i = 0;
                int rowNum = 0;
                string columns = string.Empty;                

                foreach (DataColumn dc in dt.Columns)
                {
                    columns += dc.ColumnName + '|';
                }
                fileArray[i] = columns.Remove(columns.Length - 1, 1);
                i++;

                foreach (DataRow dr in dt.Rows)
                {
                    string row = string.Empty;

                    if (rowNum % FileBreak == 0)
                    {
                        row = $"BOX DIVIDER #{count}|BOX {count} of {boxSplit}||||BoxDividerFront.pdf|1|BoxDividerBack.pdf|2";
                        fileArray[i] = row;
                        i++;
                        count++;
                        row = string.Empty;
                    }

                    foreach (var item in dr.ItemArray)
                    {
                        row += item.ToString() + '|';
                    }

                    fileArray[i] = row.Remove(row.Length - 1, 1);
                    rowNum++;
                    i++;
                    _count++;              
                }
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    foreach (var item in fileArray)
                    {
                        sw.WriteLine(item);
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsLA = false;
                IsQC = false;
                IsWHSE = false;
                IsMULTI = false;
            }
        }

        public void SaveGMCTextfile(string path, DataTable dt, string countyId)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents (.txt)|*.txt";

                string fileName = path + @"\" + countyId + "-" + AddIlkToData() + "_TESTDECK_GMC.txt";
                //string newFilename = AddSuffix(path, ".txt", true);

                var header = new StringBuilder();
                var result = new StringBuilder();

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    header.Append(dt.Columns[j].ColumnName);
                    header.Append(j == dt.Columns.Count - 1 ? "" : "|");
                }

                foreach (DataRow dr in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        result.Append(dr[i].ToString());
                        result.Append(i == dt.Columns.Count - 1 ? "" : "|");
                    }
                    _count++;
                    result.AppendLine();
                }
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.WriteLine(header.ToString());
                    sw.WriteLine(result.ToString());
                    sw.Close();
                }

                var lines = File.ReadAllLines(fileName);
                File.WriteAllLines(fileName, lines.Take(lines.Length - 1).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR in Saving GMC TextFiles");
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
                    TempDt = null;
                    OvalData = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GMCTextFile() {
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
