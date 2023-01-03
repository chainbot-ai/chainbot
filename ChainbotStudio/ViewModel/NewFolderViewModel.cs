using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NewFolderViewModel : ViewModelBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window _view;

        public string Path { get; internal set; }


        private ProjectViewModel _projectViewModel;


        /// <summary>
        /// Initializes a new instance of the NewFolderViewModel class.
        /// </summary>
        public NewFolderViewModel(ProjectViewModel projectViewModel)
        {
            _projectViewModel = projectViewModel;
        }


        private RelayCommand<RoutedEventArgs> _loadedCommand;

        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _view = (Window)p.Source;
                    }));
            }
        }


        private RelayCommand<RoutedEventArgs> _folderNameLoadedCommand;

        public RelayCommand<RoutedEventArgs> FolderNameLoadedCommand
        {
            get
            {
                return _folderNameLoadedCommand
                    ?? (_folderNameLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        var textBox = (TextBox)p.Source;
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }



        /// <summary>
        /// The <see cref="IsFolderNameCorrect" /> property's name.
        /// </summary>
        public const string IsFolderNameCorrectPropertyName = "IsFolderNameCorrect";

        private bool _isFolderNameCorrectProperty = false;


        public bool IsFolderNameCorrect
        {
            get
            {
                return _isFolderNameCorrectProperty;
            }

            set
            {
                if (_isFolderNameCorrectProperty == value)
                {
                    return;
                }

                _isFolderNameCorrectProperty = value;
                RaisePropertyChanged(IsFolderNameCorrectPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="FolderNameValidatedWrongTip" /> property's name.
        /// </summary>
        public const string FolderNameValidatedWrongTipPropertyName = "FolderNameValidatedWrongTip";

        private string _folderNameValidatedWrongTipProperty = "";

        public string FolderNameValidatedWrongTip
        {
            get
            {
                return _folderNameValidatedWrongTipProperty;
            }

            set
            {
                if (_folderNameValidatedWrongTipProperty == value)
                {
                    return;
                }

                _folderNameValidatedWrongTipProperty = value;
                RaisePropertyChanged(FolderNameValidatedWrongTipPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="FolderName" /> property's name.
        /// </summary>
        public const string FolderNamePropertyName = "FolderName";

        private string _folderNameProperty = "";

        public string FolderName
        {
            get
            {
                return _folderNameProperty;
            }

            set
            {
                if (_folderNameProperty == value)
                {
                    return;
                }

                _folderNameProperty = value;
                RaisePropertyChanged(FolderNamePropertyName);

                folderNameValidate(value);
            }
        }

        private void folderNameValidate(string value)
        {
            IsFolderNameCorrect = true;

            if (string.IsNullOrEmpty(value))
            {
                IsFolderNameCorrect = false;
                FolderNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong1;
            }
            else
            {
                if (value.Contains(@"\") || value.Contains(@"/"))
                {
                    IsFolderNameCorrect = false;
                    FolderNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                }
                else
                {
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(value);
                    }
                    catch (ArgumentException) { }
                    catch (System.IO.PathTooLongException) { }
                    catch (NotSupportedException) { }
                    if (ReferenceEquals(fi, null))
                    {
                        // file name is not valid
                        IsFolderNameCorrect = false;
                        FolderNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                    }
                    else
                    {
                        // file name is valid... May check for existence by calling fi.Exists.
                    }
                }
            }

            if (Directory.Exists(Path + @"\" + FolderName))
            {
                IsFolderNameCorrect = false;
                FolderNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewFolder_NameValidateWrong;
            }
        }

        private RelayCommand _okCommand;

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        Directory.CreateDirectory(Path + @"\" + FolderName);
                        _projectViewModel.Refresh();

                        _view.Close();
                    },
                    () => IsFolderNameCorrect));
            }
        }



        private RelayCommand _cancelCommand;

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        _view.Close();
                    },
                    () => true));
            }
        }






    }
}