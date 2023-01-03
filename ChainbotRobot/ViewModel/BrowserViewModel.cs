using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Diagnostics;
using System.IO;

namespace ChainbotRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class BrowserViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the BrowserViewModel class.
        /// </summary>
        public BrowserViewModel()
        {
        }

        private RelayCommand _firefoxConfirmCommand;

        public RelayCommand FirefoxConfirmCommand
        {
            get
            {
                return _firefoxConfirmCommand
                    ?? (_firefoxConfirmCommand = new RelayCommand(
                    () =>
                    {
                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string text = Path.Combine(baseDirectory, "Chainbot.MsgHostInstall.exe");
                        Process firefox = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = text,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                Arguments = "\"" + "firefox" + "\""
                            }
                        };
                        firefox.Start();
                        firefox.WaitForExit();

                        if (firefox.ExitCode == 0)
                        {
                            System.Windows.MessageBox.Show("Firefox browser plug-in has been successfully installed. Please click Add (rpa_msghost) extension in the opened browser!", "Prompt", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Firefox browser plug-in installation failed!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                    }));
            }
        }

        private RelayCommand _chromeConfirmCommand;

        public RelayCommand ChromeConfirmCommand
        {
            get
            {
                return _chromeConfirmCommand
                    ?? (_chromeConfirmCommand = new RelayCommand(
                    () =>
                    {
                        if ((System.Windows.MessageBox.Show("Chrome extension is not installed or disabled, you need to close Chrome browser before installing! Click \"Yes\" to install.", "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes))
                        {
                            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            string text = Path.Combine(baseDirectory, "Chainbot.MsgHostInstall.exe");
                            Process chrome = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = text,
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    Arguments = "\"" + "chrome" + "\""
                                }
                            };
                            chrome.Start();
                            chrome.WaitForExit();

                            if (chrome.ExitCode == 0)
                            {
                                System.Windows.MessageBox.Show("Chrome browser plug-in has been successfully installed. Please open the browser to enable (com. rpa. msghost) extension!", "Prompt", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Chrome browser plug-in installation failed!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            }
                        }
                    }));
            }
        }
    }
}