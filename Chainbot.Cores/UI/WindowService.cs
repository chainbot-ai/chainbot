using Chainbot.Contracts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Cores.UI
{
    public class WindowService : IWindowService
    {
        public bool IsMinimized
        {
            get
            {
                return Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized;
            }
        }

        public bool IsMaximized
        {
            get
            {
                return Application.Current.MainWindow.WindowState == System.Windows.WindowState.Maximized;
            }
        }

        public bool IsNormal
        {
            get
            {
                return Application.Current.MainWindow.WindowState == System.Windows.WindowState.Normal;
            }
        }

        public bool? ShowDialog(Window window, bool bOwner = true)
        {
            if(bOwner)
            {
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            
            return window.ShowDialog();
        }

        public void Show(Window window,bool bOwner = true)
        {
            if (bOwner)
            {
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            window.Show();
        }

        public void ShowMainWindowMaximized()
        {
            Application.Current.Dispatcher.InvokeAsync(()=> {
                Application.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
            });
        }

        public void ShowMainWindowMinimized()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
            });
        }

        public void ShowMainWindowNormal(bool bCheckMinimized = true)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (bCheckMinimized && IsMinimized)
                {
                    Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                }

                Application.Current.MainWindow.Activate();
            });
        }
    }
}
