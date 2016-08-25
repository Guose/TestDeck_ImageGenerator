using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using BitMiracle.Docotic.Pdf;
using System.Windows;

namespace XYCoordinates
{
    public sealed class ArrowCoordinates
    {
        private DataTable _queryTable = new DataTable();
        public ArrowCoordinates(DataTable dt)
        {
            OvalData = dt;

            if (ArrowDumpDt == null)
            {
                SetArrowDumpDt();
            }
        }

        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public DataTable OvalData { get; set; }
        public string TextToFind { get; set; }
        public List<string> FileNames { get; set; }
        public DataTable ArrowDumpDt { get; set; }

        private void SetArrowDumpDt()
        {
            ArrowDumpDt = new DataTable();

            ArrowDumpDt.Columns.Add("SortKey", typeof(int));
            ArrowDumpDt.Columns.Add("BallotImage", typeof(string));
            ArrowDumpDt.Columns.Add("XCoord", typeof(int));
            ArrowDumpDt.Columns.Add("YCoord", typeof(int));
        }


        public DataTable GetXYCoordinates(string fileName)
        {
            string file = Path.GetFileNameWithoutExtension(fileName);
            string race = string.Empty;
            string match = string.Empty;
            int yesNoCount = 0;

            try
            {
                using (PdfDocument doc = new PdfDocument(fileName))
                {
                    IEnumerable<DataRow> query = from images in OvalData.AsEnumerable()
                                                 where images.Field<string>("BallotImageFront") == file
                                                 select images;

                    _queryTable = query.CopyToDataTable();

                    for (int i = 0; i < _queryTable.Rows.Count; i++)
                    {
                        TextToFind = _queryTable.Rows[i]["Candidate"].ToString();
                        race = _queryTable.Rows[i]["Race"].ToString();                        

                        // loops through pdf for text clips
                        foreach (PdfTextData textData in doc.Pages[0].Canvas.GetTextData())
                        {
                            if (TextToFind == textData.Text)
                            {
                                if (TextToFind == "YES")
                                {
                                    YesCount++;
                                    AddRowsToArrowDumpDT(textData, i, file);
                                    TextToFind = "NO";
                                    i++;
                                }
                                else if (TextToFind == "NO")
                                {
                                    NoCount++;
                                    AddRowsToArrowDumpDT(textData, i, file);
                                    TextToFind = "YES";
                                    i++;
                                }
                                else
                                {
                                    AddRowsToArrowDumpDT(textData, i, file);
                                    break;
                                }
                            }                            
                        }
                        yesNoCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR IN ARROW COORDINATES CLASS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ArrowDumpDt;
        }

        private void AddRowsToArrowDumpDT(PdfTextData textData, int i, string file)
        {
            ArrowDumpDt.Rows.Add();
            ArrowDumpDt.Rows[i]["SortKey"] = i + 1;
            ArrowDumpDt.Rows[i]["BallotImage"] = file;
            if (textData.Position.X < 412)
            {
                ArrowDumpDt.Rows[i]["XCoord"] = 412;
            }
            else if (textData.Position.X > 412 && textData.Position.X < 630)
            {
                ArrowDumpDt.Rows[i]["XCoord"] = 630;
            }

            ArrowDumpDt.Rows[i]["YCoord"] = Math.Round((decimal)textData.Position.Y + 12);
        }


        public List<string> GetPdfFileNames(string path)
        {
            string[] files = Directory.GetFiles(path, "*.pdf");
            FileNames = new List<string>();

            foreach (string file in files)
            {
                FileNames.Add(file);
            }
            return FileNames;
        }

    }
}
