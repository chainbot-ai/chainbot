using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using Chainbot.Contracts.Config;
using System.Windows.Media;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class HelpPageViewModel : ViewModelBase
    {
        private IPathConfigService _pathConfigService;

        private List<string> _currentVersionUpdateLogList { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the HelpPageViewModel class.
        /// </summary>
        public HelpPageViewModel(IPathConfigService pathConfigService)
        {
            _pathConfigService = pathConfigService;

            Init();
        }

        //private DrawingImage Convert(string svgFile)
        //{
        //    WpfDrawingSettings  _wpfSettings = new WpfDrawingSettings();
        //    _wpfSettings.IncludeRuntime = false;
        //    _wpfSettings.TextAsGeometry = false;

        //    var converter = new DrawingSvgConverter(_wpfSettings);
        //    var iconStream = Application.GetResourceStream(new Uri($"{svgFile}", UriKind.Absolute)).Stream;
        //    DrawingGroup drawingGroup = converter.Read(iconStream);
        //    DrawingImage drawingImage = new DrawingImage(drawingGroup);

        //    return drawingImage;
        //}

        private void Init()
        {
            HelpItems.Add(new HelpItemViewModel() {
                //Icon = Convert("pack://application:,,,/Chainbot.Resources;Component/Image/Help/product-documentation.svg"),
                Icon = Application.Current.TryFindResource("DocumentationDrawingImage") as DrawingImage,
                Name = Chainbot.Resources.Properties.Resources.HelpPage_DocumentLable,
                Description = Chainbot.Resources.Properties.Resources.HelpPage_DocumentDescription,

                OpenAction = () => {
                    var path = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"Doc\ChainbotStudio-Help.chm");
                    Common.ShellExecute(path);
                },
            });

            HelpItems.Add(new HelpItemViewModel()
            {
                Icon = Application.Current.TryFindResource("HistoryVersionDrawingImage") as DrawingImage,
                Name = Chainbot.Resources.Properties.Resources.HelpPage_HistoryVersionLable,
                Description = Chainbot.Resources.Properties.Resources.HelpPage_HistoryVersionDescription,

                OpenAction = () => {
                    var path = GlobalConfig.CurrentLanguage == enLanguage.English ? _pathConfigService.HistoryVersionTxt : _pathConfigService.HistoryVersionTxt_Ch;
                    Common.ShellExecute(path);
                },
            });

            HelpItems.Add(new HelpItemViewModel()
            {
                Icon = Application.Current.TryFindResource("CommunityDrawingImage") as DrawingImage,
                Name = Chainbot.Resources.Properties.Resources.HelpPage_CommunityLable,
                Description = Chainbot.Resources.Properties.Resources.HelpPage_CommunityDescription,

                OpenAction = () =>
                {
                    
                },
            });
        }

        /// <summary>
        /// The <see cref="HelpItems" /> property's name.
        /// </summary>
        public const string HelpItemsPropertyName = "HelpItems";

        private ObservableCollection<HelpItemViewModel> _helpItemsProperty = new ObservableCollection<HelpItemViewModel>();

        /// <summary>
        /// Sets and gets the HelpItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<HelpItemViewModel> HelpItems
        {
            get
            {
                return _helpItemsProperty;
            }

            set
            {
                if (_helpItemsProperty == value)
                {
                    return;
                }

                _helpItemsProperty = value;
                RaisePropertyChanged(HelpItemsPropertyName);
            }
        }

    }
}