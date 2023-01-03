using GalaSoft.MvvmLight;

namespace ChainbotRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ScheduledPackageItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ScheduledPackageItemViewModel class.
        /// </summary>
        public ScheduledPackageItemViewModel()
        {
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _nameProperty = "";

        public string Name
        {
            get
            {
                return _nameProperty;
            }

            set
            {
                if (_nameProperty == value)
                {
                    return;
                }

                _nameProperty = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Version" /> property's name.
        /// </summary>
        public const string VersionPropertyName = "Version";

        private string _versionProperty = "";


        public string Version
        {
            get
            {
                return _versionProperty;
            }

            set
            {
                if (_versionProperty == value)
                {
                    return;
                }

                _versionProperty = value;
                RaisePropertyChanged(VersionPropertyName);
            }
        }


    }
}