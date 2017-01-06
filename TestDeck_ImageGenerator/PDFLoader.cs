using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PdfSharp.Drawing;
using System.Data;
using System.Windows;

namespace TestDeck_ImageGenerator
{
    public class PDFLoader : DataLoader
    {
        private PdfDrawLine _pdf;
        public PDFLoader(string path, string fileName, bool includeWriteIns) : this()
        {
            PdfPath = path;
            PdfFileName = fileName;
            IncludeWriteIns = includeWriteIns;
        }

        public PDFLoader()
        {

        }

        private GMCTextFile _gmc = new GMCTextFile();

        public string PdfPath { get; set; }
        public string PdfFileName { get; set; }
        public int Count { get; set; }
        public int RowIndex { get; set; }
        public bool IncludeWriteIns { get; set; }
        public bool IsLA { get; set; }
        public bool IsWHSE { get; set; }
        public bool IsQC { get; set; }
        public bool IsMULTI { get; set; }     

        public void LoadPdfDocument(DataTable posnDt, DataTable ovaldt, int rotation, string countyID)
        {
            string ilk = string.Empty;
            string date = DateTime.Now.ToString("yyyyMMdd");
            DataTable tempLATable = new DataTable();

            try
            {
                PdfDocument pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open
                    (PdfPath + PdfFileName, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                PdfDocument pdfNewDoc = new PdfDocument();
                MaxNumOfCandidate = Convert.ToInt32(posnDt.Compute("max(TtlRaceOvals)", string.Empty));
                int totalWriteIns = Convert.ToInt32(posnDt.Compute("max(TtlWriteIns)", string.Empty));
                int page = Convert.ToInt16(posnDt.Rows[0]["Page"]);

                if (IncludeWriteIns == false && IsLA && MaxNumOfCandidate > 1)
                {
                    var decrementMaxCand = from m in posnDt.AsEnumerable()
                                           where m.Field<int>("IsWriteIn") < 1
                                           select m;

                    tempLATable = decrementMaxCand.CopyToDataTable();

                    MaxNumOfCandidate = Convert.ToInt32(tempLATable.Compute("max(RacePosn)", string.Empty));
                }

                if (page > 1)
                {
                    MaxNumOfCandidate = MaxNumOfCandidate + 1;
                }

                if (IsQC || IsWHSE)
                {
                    if (IsQC)
                        MaxNumOfCandidate = 2;
                    else
                        MaxNumOfCandidate = 1;
                }

                for (int pg = 0; pg < MaxNumOfCandidate; pg++)
                {                    
                    Count++;
                    pdfNewDoc.AddPage(pdfDoc.Pages[0]);
                    XGraphics gfx = XGraphics.FromPdfPage(pdfNewDoc.Pages[pg]);
                    if (IsLA)
                    {
                        MarkPdf_LandADeck(posnDt, gfx);
                        _gmc.IsLA = IsLA;
                        ilk = "LA";
                    }
                    else if (IsQC)
                    {
                        if (pg == 1)
                        {
                            MarkPdf_QCDeck(posnDt, gfx);
                        }
                        _gmc.IsQC = IsQC;
                        ilk = "QC";
                    }
                    else if (IsWHSE)
                    {
                        MarkPdf_WHSEDeck(posnDt, gfx);
                        _gmc.IsWHSE = IsWHSE;
                        ilk = "WHSE";
                    }
                    else if (IsMULTI)
                    {
                        MarkPdf_MULTIDeck(posnDt, ovaldt, gfx);
                        _gmc.IsMULTI = IsMULTI;
                        ilk = "MULTIVOTE";
                    }
                }
                if (ilk != "")
                {
                    string savedImages = Path.Combine(PdfPath, countyID + "_Processed Images", countyID + "-" + date + "_" + ilk + "_TESTDECK");
                    Directory.CreateDirectory(savedImages);

                    PdfPath = savedImages;
                    string pdfName = ilk + "-MarkedTest_" + PdfFileName;

                    _gmc.PdfFileName = pdfName;
                    _gmc.Rotation = rotation;
                    _gmc.GenerateOutputTextFile(posnDt, ovaldt, Count);

                    pdfNewDoc.Save(savedImages + @"\" + pdfName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadPDFDocument Method ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private int myIterator = 0;
        private int _TotalVotes = 0;

        private void MarkPdf_MULTIDeck(DataTable posnDT, DataTable ovalDT, XGraphics gfx)
        {
            if (Count >= 3)
            {
                myIterator++;
            }
            DataTable _maxVotesDT = new DataTable();
            DataTable raceIdDT = new DataTable();
            DataTable queryRaceId = new DataTable();

            IEnumerable<DataRow> raceCount = from rc in posnDT.AsEnumerable()
                                             group rc by rc.Field<int>("RaceID") into grp
                                             select grp.First();

            raceIdDT = raceCount.CopyToDataTable();

            DataView dv = raceIdDT.DefaultView;
            dv.Sort = "TtlRaceOvals ASC";
            raceIdDT = dv.ToTable();            

            int totalVotes = 0;
            int maxVotes = 0;
            int ovalPosn = 0;
            int raceId = 0;
            int counter = 0;
            

            for (int j = 0; j < raceIdDT.Rows.Count; j++)
            {
                int i = 0;
                bool process = true;
                raceId = Convert.ToInt32(raceIdDT.Rows[j]["RaceID"]);

                // totalVotes is the highest number of candidates of the multivote races
                totalVotes = Convert.ToInt32(raceIdDT.Rows[j]["TtlRaceOvals"]);
                // maxVotes is the maximum votes for this ballot
                maxVotes = Convert.ToInt32(raceIdDT.Rows[j]["MaxVotes"]);

                IEnumerable<DataRow> raceIdQuery = from race in posnDT.AsEnumerable()
                                                   where race.Field<int>("RaceID") == raceId
                                                   select race;

                queryRaceId = raceIdQuery.CopyToDataTable();

                _TotalVotes = Convert.ToInt32(queryRaceId.Compute("max(TtlRaceOvals)", string.Empty));

                if (Count == 1)
                {
                    counter = maxVotes + 1;
                }
                if (Count == 2)
                {
                    counter = maxVotes;
                }
                if (Count >= 3)
                {
                    i = myIterator;
                    counter = i + maxVotes;
                }

                if (counter > _TotalVotes)
                {
                    process = false;                    
                }

                if (maxVotes > 2 && counter == _TotalVotes)
                {
                    MaxNumOfCandidate = MaxNumOfCandidate - (maxVotes - 2);
                }    

                if (process)
                {
                    while (i < counter)
                    {
                        ovalPosn = Convert.ToInt32(queryRaceId.Rows[i]["RacePosn"]);
                        string imageName = posnDT.Rows[i]["BallotImage"].ToString();

                        IEnumerable<DataRow> queryCandidate = from c in DataAccess.Instance.OvalDataTable.AsEnumerable()
                                                              where c.Field<string>("RaceID") == raceId.ToString()
                                                              && c.Field<string>("OvalPosition") == ovalPosn.ToString()
                                                              && c.Field<string>("BallotImageFront") == imageName
                                                              select c;

                        foreach (var item in queryCandidate)
                        {
                            DataAccess.Instance.MultiVoteTable.Rows.Add(item.ItemArray);
                        }

                        SetCoordinatesForTestDeck(i, queryRaceId, gfx);
                        i++;
                    }

                    RowIndex++;

                    //if (counter == totalVotes)
                    //    MaxNumOfCandidate = MaxNumOfCandidate - 1;
                }                
            }       
        }

        private void SetCoordinatesForTestDeck(int z, DataTable data, XGraphics gfx)
        {
            int x = 0;
            int y = 0;
            _pdf = new PdfDrawLine();
            
            x = Convert.ToInt32(data.Rows[z]["XCoord"]);
            y = Convert.ToInt32(data.Rows[z]["YCoord"]);

            _pdf.DrawLine(gfx, x, y);
        }


        private void MarkPdf_WHSEDeck(DataTable posnDT, XGraphics gfx)
        {
            int racePosn = 0;
            int maxRaceCand = 0;

            try
            {
                IEnumerable<DataRow> queryRaces = from race in posnDT.AsEnumerable()
                                                  group race by race.Field<int>("RaceID") into grp
                                                  select grp.First();

                DataTable races = queryRaces.CopyToDataTable();

                foreach (DataRow dr in races.Rows)
                {
                    IEnumerable<DataRow> equalRaces = from r in posnDT.AsEnumerable()
                                                      where r.Field<int>("RaceID") == Convert.ToInt32(dr["RaceID"])
                                                      && r.Field<int>("IsWriteIn") == 0
                                                      select r;

                    DataTable racePosnDT = equalRaces.CopyToDataTable();

                    for (int i = 0; i < racePosnDT.Rows.Count; i++)
                    {
                        racePosn = Convert.ToInt32(racePosnDT.Rows[i]["RacePosn"]);
                        maxRaceCand = Convert.ToInt32(racePosnDT.Compute("max(RacePosn)", string.Empty));

                        if (racePosn == maxRaceCand)
                        {
                            SetCoordinatesForTestDeck(i, racePosnDT, gfx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WHSE DECK ERROR");
            }
        }

        private void MarkPdf_QCDeck(DataTable posnDT, XGraphics gfx)
        {
            try
            {
                for (int i = 0; i < posnDT.Rows.Count; i++)
                {
                    SetCoordinatesForTestDeck(i, posnDT, gfx);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QC DECK ERROR");
            }
        }

        private void MarkPdf_LandADeck(DataTable posnDT, XGraphics gfx)
        {
            int racePosn = 0;            

            try
            {
                for (int i = RowIndex; i < posnDT.Rows.Count; i++)
                {
                    racePosn = Convert.ToInt32(posnDT.Rows[i]["RacePosn"]);
                    IsWriteIn = Convert.ToInt32(posnDT.Rows[i]["IsWriteIn"]);
                    bool isWriteIn = false;

                    if (racePosn == Count)
                    {
                        if (IsWriteIn == 1)
                            isWriteIn = true;

                        if (isWriteIn == false)
                        {
                            SetCoordinatesForTestDeck(i, posnDT, gfx);
                        }
                        else
                        {
                            if (isWriteIn && IncludeWriteIns)
                            {
                                SetCoordinatesForTestDeck(i, posnDT, gfx);
                            }
                        }
                        RowIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "L&A DECK ERROR");
            }
        }
    }
}
