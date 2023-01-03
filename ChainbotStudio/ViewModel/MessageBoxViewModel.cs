using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MessageBoxViewModel : ViewModelBase
    {
        private Window _view;

        public bool? DialogResult { get; set; }
        /// <summary>
        /// Initializes a new instance of the MessageBoxViewModel class.
        /// </summary>
        public MessageBoxViewModel()
        {
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

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _titleProperty = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _titleProperty;
            }

            set
            {
                if (_titleProperty == value)
                {
                    return;
                }

                _titleProperty = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _textProperty = "";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return _textProperty;
            }

            set
            {
                if (_textProperty == value)
                {
                    return;
                }

                _textProperty = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowIcon" /> property's name.
        /// </summary>
        public const string IsShowIconPropertyName = "IsShowIcon";

        private bool _isShowIconProperty = true;

        /// <summary>
        /// Sets and gets the IsShowIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowIcon
        {
            get
            {
                return _isShowIconProperty;
            }

            set
            {
                if (_isShowIconProperty == value)
                {
                    return;
                }

                _isShowIconProperty = value;
                RaisePropertyChanged(IsShowIconPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowInformationIcon" /> property's name.
        /// </summary>
        public const string IsShowInformationIconPropertyName = "IsShowInformationIcon";

        private bool _isShowInformationIconProperty = false;

        /// <summary>
        /// Sets and gets the IsShowInformationIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowInformationIcon
        {
            get
            {
                return _isShowInformationIconProperty;
            }

            set
            {
                if (_isShowInformationIconProperty == value)
                {
                    return;
                }

                _isShowInformationIconProperty = value;
                RaisePropertyChanged(IsShowInformationIconPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowWarningIcon" /> property's name.
        /// </summary>
        public const string IsShowWarningIconPropertyName = "IsShowWarningIcon";

        private bool _isShowWarningIconProperty = false;

        /// <summary>
        /// Sets and gets the IsShowWarningIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowWarningIcon
        {
            get
            {
                return _isShowWarningIconProperty;
            }

            set
            {
                if (_isShowWarningIconProperty == value)
                {
                    return;
                }

                _isShowWarningIconProperty = value;
                RaisePropertyChanged(IsShowWarningIconPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowErrorIcon" /> property's name.
        /// </summary>
        public const string IsShowErrorIconPropertyName = "IsShowErrorIcon";

        private bool _isShowErrorIconProperty = false;

        /// <summary>
        /// Sets and gets the IsShowErrorIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowErrorIcon
        {
            get
            {
                return _isShowErrorIconProperty;
            }

            set
            {
                if (_isShowErrorIconProperty == value)
                {
                    return;
                }

                _isShowErrorIconProperty = value;
                RaisePropertyChanged(IsShowErrorIconPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsShowQuestionIcon" /> property's name.
        /// </summary>
        public const string IsShowQuestionIconPropertyName = "IsShowQuestionIcon";

        private bool _isShowQuestionIconProperty = false;

        /// <summary>
        /// Sets and gets the IsShowQuestionIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowQuestionIcon
        {
            get
            {
                return _isShowQuestionIconProperty;
            }

            set
            {
                if (_isShowQuestionIconProperty == value)
                {
                    return;
                }

                _isShowQuestionIconProperty = value;
                RaisePropertyChanged(IsShowQuestionIconPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsShowDefault" /> property's name.
        /// </summary>
        public const string IsShowDefaultPropertyName = "IsShowDefault";

        private bool _isShowDefaultProperty = false;

        /// <summary>
        /// Sets and gets the IsShowDefault property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowDefault
        {
            get
            {
                return _isShowDefaultProperty;
            }

            set
            {
                if (_isShowDefaultProperty == value)
                {
                    return;
                }

                _isShowDefaultProperty = value;
                RaisePropertyChanged(IsShowDefaultPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsShowYesNo" /> property's name.
        /// </summary>
        public const string IsShowYesNoPropertyName = "IsShowYesNo";

        private bool _isShowYesNoProperty = false;

        /// <summary>
        /// Sets and gets the IsShowYesNo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowYesNo
        {
            get
            {
                return _isShowYesNoProperty;
            }

            set
            {
                if (_isShowYesNoProperty == value)
                {
                    return;
                }

                _isShowYesNoProperty = value;
                RaisePropertyChanged(IsShowYesNoPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsShowYesNoCancel" /> property's name.
        /// </summary>
        public const string IsShowYesNoCancelPropertyName = "IsShowYesNoCancel";

        private bool _isShowYesNoCancelProperty = false;

        /// <summary>
        /// Sets and gets the IsShowYesNoCancel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsShowYesNoCancel
        {
            get
            {
                return _isShowYesNoCancelProperty;
            }

            set
            {
                if (_isShowYesNoCancelProperty == value)
                {
                    return;
                }

                _isShowYesNoCancelProperty = value;
                RaisePropertyChanged(IsShowYesNoCancelPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsYesDefault" /> property's name.
        /// </summary>
        public const string IsYesDefaultPropertyName = "IsYesDefault";

        private bool _isYesDefaultProperty = false;

        /// <summary>
        /// Sets and gets the IsYesDefault property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsYesDefault
        {
            get
            {
                return _isYesDefaultProperty;
            }

            set
            {
                if (_isYesDefaultProperty == value)
                {
                    return;
                }

                _isYesDefaultProperty = value;
                RaisePropertyChanged(IsYesDefaultPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="IsNoDefault" /> property's name.
        /// </summary>
        public const string IsNoDefaultPropertyName = "IsNoDefault";

        private bool _isNoDefaultProperty = false;

        /// <summary>
        /// Sets and gets the IsNoDefault property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNoDefault
        {
            get
            {
                return _isNoDefaultProperty;
            }

            set
            {
                if (_isNoDefaultProperty == value)
                {
                    return;
                }

                _isNoDefaultProperty = value;
                RaisePropertyChanged(IsNoDefaultPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsCancelDefault" /> property's name.
        /// </summary>
        public const string IsCancelDefaultPropertyName = "IsCancelDefault";

        private bool _isCancelDefaultProperty = false;

        /// <summary>
        /// Sets and gets the IsCancelDefault property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCancelDefault
        {
            get
            {
                return _isCancelDefaultProperty;
            }

            set
            {
                if (_isCancelDefaultProperty == value)
                {
                    return;
                }

                _isCancelDefaultProperty = value;
                RaisePropertyChanged(IsCancelDefaultPropertyName);
            }
        }





        private RelayCommand _yesCommand;

        /// <summary>
        /// Gets the YesCommand.
        /// </summary>
        public RelayCommand YesCommand
        {
            get
            {
                return _yesCommand
                    ?? (_yesCommand = new RelayCommand(
                    () =>
                    {
                        DialogResult = true;
                        this._view.Close();
                    }));
            }
        }



        private RelayCommand _noCommand;

        /// <summary>
        /// Gets the NoCommand.
        /// </summary>
        public RelayCommand NoCommand
        {
            get
            {
                return _noCommand
                    ?? (_noCommand = new RelayCommand(
                    () =>
                    {
                        DialogResult = false;
                        this._view.Close();
                    }));
            }
        }


        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        this._view.Close();
                    }));
            }
        }








    }
}