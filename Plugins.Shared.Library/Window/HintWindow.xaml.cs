using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Plugins.Shared.Library.Window
{
    public partial class HintWindow : System.Windows.Window
    {
        private bool isTop = true;

        private double rate = 1;
        private Rect desktopWorkingArea;

        [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps", SetLastError = true)]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        enum DeviceCap
        {
            VERTRES = 10,
            PHYSICALWIDTH = 110,
            SCALINGFACTORX = 114,
            DESKTOPVERTRES = 117,
        }

        public HintWindow()
        {
            InitializeComponent();

            var g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            desktopWorkingArea = SystemParameters.WorkArea;
            var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
            rate = physicalScreenHeight / desktopWorkingArea.Height;
        }

        private void HintWindowPos( System.Drawing.Point mousePos)
        {
            int x = (int)(mousePos.X /rate);
            int y = (int)(mousePos.Y /rate);

            if (isTop && x - 5 < this.Width && y - 5 < this.Height)
            {
                isTop = false;                
            }
            else if(!isTop  &&  (x > (desktopWorkingArea.Width / 2 ) || y > (desktopWorkingArea.Height / 2)))
            {
                isTop = true;
            }
        }

        public void ShowInfo()
        {
            var screenPoint = System.Windows.Forms.Cursor.Position;
            var desktopWorkingArea = SystemParameters.WorkArea;
            int spacex = 5;
            int spacey = 5;

            string mousepos = "X:" + screenPoint.X + "  Y:" + screenPoint.Y;
            mousebox.Text = mousepos;
            HintWindowPos(screenPoint);
            if (isTop)
            {
                this.Left = spacex;
                this.Top = spacey;
            }else
            {
                this.Left = desktopWorkingArea.Right - this.Width - spacex;
                this.Top = desktopWorkingArea.Bottom - this.Height - spacey;
            }

            int magnification = 2;
            const int imgWidth = 500;
            const int imgHeight = 500;
      
            Bitmap bt = new Bitmap(imgWidth / magnification, imgHeight / magnification);
            Graphics g = Graphics.FromImage(bt);
            g.CopyFromScreen(
                     new System.Drawing.Point(screenPoint.X - imgWidth / (2 * magnification),
                               screenPoint.Y - imgHeight / (2 * magnification)),
                     new System.Drawing.Point(0, 0),
                     new System.Drawing.Size(imgWidth / magnification, imgHeight / magnification));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            imgDynamic.Source = ToBitmapSource(bt);
        }

        private BitmapSource ToBitmapSource(Bitmap image)
        {
            IntPtr ptr = image.GetHbitmap();
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
            (
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            DeleteObject(ptr);
            return bs;
        }

        public void Rest()
        {     
            this.Left = 5;
            this.Top = 5;
            isTop = true;

            string mousepos = "X:0  Y:0";
            mousebox.Text = mousepos;
            imgDynamic.Source = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 5;
            this.Top = 5;
        }
    }
}
