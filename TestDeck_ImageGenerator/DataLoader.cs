using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using XYCoordinates;
using System.Linq;

namespace TestDeck_ImageGenerator
{
    public class DataLoader
    {
        public DataLoader(string path, string fileName) : this()
        {
            FilePath = path;
            FileName = fileName;
        }

        public DataLoader()
        {

        }
        public int[] XCoord { get; set; }
        public int[] YCoord { get; set; }
        public int MaxNumOfCandidate { get; set; }
        public int IsWriteIn { get; set; }
        public DataTable PositionDT { get; set; }
        public DataTable OvalDT { get; set; }
        public string FilePath { get; set; }
        public string OvalFileName { get; set; }
        public string FileName { get; set; }

        public List<string> RetrieveArrowDumpFiles()
        {
            string[] files = Directory.GetFiles(FilePath, "*.dat");
            List<string> fileList = new List<string>();

            foreach (string file in files)
            {
                fileList.Add(file);
            }
            return fileList;
        }

        public List<string> RetrievePdfFileNames(string path)
        {
            DataTable pdfName = new DataTable();
            //string[] files = Directory.GetFiles(path, "*.pdf");
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

        public void GetCoordinates()
        {
            XCoord = new int[PositionDT.Rows.Count];
            YCoord = new int[PositionDT.Rows.Count];
            for (int i = 0; i < PositionDT.Rows.Count; i++)
            {
                XCoord[i] = Convert.ToInt32(PositionDT.Rows[i]["XCoord"]);
                YCoord[i] = Convert.ToInt32(PositionDT.Rows[i]["YCoord"]);
            }
        }
        public DataTable GeneratePositionDataTable(DataTable dt)
        {
            ArrowCoordinates xyc = new ArrowCoordinates(OvalDT);
            dt = xyc.GetXYCoordinates(FileName);
            
            dt.Columns.Add("RaceNbr", typeof(int));
            dt.Columns.Add("RacePosn", typeof(int));
            dt.Columns.Add("TtlRaceOvals", typeof(int));
            dt.Columns.Add("RecNbr", typeof(int));
            dt.Columns.Add("IsWriteIn", typeof(int));
            dt.Columns.Add("MaxVotes", typeof(int));

            dt = UpdatePositionData(dt);
            DataView dv = dt.DefaultView;
            dv.Sort = "RacePosn";
            dt = dv.ToTable();

            return dt;
        }

        private DataTable UpdatePositionData(DataTable dt)
        {
            OvalFileName = dt.Rows[0]["BallotImage"].ToString();

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
            RetrieveRaceNbr(dt);
            
            return dt;
        }

        private DataTable RetrieveRaceNbr(DataTable dt)
        {
            int totalArrows = 0;
            int racePosn = 0;
            int raceNbr = 1;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                totalArrows = Convert.ToInt32(dt.Rows[i]["TtlRaceOvals"]);
                racePosn = Convert.ToInt32(dt.Rows[i]["RacePosn"]);

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
    }
}
