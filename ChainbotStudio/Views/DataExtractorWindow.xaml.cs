using ActiproSoftware.Windows.Controls.Ribbon;
using Panuon.UI.Silver;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotStudio.Views
{
    /// <summary>
    /// Description for DataExtractorWindow.
    /// </summary>
    public partial class DataExtractorWindow : RibbonWindow
    {
        /// <summary>
        /// Initializes a new instance of the DataExtractorWindow class.
        /// </summary>
        public DataExtractorWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            oneWindow = new DataExtractorPageOneWindow();
            twoWindow = new DataExtractorPageTwoWindow();
            threeWindow = new DataExtractorPageThreeWindow();
            fourWindow = new DataExtractorPageFourWindow();
            Change_Page.Content = new Frame()
            {
                Content = oneWindow
            };
        }

        public void InitWindow()
        {
            threeWindow.checkBox1.IsChecked = false;
            btnNext.Content = Chainbot.Resources.Properties.Resources.DataExtractor_NextButton;
        }

        public void ClearDataTable()
        {
            fourWindow.ClearDataTable();
        }

        public void ChangePageOne()
        {
            Change_Page.Content = new Frame()
            {
                Content = oneWindow
            };
        }

        public void ChangePageTwo()
        {
            Change_Page.Content = new Frame()
            {
                Content = twoWindow
            };
        }

        public void ChangePageThree()
        {
            Change_Page.Content = new Frame()
            {
                Content = threeWindow
            };
        }

        public void SetUrlCheckStatus(bool bShowUrl)
        {
            threeWindow.checkBox1.IsEnabled = bShowUrl;
            threeWindow.textBox1.IsEnabled = bShowUrl;
        }

        public bool GetUrlCheckStatus()
        {
            return (bool)threeWindow.checkBox1.IsChecked;
        }

        public bool GetTitleCheckStatus()
        {
            return (bool)threeWindow.checkBox.IsChecked;
        }

        public void SetDataExtractorButtonStatus(bool bStatus)
        {
            if (bStatus)
                btnMoreButton.Visibility = Visibility.Visible;
            else
                btnMoreButton.Visibility = Visibility.Hidden;
        }

        public void ChangePageFour()
        {
            Change_Page.Content = new Frame()
            {
                Content = fourWindow
            };
        }

        public void SetReturnButtonStauts(bool bShow)
        {
            if (bShow)
                btnReturn.Visibility = Visibility.Visible;
            else
                btnReturn.Visibility = Visibility.Hidden;
        }

        public void InitPageFourData(string strContent, bool bCheckPage)
        {
            if (bCheckPage)
            {
                bool cbTextCheck = (bool)threeWindow.checkBox.IsChecked;
                bool cbUrlCheck = (bool)threeWindow.checkBox1.IsChecked;
                if (cbTextCheck && cbUrlCheck)
                {
                    string colName = threeWindow.textBox.Text;
                    string colName1 = threeWindow.textBox1.Text;
                    fourWindow.InitPageData(colName, colName1, strContent);
                }
                else if (cbTextCheck)
                {
                    string colName = threeWindow.textBox.Text;
                    fourWindow.InitPageData(colName, strContent);
                }
                else if (cbUrlCheck)
                {
                    string colName1 = threeWindow.textBox1.Text;
                    fourWindow.InitPageData(colName1, strContent);
                }
            }
            else
            {
                fourWindow.InitPageData(strContent);
            }
        }

        private DataExtractorPageOneWindow oneWindow;
        private DataExtractorPageTwoWindow twoWindow;
        private DataExtractorPageThreeWindow threeWindow;
        private DataExtractorPageFourWindow fourWindow;
    }
}