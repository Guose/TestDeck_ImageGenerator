﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using TestDeck_ImageGenerator;
using System.Globalization;
using CountyId.DAL;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MarkedTestDeck_Image
{
    public enum ProcessOptions
    {
        ByCard,
        ByBT,
        All
    }
    public enum LARotation
    {
        Two,
        Three,
        Five,
        Max
    }
    public enum TestDeckIlk
    {
        LA,
        MULTI,
        QC,
        WHSE
    }
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class TestOptions
    {
        public ProcessOptions ProcessOpt { get; set; }
        public LARotation Rotation { get; set; }
        public TestDeckIlk TestDeck { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path = string.Empty;
        private string ilk = string.Empty;
        private string savedPath = string.Empty;
        private bool includeWriteIns;
        private string countyID = string.Empty;
        private AddCountyId addCounty;
        private IVSDataAccess data = new IVSDataAccess();
        private TestOptions options = null;        
        private PDFLoader pdfTestIlk = new PDFLoader();
        private DataLoader dl = new DataLoader();
        private GMCTextFile gmc = new GMCTextFile();
        private FileInfo fi;

        public MainWindow()
        {
            InitializeComponent();

            DataAccess.Instance.VotePositionTable = new DataTable();

            options = new TestOptions();
            DataContext = options;
            DragEnter += new DragEventHandler(Rectangle_DragEnter);
            Drop += new DragEventHandler(Rectangle_Drop);            
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            lblDragOvalFile.Visibility = Visibility.Hidden;
            Cursor = Cursors.AppStarting;
            string[] filepaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filepaths)
            {
                if (File.Exists(file))
                {
                    using (TextReader tr = new StreamReader(file))
                    {
                        path = file;
                    }
                }
            }

            DataAccess.Instance.OvalDataTable = FileDelimeter.DataFromTextFile(path, '|');
            lblFileLoaded.Visibility = Visibility.Visible;
            Cursor = Cursors.Arrow;
        }
        private void QueryProcessOptions()
        {
            delUpdateCardCount delCard = new delUpdateCardCount(UpdateCardCountTextbox);
            delUpdateVoteCount delVote = new delUpdateVoteCount(UpdateVoteCountTextbox);

            DataTable qdt = DataAccess.Instance.OvalDataTable;
            DataTable tempdt = new DataTable();
            DataTable joinTable = new DataTable();

            joinTable.Columns.Add("SortID", typeof(string));
            joinTable.Columns.Add("Precinct", typeof(string));

            dl.OvalDT = qdt.Clone();

            try
            {
                tempdt = dl.OvalDT.Clone();
                string sort = QuerySortOvalDT();

                if (options.TestDeck == TestDeckIlk.MULTI)
                {
                    IEnumerable<DataRow> tempQuery = from oval in qdt.AsEnumerable()
                                                     where oval.Field<string>("MaxVotes") != "1"
                                                     group oval by oval.Field<string>(sort) into grp
                                                     select grp.First();
                    if (!tempQuery.Any())
                    {
                        MessageBox.Show("There are no records to process Multivote Testdecks", "Multivote records is null", MessageBoxButton.OK, MessageBoxImage.Information);
                        throw new NullReferenceException("Table does not contain any multivote races");
                    }
                    else
                    {
                        tempdt = tempQuery.CopyToDataTable();
                    }                    
                }
                else
                {
                    IEnumerable<DataRow> tempQuery = from oval in qdt.AsEnumerable()
                                                     group oval by oval.Field<string>(sort) into grp
                                                     select grp.First();

                    tempdt = tempQuery.CopyToDataTable();
                }

                int count = 0;

                for (int i = 0; i < tempdt.Rows.Count; i++)
                {
                    joinTable.Rows.Add();
                    joinTable.Rows[i]["SortID"] = tempdt.Rows[i][sort];
                    joinTable.Rows[i]["Precinct"] = tempdt.Rows[i]["PrecinctID"];

                    count = i + 1;
                    delCard(count.ToString());
                }

                IEnumerable<DataRow> sortQuery = from q in joinTable.AsEnumerable()
                                                 join ov in qdt.AsEnumerable()
                                                 on q.Field<string>("SortID") equals
                                                 ov.Field<string>(sort)
                                                 where q.Field<string>("SortID") == ov.Field<string>(sort)
                                                 && q.Field<string>("Precinct") == ov.Field<string>("PrecinctID")
                                                 select ov;

                dl.OvalDT = sortQuery.CopyToDataTable();

                int votes = 0;
                for (int i = 0; i < dl.OvalDT.Rows.Count; i++)
                {
                    votes = i + 1;
                    delVote(votes.ToString());
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "IS OVAL FILE LOADED TO THE PROGRAM???", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            }
        }

        private string GetPdfFilePathName(string filePath)
        {
            fi = new FileInfo(filePath);
            string pdfPath = fi.DirectoryName + @"\Images\";

            return pdfPath;
        }

        private void DoWorkOnImages()
        {
            DataTable dt = new DataTable();
            delUpdateBallotCount delBallot = new delUpdateBallotCount(UpdateBallotCountTextbox);
            string pdfPath = GetPdfFilePathName(path);
            int imagesProcessed = 0;            

            try
            {
                QueryProcessOptions();

                foreach (var file in dl.RetrievePdfFileNames(pdfPath))
                {
                    delUpdateUITextbox mydel = new delUpdateUITextbox(UpdateUITextbox);

                    dl.FileName = file;
                    dl.PositionDT = dl.GeneratePositionDataTable(dt);
                    PDFLoader pdf = new PDFLoader(pdfPath, dl.RetrieveFileName(), includeWriteIns);
                    SetTestDeckIlk(pdf);

                    if (gmc.IsLA == true)
                    {
                        SelectLARotation();
                    }
                    pdf.LoadPdfDocument(dl.PositionDT, dl.OvalDT, gmc.Rotation, countyID);
                    savedPath = pdf.PdfPath;

                    imagesProcessed++;
                    string name = Path.GetFileName(file);

                    mydel(name, imagesProcessed.ToString());
                }

                SaveGMCTextFile(savedPath);

                delBallot(gmc._count.ToString());
                MessageBox.Show(options.TestDeck.ToString() + " Testdecks are completed \n\n   " + gmc._count + " total Testdecks processed",
                    "PROCESS COMPLETE!", MessageBoxButton.OK, MessageBoxImage.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {                
                gmc._count = 0;
            }
        }


        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            dl.OvalDT = DataAccess.Instance.OvalDataTable;
            includeWriteIns = chkIncludeWriteIns.IsChecked.Value;
            countyID = cboCountyName.SelectedValue.ToString();

            workerThread = new Thread(DoWorkOnImages);
            workerThread.Start();

            Thread.Sleep(500);
            Cursor = Cursors.Arrow;
            MainWindow m = new MainWindow();
        }


        public delegate void delUpdateUITextbox(string fileName, string imageCount);
        public delegate void delUpdateCardCount(string card);
        public delegate void delUpdateVoteCount(string vote);
        public delegate void delUpdateBallotCount(string ballots);
        private Thread workerThread;

        private void UpdateBallotCountTextbox(string ballots)
        {
            Dispatcher.BeginInvoke((Action)(() => txtBallotCount.Text = ballots)
                , System.Windows.Threading.DispatcherPriority.Input);
        }
        private void UpdateVoteCountTextbox(string vote)
        {
            Dispatcher.BeginInvoke((Action)(() => txtVoteCount.Text = vote)
                , System.Windows.Threading.DispatcherPriority.Input);
        }
        private void UpdateCardCountTextbox(string card)
        {
            Dispatcher.BeginInvoke((Action)(() => txtCardCount.Text = card)
                , System.Windows.Threading.DispatcherPriority.Input);            
        }
        private void UpdateUITextbox(string name, string image)
        {
            Dispatcher.BeginInvoke((Action)(() => txtImageFileName.Text = name)
                , System.Windows.Threading.DispatcherPriority.Input);
            Dispatcher.BeginInvoke((Action)(() => txtImageCount.Text = image)
                , System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            PopulateCountyIdToComboBox();
        }        

        private void SaveGMCTextFile(string savedPath)
        {
            //string savedImages = Path.Combine(GetPdfFilePathName(path), "Processed Images");

            string newFileName = gmc.SaveGMCTextfile(savedPath, DataAccess.Instance.VotePositionTable, countyID);

            var lines = File.ReadAllLines(newFileName);
            File.WriteAllLines(newFileName, lines.Take(lines.Length - 1).ToArray());
        }

        private void radLA_Checked(object sender, RoutedEventArgs e)
        {
            grdRotation.IsEnabled = true;
        }

        private void radLA_Unchecked(object sender, RoutedEventArgs e)
        {
            grdRotation.IsEnabled = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }
        private void btnAddCounty_Click(object sender, RoutedEventArgs e)
        {
             addCounty = new AddCountyId();
            frmAddCounty.NavigationService.Navigate(addCounty);
            btnProcess.IsEnabled = false;
            btnClear.IsEnabled = false;
        }

        private void PopulateCountyIdToComboBox()
        {
            foreach (var item in data.AddCustomersToList())
            {
                cboCountyName.Items.Add(item);
            }
        }

        private void frmAddCounty_LostFocus(object sender, RoutedEventArgs e)
        {
            cboCountyName.Items.Clear();
            PopulateCountyIdToComboBox();
            btnProcess.IsEnabled = true;
            btnClear.IsEnabled = true;
        }       

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtBallotCount.Text = "";
            txtCardCount.Text = "";
            txtImageCount.Text = "";
            txtImageFileName.Text = "";
            txtLARotationOther.Text = "";
            txtVoteCount.Text = "";
        }
        private string QuerySortOvalDT()
        {
            string sort = string.Empty;

            switch (options.ProcessOpt)
            {
                case ProcessOptions.ByCard:
                sort = "CardStyle";
                break;
                case ProcessOptions.ByBT:
                sort = "BallotStyle";
                break;
                case ProcessOptions.All:
                sort = "SortKey";
                break;
                default:
                break;
            }
            return sort;
        }

        private void SelectLARotation()
        {
            try
            {
                switch (options.Rotation)
                {
                    case LARotation.Two:
                    gmc.Rotation = 2;
                    break;
                    case LARotation.Three:
                    gmc.Rotation = 3;
                    break;
                    case LARotation.Five:
                    gmc.Rotation = 5;
                    break;
                    case LARotation.Max:
                    gmc.Rotation = 100;
                    break;
                    default:
                    if (txtLARotationOther.Text != "")
                        gmc.Rotation = int.Parse(txtLARotationOther.Text);
                    break;
                }
            }
            catch
            {
                MessageBox.Show("Please select a rotation for Logic & Accuracy Testdeck", "Rotation Cannot Be NULL", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void SetTestDeckIlk(PDFLoader pdf)
        {
            switch (options.TestDeck)
            {
                case TestDeckIlk.LA:
                pdf.IsLA = true;
                gmc.IsLA = true;
                ilk = "L&A";                
                break;
                case TestDeckIlk.MULTI:
                pdf.IsMULTI = true;
                gmc.IsMULTI = true;
                ilk = "MULTI";
                break;
                case TestDeckIlk.QC:
                pdf.IsQC = true;
                gmc.IsQC = true;
                ilk = "QC";
                break;
                case TestDeckIlk.WHSE:
                pdf.IsWHSE = true;
                gmc.IsWHSE = true;
                ilk = "WHSE";
                break;
                default:
                break;
            }
        }        
    }
}