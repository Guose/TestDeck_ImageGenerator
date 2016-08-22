using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDeck_ImageGenerator;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace RunPdfProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            string pdfPath = string.Empty;
            bool includeWriteIns = false;
            Console.WriteLine("Please enter rotation value for Logic & Accuracy Testdeck");
            string rotationEntry = Console.ReadLine();

            Console.WriteLine("Please enter County Id");
            string countyId = Console.ReadLine();

            int rotation = int.Parse(rotationEntry);
            DataLoader dl = new DataLoader();

            // Path of files and images
            dl.FilePath = @"C:\Users\Justin\Desktop\Visual Studio\IVS\Data\TestDecks\COELBERT\";

            pdfPath = dl.FilePath + @"Images\";

            // Oval data from GEMS textfile name and write to datatable
            dl.OvalFileName = "COELBERT_T_Ovals_Style.txt";

            dl.OvalDT = FileDelimeter.DataFromTextFile(dl.FilePath + dl.OvalFileName, '|');

            GMCTextFile gmc = new GMCTextFile(dl.OvalDT, dl.FilePath);

            // Get arrow position file name
            foreach (var file in dl.RetrievePdfFileNames(pdfPath))
            {
                stopWatch.Start();
                DataTable dt = new DataTable();

                dl.FileName = file;

                dl.PositionDT = dl.GeneratePositionDataTable();

                PDFLoader pdf = new PDFLoader(pdfPath, dl.RetrieveFileName(), includeWriteIns);

                pdf.LoadPdfDocument(dl.PositionDT, dl.OvalDT, rotation, countyId);

                string name = Path.GetFileName(file);

                stopWatch.Stop();

                Console.WriteLine("\n MARK BALLOTS total time: {0} for IMAGE: {1}", stopWatch.Elapsed.ToString(), name);

                stopWatch.Reset();                
            }
            stopWatch.Stop();            

            stopWatch.Reset();
            stopWatch.Start();

            string savedImages = Path.Combine(pdfPath, "Processed Images");

            gmc.SaveGMCTextfile(savedImages, DataAccess.Instance.VotePositionTable, countyId);

            stopWatch.Stop();

            Console.WriteLine("\n\n WRITE TEXTFILE total time: {0}", stopWatch.Elapsed.ToString());

            Console.ReadKey();

        }
    }
}
