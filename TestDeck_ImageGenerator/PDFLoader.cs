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
        public int MaxVotes { get; set; }

        public List<string> RetrievePdfFileNames()
        {
            string[] files = Directory.GetFiles(PdfPath, "*.pdf");
            List<string> fileList = new List<string>();

            foreach (string file in files)
            {
                fileList.Add(file);
            }
            return fileList;
        }

        public void LoadPdfDocument(DataTable dt, DataTable ovaldt, int rotation, string countyID)
        {
            string ilk = string.Empty;
            string date = DateTime.Now.ToString("yyyyMMdd");
            int candidate = 0;
            PositionDT = dt;

            try
            {
                PdfDocument pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open
                    (PdfPath + PdfFileName, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                PdfDocument pdfNewDoc = new PdfDocument();
                MaxNumOfCandidate = Convert.ToInt32(PositionDT.Compute("max(TtlRaceOvals)", string.Empty));

                if (IncludeWriteIns == false && IsLA && MaxNumOfCandidate > 1)
                {
                    for (int i = 0; i < PositionDT.Rows.Count; i++)
                    {
                        candidate = Convert.ToInt32(PositionDT.Rows[i]["IsWriteIn"]);

                        if (candidate > 0)
                        {
                            MaxNumOfCandidate = MaxNumOfCandidate - 1;
                            break;
                        }
                    }
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
                    MaxVotes = Convert.ToInt32(PositionDT.Compute("max(MaxVotes)", string.Empty));
                    Count++;
                    pdfNewDoc.AddPage(pdfDoc.Pages[0]);
                    XGraphics gfx = XGraphics.FromPdfPage(pdfNewDoc.Pages[pg]);
                    if (IsLA)
                    {
                        MarkPdf_LandADeck(PositionDT, gfx);
                        _gmc.IsLA = IsLA;
                        ilk = "L&A";
                    }
                    else if (IsQC)
                    {
                        if (pg == 1)
                        {
                            MarkPdf_QCDeck(PositionDT, gfx);
                        }
                        _gmc.IsQC = IsQC;
                        ilk = "QC";
                    }
                    else if (IsWHSE)
                    {
                        MarkPdf_WHSEDeck(PositionDT, gfx);
                        _gmc.IsWHSE = IsWHSE;
                        ilk = "WHSE";
                    }
                    else if (IsMULTI)
                    {
                        if (MaxVotes > 1)
                        {
                            MarkPdf_MULTIDeck(PositionDT, gfx);
                            _gmc.IsMULTI = IsMULTI;
                            ilk = "MULTIVOTE";
                        }
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
                    _gmc.GenerateOutputTextFile(dt, ovaldt, Count);

                    pdfNewDoc.Save(savedImages + @"\" + pdfName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadPDFDocument Method ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void MarkPdf_MULTIDeck(DataTable dt, XGraphics gfx)
        {
            DataTable _maxVotes = new DataTable();
            int myiterator = 0;
            bool process = true;
            try
            {
                IEnumerable<DataRow> multiVotes = from m in dt.AsEnumerable()
                                                  where m.Field<int>("MaxVotes") > 1
                                                  select m;
                _maxVotes = multiVotes.CopyToDataTable();

                int totalVotes = Convert.ToInt32(_maxVotes.Rows[0]["TtlRaceOvals"]);
                int maxVotes = Convert.ToInt32(_maxVotes.Rows[0]["MaxVotes"]);

                if (MaxVotes > 2 && Count == totalVotes)
                {
                    process = false;
                }

                if (process)
                {
                    if (Count == 1)
                        MaxVotes = MaxVotes + 1;

                    if (Count >= 3)
                    {
                        myiterator = RowIndex - 1;
                        MaxVotes = myiterator + MaxVotes;
                    }
                    for (int i = myiterator; i < MaxVotes; i++)
                    {
                        SetCoordinatesForTestDeck(i, _maxVotes, gfx);

                        if (i == totalVotes - 1 && maxVotes > 2)
                        {
                            MaxNumOfCandidate = MaxNumOfCandidate - 1;
                            break;
                        }
                    }
                    RowIndex++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MULTI DECK ERROR");
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


        private void MarkPdf_WHSEDeck(DataTable dt, XGraphics gfx)
        {
            int racePosn = 0;
            int totalInRace = 0;
            bool writeIn = false;
            DataTable queryDT = new DataTable();

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    racePosn = Convert.ToInt32(dt.Rows[i]["RacePosn"]);
                    totalInRace = Convert.ToInt32(dt.Rows[i]["TtlRaceOvals"]);
                    IsWriteIn = Convert.ToInt32(dt.Rows[i]["IsWriteIn"]);

                    if (racePosn == totalInRace)
                    {
                        if (IsWriteIn == 1)
                            writeIn = true;

                        if (writeIn == false)
                        {
                            SetCoordinatesForTestDeck(i, dt, gfx);
                        }
                        else
                        {
                            if (writeIn && IncludeWriteIns)
                            {
                                SetCoordinatesForTestDeck(i, dt, gfx);
                            }
                            else
                            {
                                int raceNbr = Convert.ToInt32(dt.Rows[i]["RaceNbr"]);

                                IEnumerable<DataRow> queryWriteIn = from q in dt.AsEnumerable()
                                                                    where q.Field<int>("RaceNbr") == raceNbr
                                                                    && q.Field<int>("IsWriteIn") < 1
                                                                    select q;

                                queryDT = queryWriteIn.CopyToDataTable();

                                for (int j = 0; j < queryDT.Rows.Count; j++)
                                {
                                    int newRacePos = Convert.ToInt32(queryDT.Rows[j]["RacePosn"]);
                                    if (newRacePos == racePosn - 1)
                                    {
                                        SetCoordinatesForTestDeck(j, queryDT, gfx);
                                    }
                                }                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WHSE DECK ERROR");
            }
        }

        private void MarkPdf_QCDeck(DataTable dt, XGraphics gfx)
        {
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SetCoordinatesForTestDeck(i, dt, gfx);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "QC DECK ERROR");
            }
        }

        private void MarkPdf_LandADeck(DataTable dt, XGraphics gfx)
        {
            int racePosn = 0;
            bool isWriteIn = false;

            try
            {
                for (int i = RowIndex; i < dt.Rows.Count; i++)
                {
                    racePosn = Convert.ToInt32(dt.Rows[i]["RacePosn"]);
                    IsWriteIn = Convert.ToInt32(dt.Rows[i]["IsWriteIn"]);

                    if (racePosn == Count)
                    {
                        if (IsWriteIn == 1)
                            isWriteIn = true;

                        if (isWriteIn == false)
                        {
                            SetCoordinatesForTestDeck(i, dt, gfx);
                        }
                        else
                        {
                            if (isWriteIn && IncludeWriteIns)
                            {
                                SetCoordinatesForTestDeck(i, dt, gfx);
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
