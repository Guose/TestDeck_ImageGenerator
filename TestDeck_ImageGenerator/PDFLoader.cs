using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            PositionDT = dt;
            PdfDocument pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open
                (PdfPath + PdfFileName, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);         

            PdfDocument pdfNewDoc = new PdfDocument();

            string ilk = string.Empty;
            string date = DateTime.Now.ToString("yyyyddMM");

            MaxNumOfCandidate = Convert.ToInt32(PositionDT.Compute("max(TtlRaceOvals)", string.Empty));

            if (IncludeWriteIns == false && IsLA && MaxNumOfCandidate > 1)
                MaxNumOfCandidate = MaxNumOfCandidate - 1;

            // gets an array of vote position coordinates
            GetCoordinates();            

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
                _gmc.AddRowsToTempDT(dt, ovaldt, Count);               

                pdfNewDoc.Save(savedImages + @"\" + pdfName);
            }
            
        }

        private void MarkPdf_MULTIDeck(DataTable dt, XGraphics gfx)
        {
            DataTable _maxVotes = new DataTable();
            int myiterator = 0;
            bool process = true;            

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
                    SetCoordinatesForMultiDeck(i, _maxVotes, gfx);

                    if (i == totalVotes - 1 && maxVotes > 2)
                    {
                        MaxNumOfCandidate = MaxNumOfCandidate - 1;
                        break;
                    }
                }
                RowIndex++;
            }            
        }

        private void SetCoordinatesForMultiDeck(int z, DataTable data, XGraphics gfx)
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
            _pdf = new PdfDrawLine();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                racePosn = Convert.ToInt32(PositionDT.Rows[i]["RacePosn"]);
                totalInRace = Convert.ToInt32(PositionDT.Rows[i]["TtlRaceOvals"]);
                IsWriteIn = Convert.ToInt32(PositionDT.Rows[i]["IsWriteIn"]);

                if (racePosn == totalInRace)
                {
                    if (IsWriteIn == 1)
                        writeIn = true;

                    if (writeIn == false)
                    {
                        _pdf.DrawLine(gfx, XCoord[i], YCoord[i]);
                    }
                    else
                    {
                        if (writeIn && IncludeWriteIns)
                        {
                            _pdf.DrawLine(gfx, XCoord[i], YCoord[i]);
                        }
                        else
                        {
                            _pdf.DrawLine(gfx, XCoord[i - 1], YCoord[i - 1]);
                        }
                    }
                }
            }
        }

        private void MarkPdf_QCDeck(DataTable dt, XGraphics gfx)
        {
            _pdf = new PdfDrawLine();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                _pdf.DrawLine(gfx, XCoord[i], YCoord[i]);
            }
        }

        private void MarkPdf_LandADeck(DataTable dt, XGraphics gfx)
        {
            int racePosn = 0;
            bool isWriteIn = false;            
            _pdf = new PdfDrawLine();

            for (int i = RowIndex; i < dt.Rows.Count; i++)
            {
                racePosn = Convert.ToInt32(PositionDT.Rows[i]["RacePosn"]);
                IsWriteIn = Convert.ToInt32(PositionDT.Rows[i]["IsWriteIn"]);

                if (racePosn == Count)
                {
                    if (IsWriteIn == 1)
                        isWriteIn = true;

                    if (isWriteIn == false)
                    {
                        _pdf.DrawLine(gfx, XCoord[i], YCoord[i]);                        
                    }
                    else
                    {
                        if (isWriteIn && IncludeWriteIns)
                        {
                            _pdf.DrawLine(gfx, XCoord[i], YCoord[i]);
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
    }
}
