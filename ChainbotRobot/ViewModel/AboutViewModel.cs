using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ChainbotRobot.Contracts;
using System.ComponentModel;
using System.Windows;

namespace ChainbotRobot.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private IAboutInfoService _aboutInfoService;

        public Window m_view { get; set; }

        /// <summary>
        /// Initializes a new instance of the AboutViewModel class.
        /// </summary>
        public AboutViewModel(IAboutInfoService aboutInfoService)
        {
            _aboutInfoService = aboutInfoService;
        }

        public void LoadAboutInfo()
        {
            MachineName = _aboutInfoService.GetMachineName();
            MachineId = _aboutInfoService.GetMachineId();
            IpAddress = _aboutInfoService.GetIp();
            ProgramVersion = _aboutInfoService.GetVersion();
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
                        m_view = (Window)p.Source;
                    }));
            }
        }

        private RelayCommand _MouseLeftButtonDownCommand;

        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                return _MouseLeftButtonDownCommand
                    ?? (_MouseLeftButtonDownCommand = new RelayCommand(
                    () =>
                    {
                        m_view.DragMove();
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
                    e =>
                    {
                        e.Cancel = true;
                        m_view.Hide();
                    }));
            }
        }


        /// <summary>
        /// The <see cref="MachineName" /> property's name.
        /// </summary>
        public const string MachineNamePropertyName = "MachineName";

        private string _machineNameProperty = "";

        public string MachineName
        {
            get
            {
                return _machineNameProperty;
            }

            set
            {
                if (_machineNameProperty == value)
                {
                    return;
                }

                _machineNameProperty = value;
                RaisePropertyChanged(MachineNamePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="MachineId" /> property's name.
        /// </summary>
        public const string MachineIdPropertyName = "MachineId";

        private string _machineIdProperty = "";

        /// <summary>
        /// Sets and gets the MachineId property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MachineId
        {
            get
            {
                return _machineIdProperty;
            }

            set
            {
                if (_machineIdProperty == value)
                {
                    return;
                }

                _machineIdProperty = value;
                RaisePropertyChanged(MachineIdPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IpAddress" /> property's name.
        /// </summary>
        public const string IpAddressPropertyName = "IpAddress";

        private string _ipAddressProperty = "";


        public string IpAddress
        {
            get
            {
                return _ipAddressProperty;
            }

            set
            {
                if (_ipAddressProperty == value)
                {
                    return;
                }

                _ipAddressProperty = value;
                RaisePropertyChanged(IpAddressPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ProgramVersion" /> property's name.
        /// </summary>
        public const string ProgramVersionPropertyName = "ProgramVersion";

        private string _programVersionProperty = "";

        public string ProgramVersion
        {
            get
            {
                return _programVersionProperty;
            }

            set
            {
                if (_programVersionProperty == value)
                {
                    return;
                }

                _programVersionProperty = value;
                RaisePropertyChanged(ProgramVersionPropertyName);
            }
        }

    }

}