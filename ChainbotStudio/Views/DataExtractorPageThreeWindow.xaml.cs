using System.Windows;
using System.Windows.Controls;


namespace ChainbotStudio.Views
{
    public partial class DataExtractorPageThreeWindow : Page
    {
        public DataExtractorPageThreeWindow()
        {
            InitializeComponent();
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBox1.IsChecked)
            {
                MessageBox.Show(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage4, Chainbot.Resources.Properties.Resources.MessageBox_WarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                checkBox.IsChecked = true;
            } 
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBox.IsChecked)
            {
                MessageBox.Show(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage4, Chainbot.Resources.Properties.Resources.MessageBox_WarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                checkBox1.IsChecked = true;
            }  
        }
    }
}
