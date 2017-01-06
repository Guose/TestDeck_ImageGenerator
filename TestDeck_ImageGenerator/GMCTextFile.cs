using System;
using System.Text;
using System.Data;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Collections;

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
            DeckinatorReport dr;
            bool skipDeckinator = false;

            if (IsMULTI)
            {
                DataAccess.Instance.IsMultiVote = IsMULTI;
                dr = new DeckinatorReport("MultiVote");
            }
            else
            {
                dr = new DeckinatorReport();
            }

            // Assignment of GMC Rotation property
            if (IsLA == false)
            {
                Rotation = 1;
            }
            OvalData = new DataTable();
            OvalData = ovaldt;

            
            string columnToSearch = "BallotImageFront";

            string ovalFileName = dt.Rows[0]["BallotImage"].ToString();
            int maxVote = Convert.ToInt32(dt.Compute("max(MaxVotes)", string.Empty));

            if (IsMULTI && maxVote < 2)
                skipDeckinator = true;

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
                            SetDataTableRowsForOutput(count, ref rowIndex);
                            RotationGMCOutputDataTable(Rotation, TempDt);
                            break;
                        }
                    }
                    break;
                }
            }

            if (skipDeckinator == false)
            {
                drDt = dr.QueryDeckReportDTByImageName(ovalFileName);
                dr.SetDataAccessDeckinatorDT(Rotation, drDt);
            }
        }

        private int ComputePageBack(string backImage)
        {
            int totalVotes = 0;
            DataTable backImageDT = new DataTable();
            IEnumerable<DataRow> queryBackImages = from b in OvalData.AsEnumerable()
                                                   where b.Field<string>("BallotImageFront") == backImage
                                                   && b.Field<string>("Pge") != "1.00"
                                                   select b;

            if (queryBackImages.Any())
            {
                backImageDT = queryBackImages.CopyToDataTable();

                int maxVotes = Convert.ToInt32(backImageDT.Compute("max(MaxVotes)", string.Empty));
                totalVotes = Convert.ToInt32(backImageDT.Compute("max(OvalPosition)", string.Empty));

                if (maxVotes < 2 && IsMULTI)
                {
                    totalVotes = 0;
                }
            }

            return totalVotes;
        }

        private void SetDataTableRowsForOutput(int count, ref int rowIndex)
        {
            DataTable ovalDataDT = new DataTable();
            int sortKey = 1;
            string page = string.Empty;

            for (int i = 0; i < count; i++)
            {
                TempDt.Rows.Add();

                page = string.Format(OvalData.Rows[rowIndex]["Pge"].ToString().Substring(0, 1));
                TempDt.Rows[i]["SortKey"] = sortKey;
                TempDt.Rows[i]["Card"] = OvalData.Rows[rowIndex]["CardStyle"];
                TempDt.Rows[i]["BallotStyle"] = OvalData.Rows[rowIndex]["BallotStyle"];
                TempDt.Rows[i]["PrecinctID"] = OvalData.Rows[rowIndex]["PrecinctID"];
                TempDt.Rows[i]["Page"] = page;
                TempDt.Rows[i]["BallotImageFront"] = PdfFileName;
                TempDt.Rows[i]["BallotImageBack"] = OvalData.Rows[rowIndex]["BallotImageBack"];
                TempDt.Rows[i]["WatermarkColor"] = OvalData.Rows[rowIndex]["WatermarkColor"];
                TempDt.Rows[i]["WatermarkName"] = OvalData.Rows[rowIndex]["WatermarkName"];                
                TempDt.Rows[i]["HeaderLeft1"] = OvalData.Rows[rowIndex]["HeaderLeft1"];
                TempDt.Rows[i]["HeaderLeft2"] = OvalData.Rows[rowIndex]["HeaderLeft2"];
                TempDt.Rows[i]["HeaderLeft3"] = OvalData.Rows[rowIndex]["HeaderLeft3"];
                TempDt.Rows[i]["HeaderRight1"] = OvalData.Rows[rowIndex]["HeaderRight1"];
                TempDt.Rows[i]["HeaderRight2"] = OvalData.Rows[rowIndex]["HeaderRight2"];
                TempDt.Rows[i]["HeaderRight3"] = OvalData.Rows[rowIndex]["HeaderRight3"];
                TempDt.Rows[i]["Party"] = OvalData.Rows[rowIndex]["Party"];
                TempDt.Rows[i]["ILK"] = AddIlkToData();                

                if (TempDt.Rows[i]["BallotImageBack"].ToString() == "NO BACK")
                {
                    TempDt.Rows[i]["PageFront"] = sortKey;
                    TempDt.Rows[i]["PageBack"] = 0;
                }
                else
                {
                    string backImage = TempDt.Rows[0]["BallotImageBack"].ToString();
                    int backVotes = ComputePageBack(backImage) + 1;

                    if (sortKey == 1)
                    {
                        TempDt.Rows[i]["PageFront"] = 1;
                        TempDt.Rows[i]["PageBack"] = 1;
                    }
                    else
                    {
                        TempDt.Rows[i]["PageFront"] = sortKey;

                        if (sortKey <= backVotes)
                        {
                            TempDt.Rows[i]["PageBack"] = sortKey;
                        }
                        else
                        {
                            TempDt.Rows[i]["PageBack"] = backVotes;
                        }
                        
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
                    //DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                    loops++; //sequenceNum++;
                    row++;
                }
                else
                {
                    if (loops <= rotation)
                    {
                        for (int i = 0; i < loops; i++)
                        {
                            DataAccess.Instance.VotePositionTable.Rows.Add(item.ItemArray);
                            //DataAccess.Instance.VotePositionTable.Rows[row]["Sequence"] = sequenceNum.ToString();
                            //sequenceNum++;
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
                testdeckIlk = "LA";
            else if (IsQC)
                testdeckIlk = "QC";
            else if (IsWHSE)
                testdeckIlk = "WH";
            else if (IsMULTI)
                testdeckIlk = "MV";

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
                TempDt.Columns.Add("Page", typeof(string));
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

        private void SetTotalBallotCount()
        {
            DataTable imageOneMaxVotes = new DataTable();
            DataTable imageName = new DataTable();
            DataTable multiMaxVotes = new DataTable();
            DataTable transferDT = DataAccess.Instance.DeckinatorTable.Clone();

            DataView dv = DataAccess.Instance.DeckinatorTable.DefaultView;
            dv.Sort = "TotalVotes DESC";
            DataAccess.Instance.DeckinatorTable = dv.ToTable();

            IEnumerable<DataRow> images = from i in DataAccess.Instance.DeckinatorTable.AsEnumerable()
                                               group i by i.Field<string>("BallotImageFront") into grp
                                               select grp.FirstOrDefault();

            imageName = images.CopyToDataTable();

            for (int j = 0; j < imageName.Rows.Count; j++)
            {
                int counter = 0;
                int totalVotes = Convert.ToInt32(imageName.Rows[j]["TotalVotes"]);
                int maxVotes = Convert.ToInt32(imageName.Rows[j]["MaxVotes"]);
                string ballotImage = imageName.Rows[j]["BallotImageFront"].ToString();

                // Select all images that equal ballotImage
                IEnumerable<DataRow> allImageNameQuery = from m in DataAccess.Instance.DeckinatorTable.AsEnumerable()
                                                    where m.Field<string>("BallotImageFront") == ballotImage
                                                    select m;

                imageOneMaxVotes = allImageNameQuery.CopyToDataTable();

                int raceID = Convert.ToInt32(imageOneMaxVotes.Rows[0]["RaceID"]);

                foreach (DataRow item in imageOneMaxVotes.Rows)
                {
                    if (ballotImage == imageOneMaxVotes.Rows[counter]["BallotImageFront"].ToString())
                    {
                        if (!IsMULTI)
                        {
                            if (totalVotes == 1||IsWHSE||IsQC)
                            {
                                if (IsQC)
                                {
                                    transferDT.Rows.Add(item.ItemArray);
                                    transferDT.Rows[transferDT.Rows.Count - 1]["TotalBallots"] = 2;
                                    counter++;
                                }
                                else
                                {
                                    transferDT.Rows.Add(item.ItemArray);
                                    transferDT.Rows[transferDT.Rows.Count - 1]["TotalBallots"] = 1;
                                    counter++;
                                }
                            }
                            else
                            {
                                //Gets count of total ballots that will be printed
                                IEnumerable<DataRow> maxVoteOverOne = from v in imageOneMaxVotes.AsEnumerable()
                                                                      where v.Field<int>("RaceID") == raceID
                                                                      select v;

                                multiMaxVotes = maxVoteOverOne.CopyToDataTable();

                                transferDT.Rows.Add(item.ItemArray);
                                transferDT.Rows[transferDT.Rows.Count - 1]["TotalBallots"] = multiMaxVotes.Rows.Count;
                                counter++;
                            }
                        }
                        else
                        {
                            int i = 0;
                            int maxVotesCount = maxVotes;
                            while (maxVotesCount > 2)
                            {
                                i++;
                                maxVotesCount--;
                            }
                            //Gets count of total ballots that will be printed
                            IEnumerable<DataRow> maxVoteOverOne = from v in imageOneMaxVotes.AsEnumerable()
                                                                  where v.Field<int>("RaceID") == raceID
                                                                  select v;

                            multiMaxVotes = maxVoteOverOne.CopyToDataTable();

                            transferDT.Rows.Add(item.ItemArray);
                            transferDT.Rows[transferDT.Rows.Count - 1]["TotalBallots"] = totalVotes - i;
                            counter++;
                        }
                    }                    
                }
            }
            DataAccess.Instance.DeckinatorTable = transferDT;
            DataView view = DataAccess.Instance.DeckinatorTable.DefaultView;
            view.Sort = "Sequence ASC";
            DataAccess.Instance.DeckinatorTable = view.ToTable();

            transferDT = null;
            imageName = null;
            imageOneMaxVotes = null;
            multiMaxVotes = null;
        }

        public void SaveDeckReportTextFile(string path, string countyID)
        {
            string newPath = path + @"\" + countyID + "-" + AddIlkToData();
            string fileName = path + @"\" + countyID + "-" + AddIlkToData() + "_DECKREPORT.txt";
            var result = new StringBuilder();

            SetTotalBallotCount();

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
                dt = UpdateImageNameAndPage(dt);
                string ilk = AddIlkToData();                
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents (.txt)|*.txt";
                string newPath = path + @"\" + countyId + "-" + ilk;

                string fileName = path + @"\" + countyId + "-" + ilk + "_TESTDECK_GMC.txt";
                decimal tempBox = dt.Rows.Count / FileBreak;
                int boxSplit = Convert.ToInt32(Math.Ceiling(tempBox + 1));
                string[] fileArray = new string[dt.Rows.Count + boxSplit + 1];
                int count =1;
                int i = 0;
                int rowNum = 0;
                string columns = string.Empty;                

                // generates the header record
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
                        row = $"BOX DIVIDER|BOX DIVIDER #{count}|BOX {count} of {boxSplit}||||BoxDividerFront.pdf|1|BoxDividerBack.pdf|2";
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
                using (StreamWriter sw = new StreamWriter(fileName))
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
        }

        private DataTable UpdateImageNameAndPage(DataTable textDt)
        {
            //double pageNumber = 0;
            string backImage = string.Empty;
            int card = 0;
            DataTable tempDt = new DataTable();
            DataTable cardTable = new DataTable();

            IEnumerable<DataRow> queryBackImage = from data in textDt.AsEnumerable()
                                                  where data.Field<string>("Page") != "1"
                                                  group data by data.Field<string>("PrecinctID") into grp
                                                  select grp.First();            

            if (queryBackImage.Any())
            {
                tempDt = queryBackImage.CopyToDataTable();

                foreach (DataRow dr in tempDt.Rows)
                {
                    backImage = dr.Field<string>("BallotImageFront");
                    card = dr.Field<int>("Card");
                    //pct = Convert.ToInt32(dr.Field<string>("PrecinctID"));

                    IEnumerable<DataRow> queryCards = from c in textDt.AsEnumerable()
                                                      where c.Field<int>("Card") == card
                                                      select c;
                    cardTable = queryCards.CopyToDataTable();

                    for (int i = 0; i < cardTable.Rows.Count; i++)
                    {
                        DataRow[] row = textDt.Select(string.Format("[Card] = '{0}'", card));

                        row[i]["BallotImageBack"] = backImage;
                    }
                }

                IEnumerable<DataRow> queryRecords = from recs in textDt.AsEnumerable()
                                                    where recs.Field<string>("Page") == "1"
                                                    select recs;

                DataAccess.Instance.VotePositionTable = queryRecords.CopyToDataTable();
            }
            else
            {
                MessageBox.Show(
                    "If you received this message, that means there are no backs to process for this Ilk" + 
                    "\n\nHowever, if there ARE back images, be sure to move them with the corresponding front images.", 
                    "There are *NO BACK* images for this Testdeck Ilk", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            int sequence = 1;

            for (int i = 0; i < DataAccess.Instance.VotePositionTable.Rows.Count; i++)
            {
                DataAccess.Instance.VotePositionTable.Rows[i]["Sequence"] = sequence.ToString();
                sequence++;
            }

            return DataAccess.Instance.VotePositionTable; 
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
