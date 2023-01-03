using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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
    public class RenameViewModel : ViewModelBase
    {
        private ProjectViewModel _projectViewModel;
        private DocksViewModel _docksViewModel;

        /// <summary>
        /// Initializes a new instance of the RenameViewModel class.
        /// </summary>
        public RenameViewModel(ProjectViewModel projectViewModel, DocksViewModel docksViewModel)
        {
            _projectViewModel = projectViewModel;
            _docksViewModel = docksViewModel;
        }


        private Window _view;

        public string Path { get; set; }

        public string NewPath { get; set; }

        public bool IsDirectory { get; internal set; }

        public string Dir { get; internal set; }

        public bool IsMain { get; internal set; }

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



        private RelayCommand<RoutedEventArgs> _dstNameLoadedCommand;

        public RelayCommand<RoutedEventArgs> DstNameLoadedCommand
        {
            get
            {
                return _dstNameLoadedCommand
                    ?? (_dstNameLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        var textBox = (TextBox)p.Source;
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }




        /// <summary>
        /// The <see cref="SrcName" /> property's name.
        /// </summary>
        public const string SrcNamePropertyName = "SrcName";

        private string _srcNameProperty = "";

        public string SrcName
        {
            get
            {
                return _srcNameProperty;
            }

            set
            {
                if (_srcNameProperty == value)
                {
                    return;
                }

                _srcNameProperty = value;
                RaisePropertyChanged(SrcNamePropertyName);
            }
        }




        /// <summary>
        /// The <see cref="IsDstNameCorrect" /> property's name.
        /// </summary>
        public const string IsDstNameCorrectPropertyName = "IsDstNameCorrect";

        private bool _isDstNameCorrectProperty = false;

        public bool IsDstNameCorrect
        {
            get
            {
                return _isDstNameCorrectProperty;
            }

            set
            {
                if (_isDstNameCorrectProperty == value)
                {
                    return;
                }

                _isDstNameCorrectProperty = value;
                RaisePropertyChanged(IsDstNameCorrectPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="DstNameValidatedWrongTip" /> property's name.
        /// </summary>
        public const string DstNameValidatedWrongTipPropertyName = "DstNameValidatedWrongTip";

        private string _dstNameValidatedWrongTipProperty = "";

        public string DstNameValidatedWrongTip
        {
            get
            {
                return _dstNameValidatedWrongTipProperty;
            }

            set
            {
                if (_dstNameValidatedWrongTipProperty == value)
                {
                    return;
                }

                _dstNameValidatedWrongTipProperty = value;
                RaisePropertyChanged(DstNameValidatedWrongTipPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="DstName" /> property's name.
        /// </summary>
        public const string DstNamePropertyName = "DstName";

        private string _dstNameProperty = "";

        public string DstName
        {
            get
            {
                return _dstNameProperty;
            }

            set
            {
                if (_dstNameProperty == value)
                {
                    return;
                }

                _dstNameProperty = value;
                RaisePropertyChanged(DstNamePropertyName);

                dstNameValidate(value);
            }
        }

        private void dstNameValidate(string value)
        {
            IsDstNameCorrect = true;

            if (string.IsNullOrEmpty(value))
            {
                IsDstNameCorrect = false;
                DstNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong1;
            }
            else
            {
                if (value.Contains(@"\") || value.Contains(@"/"))
                {
                    IsDstNameCorrect = false;
                    DstNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
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
                        IsDstNameCorrect = false;
                        DstNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewProject_NameValidateWrong2;
                    }
                    else
                    {
                        // file name is valid... May check for existence by calling fi.Exists.
                    }
                }
            }

            var dstFullPath = Dir + @"\" + DstName;
            if (Directory.Exists(dstFullPath))
            {
                IsDstNameCorrect = false;
                DstNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.NewFolder_NameValidateWrong;
            }
            else if (File.Exists(dstFullPath))
            {
                IsDstNameCorrect = false;
                DstNameValidatedWrongTip = Chainbot.Resources.Properties.Resources.Rename_NameValidateWrong;
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
                        if (DstName != SrcName)
                        {
                            if (IsDirectory)
                            {
                                NewPath = Dir + @"\" + DstName;
                                Directory.Move(Dir + @"\" + SrcName, NewPath);
                            }
                            else
                            {
                                NewPath = Dir + @"\" + DstName;
                                File.Move(Dir + @"\" + SrcName, NewPath);
                            }

                            _projectViewModel.OnRename(this);
                            _docksViewModel.OnRename(this);
                        }

                        _view.Close();
                    },
                    () => IsDstNameCorrect));
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
                    }));
            }
        }


    }
}