using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TestDeck_ImageGenerator
{
    public class GMCTextFile
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

        public void AddRowsToTempDT(DataTable dt, DataTable ovaldt, int count)
        {
            //Temporary assignment of Rotation property
            if (IsLA == false)
            {
                Rotation = 1;
            }
            OvalData = new DataTable();

            OvalData = ovaldt;
            int iteration = DataAccess.Instance.VotePositionTable.Rows.Count;
            int sortKey = 1;

            string ovalFileName = dt.Rows[0]["BallotImage"].ToString();

            foreach (DataColumn oval in OvalData.Columns)
            {
                if (oval.ColumnName.Contains("BallotImageFront"))
                {
                    string columnToSearch = oval.ColumnName.ToString();
                    int rowIndex = 0;

                    foreach (DataRow row in OvalData.Rows)
                    {
                        if (row[columnToSearch].ToString() == ovalFileName)
                        {
                            rowIndex = OvalData.Rows.IndexOf(row);
                            SetRowsToDataTable(count, sortKey, rowIndex);

                            // arguement should be rotation which is selected on UI
                            LogicAndAccuracyRotation(Rotation);                            
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void SetRowsToDataTable(int count, int sortKey, int rowIndex)
        {
            string[] ballotImageVariations = new string[] { "NO BACK", "NO BACK.pdf", "NOBACK", "NOBACK.pdf", "NOBACK.PDF", "NO BACK.PDF" };
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

                foreach (string item in ballotImageVariations)
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

        private void LogicAndAccuracyRotation(int x) // x equals rotation LA5 or LA2
        {            
            int loops = 1;
            int sequenceNum = 1 + DataAccess.Instance.VotePositionTable.Rows.Count;
            int row = DataAccess.Instance.VotePositionTable.Rows.Count;
            

            foreach (DataRow item in TempDt.Rows)
            {
                if (loops == 1 || x < 2)
                {
                    DataAccess.Instance.VotePositionTable.Rows.Add(item.ItemArray);
                    DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                    loops++; sequenceNum++; row++;
                }
                else
                {
                    if (loops <= x)
                    {
                        for (int i = 0; i < loops; i++)
                        {
                            DataAccess.Instance.VotePositionTable.Rows.Add(item.ItemArray);
                            DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                            sequenceNum++;
                            row++;
                        }

                        loops++; 

                        if (loops > x)
                            loops = 1;
                    }                  
                }                
            }
        }

        private string AddIlkToData()
        {
            // this needs to be flagged by radiobutton from WPF
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

        public string SaveGMCTextfile(string path, DataTable dt, string countyId)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            string fileName = path + @"\" + countyId + "-" + AddIlkToData() + "_TESTDECK_GMC.txt"; //dlg.FileName;
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

            IsLA = false;
            IsQC = false;
            IsWHSE = false;
            IsMULTI = false;

            return fileName;
        }

    }
}
