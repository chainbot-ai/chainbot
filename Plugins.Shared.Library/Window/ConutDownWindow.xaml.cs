using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Plugins.Shared.Library.Window
{
    /// <summary>
    /// Description for ConutDownWindow.
    /// </summary>
    public partial class ConutDownWindow : System.Windows.Window
    {
        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private int _totalCount = 0;
        private int _count = 0;

        /// <summary>
        /// Initializes a new instance of the ConutDownWindow class.
        /// </summary>
        public ConutDownWindow(int totalCount)
        {
            InitializeComponent();
            _totalCount = totalCount;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            countDownTextBox.Text = _totalCount.ToString();

            _dispatcherTimer.Tick -= dispatcherTimer_Tick;
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimer.Start();
        }
   
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _count++;
            countDownTextBox.Text = (_totalCount - _count).ToString();
            if (_count == _totalCount)
            {
                _dispatcherTimer.Stop();
            }
        }
    }
}