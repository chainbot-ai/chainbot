using Chainbot.Contracts.Activities;
using Chainbot.Contracts.UI;
using ChainbotStudio.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.ComponentModel;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class RecordingViewModel : ViewModelBase
    {
        private Window _view;

        private IWindowService _windowService;

        private IMessageBoxService _messageBoxService;

        private IRecordingServiceProxy _recordingServiceProxy;

        private DocksViewModel _docksViewModel;

        /// <summary>
        /// Initializes a new instance of the RecordingViewModel class.
        /// </summary>
        public RecordingViewModel(IWindowService windowService,IMessageBoxService messageBoxService
            , IRecordingServiceProxy recordingServiceProxy,DocksViewModel docksViewModel )
        {
            _windowService = windowService;
            _messageBoxService = messageBoxService;
            _recordingServiceProxy = recordingServiceProxy;
            _docksViewModel = docksViewModel;

            _recordingServiceProxy.BeginEvent += _recordingServiceProxy_BeginEvent;
            _recordingServiceProxy.EndEvent += _recordingServiceProxy_EndEvent;

            _recordingServiceProxy.RecordEvent += _recordingServiceProxy_RecordEvent;
            _recordingServiceProxy.SaveEvent += _recordingServiceProxy_SaveEvent;
        }

        

        private void _recordingServiceProxy_BeginEvent(object sender, EventArgs e)
        {
            _view.WindowState = WindowState.Minimized;
        }

        private void _recordingServiceProxy_EndEvent(object sender, EventArgs e)
        {
            _view.WindowState = WindowState.Normal;
            _view.Topmost = true;
        }

        private void _recordingServiceProxy_RecordEvent(object sender, EventArgs e)
        {
            IsRecorded = true;
        }

        private void _recordingServiceProxy_SaveEvent(object sender, EventArgs e)
        {
            IsRecorded = false;
            _view.Close();
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
                        _view.Topmost = true;

                        _recordingServiceProxy.IsRecordingWindowOpened = true;
                    }));
            }
        }


        private RelayCommand<CancelEventArgs> _closingCommand;

        public RelayCommand<CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<CancelEventArgs>(
                    p =>
                    {
                        bool bContinueClose = true;

                        if (IsRecorded)
                        {
                            _view.Topmost = false;

                            var ret = _messageBoxService.ShowQuestion(Chainbot.Resources.Properties.Resources.RecordView_QuestionMessage);
                            if (!ret)
                            {
                                bContinueClose = false;
                                _view.Topmost = true;
                            }
                        }

                        if (!bContinueClose)
                        {
                            p.Cancel = true;
                        }
                    }));
            }
        }

        private RelayCommand _unloadedCommand;

        public RelayCommand UnloadedCommand
        {
            get
            {
                return _unloadedCommand
                    ?? (_unloadedCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.IsRecordingWindowOpened = false;
                    }));
            }
        }



        /// <summary>
        /// The <see cref="IsRecorded" /> property's name.
        /// </summary>
        public const string IsRecordedPropertyName = "IsRecorded";

        private bool _isRecordedProperty = false;

        public bool IsRecorded
        {
            get
            {
                return _isRecordedProperty;
            }

            set
            {
                if (_isRecordedProperty == value)
                {
                    return;
                }

                _isRecordedProperty = value;
                RaisePropertyChanged(IsRecordedPropertyName);
            }
        }



        

        private RelayCommand _mouseLeftClickCommand;


        public RelayCommand MouseLeftClickCommand
        {
            get
            {
                return _mouseLeftClickCommand
                    ?? (_mouseLeftClickCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.MouseLeftClick();
                    }));
            }
        }


        private RelayCommand _mouseRightClickCommand;

        public RelayCommand MouseRightClickCommand
        {
            get
            {
                return _mouseRightClickCommand
                    ?? (_mouseRightClickCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.MouseRightClick();
                    }));
            }
        }


        private RelayCommand _mouseDoubleLeftClickCommand;

        public RelayCommand MouseDoubleLeftClickCommand
        {
            get
            {
                return _mouseDoubleLeftClickCommand
                    ?? (_mouseDoubleLeftClickCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.MouseDoubleLeftClick();
                    }));
            }
        }



        private RelayCommand _mouseHoverCommand;

        public RelayCommand MouseHoverCommand
        {
            get
            {
                return _mouseHoverCommand
                    ?? (_mouseHoverCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.MouseHover();
                    }));
            }
        }


        private RelayCommand _keyboardInputCommand;

        public RelayCommand KeyboardInputCommand
        {
            get
            {
                return _keyboardInputCommand
                    ?? (_keyboardInputCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.KeyboardInput();
                    }));
            }
        }



        private RelayCommand _keyboardHotKeyCommand;

        public RelayCommand KeyboardHotKeyCommand
        {
            get
            {
                return _keyboardHotKeyCommand
                    ?? (_keyboardHotKeyCommand = new RelayCommand(
                    () =>
                    {
                        _recordingServiceProxy.KeyboardHotKey();
                    }));
            }
        }



        private RelayCommand _saveAndExitCommand;

        public RelayCommand SaveAndExitCommand
        {
            get
            {
                return _saveAndExitCommand
                    ?? (_saveAndExitCommand = new RelayCommand(
                    () =>
                    {
                        if (_docksViewModel.SelectedDocument is DesignerDocumentViewModel)
                        {
                            _recordingServiceProxy.Save(_docksViewModel.SelectedDocument.Path);
                        }
                    }));
            }
        }
    }
}