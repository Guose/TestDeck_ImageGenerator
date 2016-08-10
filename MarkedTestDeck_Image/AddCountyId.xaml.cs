using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CountyId.DAL;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;

namespace MarkedTestDeck_Image
{
    /// <summary>
    /// Interaction logic for AddCountyId.xaml
    /// </summary>
    public partial class AddCountyId : Page
    {
        private MainWindow main = new MainWindow();
        private IVSDataAccess data;
        public AddCountyId()
        {
            InitializeComponent();
        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            data = new IVSDataAccess();

            if (txtState.Text != "" && txtCountyName.Text != "")
            {
                try
                {
                    // Add PopulateCountyId Method after equal sign
                    txtCountyId.Text = data.PopulateCountyId(txtState.Text, txtCountyName.Text);
                    data.CountyName = txtCountyName.Text;
                    data.State = txtState.Text;
                    data.Tabulation = txtTabulation.Text;
                    data.CountyId = txtCountyId.Text;
                    data.AddCountyToDBSource();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    this.Visibility = Visibility.Hidden;
                    txtCountyId.Text = string.Empty;
                    txtCountyName.Text = string.Empty;
                    txtState.Text = string.Empty;
                    txtTabulation.Text = string.Empty;
                }
            }
            else
            {
                MessageBox.Show("County Name field or the State field, cannot be null", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void chkOverwrite_Checked(object sender, RoutedEventArgs e)
        {
            txtCountyId.IsEnabled = true;
            txtCountyId.Background = Brushes.White;
        }

        private void chkOverwrite_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCountyId.IsEnabled = false;
            txtCountyId.Background = Brushes.LightGray;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            data = new IVSDataAccess();
            try
            {
                if (txtCountyName.Text != "" || txtState.Text != "")
                {
                    txtCountyId.Text = data.PopulateCountyId(txtState.Text, txtCountyName.Text);
                }
                else
                {
                    throw new ArgumentNullException("County Name and State fields cannot be null", "ERROR:"); 
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FIELDS CANNOT BE NULL", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void TextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
    }
}
