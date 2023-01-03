using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using Chainbot.Contracts.UI;
using Plugins.Shared.Library;
using System.Diagnostics;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ToolsPageViewModel : ViewModelBase
    {
        private IMessageBoxService _messageBoxService;

        /// <summary>
        /// Initializes a new instance of the ToolsPageViewModel class.
        /// </summary>
        public ToolsPageViewModel(IMessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
        }

        private RelayCommand _installChromePluginCommand;

        /// <summary>
        /// Gets the InstallChromePluginCommand.
        /// </summary>
        public RelayCommand InstallChromePluginCommand
        {
            get
            {
                return _installChromePluginCommand
                    ?? (_installChromePluginCommand = new RelayCommand(
                    () =>
                    {      
                        if (_messageBoxService.ShowWarningYesNo(Chainbot.Resources.Properties.Resources.ToolsPage_ChromeInstallQuestion))
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
                                _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.ToolsPage_ChromeInstallSuccess);
                            }
                            else
                            {
                                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.ToolsPage_ChromeInstallError);
                            }
                        }
                    }));
            }
        }

        private RelayCommand _installFirefoxPluginCommand;

        /// <summary>
        /// Gets the InstallFirefoxPluginCommand.
        /// </summary>
        public RelayCommand InstallFirefoxPluginCommand
        {
            get
            {
                return _installFirefoxPluginCommand
                    ?? (_installFirefoxPluginCommand = new RelayCommand(
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
                            _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.ToolsPage_FirefoxInstallSuccess);
                        }
                        else
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.ToolsPage_FirefoxInstallError);
                        }
                    }));
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        private void UnlockServerManager(bool InstallFlg)
        {
            IntPtr intPtr = (IntPtr)0;
            Wow64DisableWow64FsRedirection(ref intPtr);

            string installerFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "ChainbotCredentialProvider.dll");
            string credentialRegPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{5CB652CA-4B73-4D42-96E3-CC546B427FD8}";

            if (InstallFlg)
            {
                try
                {                     
                    RegistryKey registryKey_Local;
                    RegistryKey registryKey_Classes;
                    string installerName;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        installerName = "ChainbotCredentialProvider_x64.dll";
                        registryKey_Local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        registryKey_Classes = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
                    }
                    else
                    {
                        installerName = "ChainbotCredentialProvider.dll";
                        registryKey_Local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                        registryKey_Classes = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
                    }

                    File.Copy(Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "UnlockScreen", installerName), installerFile, true);

                    RegistryKey registryKey_Credential = registryKey_Local.OpenSubKey(credentialRegPath, true);
                    if (registryKey_Credential == null)
                    {
                        registryKey_Credential = registryKey_Local.CreateSubKey(credentialRegPath);
                    }
                    registryKey_Credential.SetValue("", "ChainbotCredentialProvider");
                    registryKey_Credential.Close();
                    registryKey_Local.Close();
                    string clsId = @"CLSID\{5CB652CA-4B73-4D42-96E3-CC546B427FD8}";
                    RegistryKey registryKey_ClsId = registryKey_Classes.OpenSubKey(clsId);
                    if (registryKey_ClsId == null)
                    {
                        registryKey_ClsId = registryKey_Classes.CreateSubKey(clsId);
                        registryKey_ClsId.SetValue("", "ChainbotCredentialProvider");
                        RegistryKey registryKey_Inproc = registryKey_ClsId.CreateSubKey("InprocServer32");
                        registryKey_Inproc.SetValue("", "ChainbotCredentialProvider.dll");
                        registryKey_Inproc.SetValue("ThreadingModel", "Apartment");
                        registryKey_Inproc.Close();
                    }
                    registryKey_ClsId.Close();
                    registryKey_Classes.Close();
                    _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.ToolsPage_UnlockInstallSuccess);
                }
                catch (Exception ex)
                {
                    _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.ToolsPage_UnlockInstallError + ex.Message);
                }
            }
            else
            {
                try
                {
                    if (File.Exists(installerFile))
                    {
                        File.Delete(installerFile);
                    }

                    string unlockPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), @"ChainbotStudio\Unlock");
                    if (Directory.Exists(unlockPath))
                    {
                        Directory.Delete(unlockPath, true);
                    }

                    RegistryKey registryKey_Local;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        registryKey_Local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    }
                    else
                    {
                        registryKey_Local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    }
                    RegistryKey registryKey_Credential = registryKey_Local.OpenSubKey(credentialRegPath, true);
                    if (registryKey_Credential != null)
                    {
                        registryKey_Local.DeleteSubKeyTree(credentialRegPath);
                        registryKey_Credential.Close();
                    }
                    registryKey_Local.Close();
                    _messageBoxService.ShowInformation(Chainbot.Resources.Properties.Resources.ToolsPage_UnlockUninstallSuccess);
                }
                catch (Exception ex)
                {
                    _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.ToolsPage_UnlockUninstallError + ex.Message);
                }
            }
        }

        private RelayCommand _installUnlockServerCommand;

        /// <summary>
        /// Gets the InstallUnlockServerCommand.
        /// </summary>
        public RelayCommand InstallUnlockServerCommand
        {
            get
            {
                return _installUnlockServerCommand
                    ?? (_installUnlockServerCommand = new RelayCommand(
                    () =>
                    {
                        UnlockServerManager(true);
                    }));
            }
        }

        private RelayCommand _uninstallUnlockServerCommand;

        /// <summary>
        /// Gets the UninstallUnlockServerCommand.
        /// </summary>
        public RelayCommand UninstallUnlockServerCommand
        {
            get
            {
                return _uninstallUnlockServerCommand
                    ?? (_uninstallUnlockServerCommand = new RelayCommand(
                    () =>
                    {
                        UnlockServerManager(false);
                    }));
            }
        }
    }
}