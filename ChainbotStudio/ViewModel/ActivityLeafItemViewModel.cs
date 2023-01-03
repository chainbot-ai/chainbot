using Chainbot.Contracts.UI;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ActivityLeafItemViewModel : ActivityBaseItemViewModel
    {
        private IContextMenuService _contextMenuService;

        private ActivitiesViewModel _activitiesViewModel;

        private DocksViewModel _docksViewModel;

        public bool IsInFavorites { get; set; }

        public string TypeOf { get; set; }

        public string AssemblyQualifiedName { get; set; }

        /// <summary>
        /// Initializes a new instance of the ActivityItemViewModel class.
        /// </summary>
        public ActivityLeafItemViewModel(IContextMenuService contextMenuService, ActivitiesViewModel activitiesViewModel, DocksViewModel docksViewModel)
        {
            _contextMenuService = contextMenuService;
            _activitiesViewModel = activitiesViewModel;
            _docksViewModel = docksViewModel;
        }


        protected override string GetClassName()
        {
            var fullClassName = TypeOf.Split(',').First();

            var className = fullClassName.Split('.').Last();

            return className;
        }

        /// <summary>
        /// The <see cref="IconSource" /> property's name.
        /// </summary>
        public const string IconSourcePropertyName = "IconSource";

        private ImageSource _iconSourceProperty = null;

        /// <summary>
        /// Sets and gets the IconSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ImageSource IconSource
        {
            get
            {
                return _iconSourceProperty;
            }

            set
            {
                if (_iconSourceProperty == value)
                {
                    return;
                }

                _iconSourceProperty = value;
                RaisePropertyChanged(IconSourcePropertyName);
            }
        }



        

        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseRightButtonUpCommand;

        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseRightButtonUpCommand
        {
            get
            {
                return _treeNodeMouseRightButtonUpCommand
                    ?? (_treeNodeMouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if(IsInFavorites)
                        {
                            _contextMenuService.Show(this, "RemoveFromFavoritesContextMenu");
                        }
                        else
                        {
                            _contextMenuService.Show(this, "AddToFavoritesContextMenu");
                        }
                        
                    }));
            }
        }


        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseDoubleClickCommand;

        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseDoubleClickCommand
        {
            get
            {
                return _treeNodeMouseDoubleClickCommand
                    ?? (_treeNodeMouseDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if (_docksViewModel.SelectedDocument is DesignerDocumentViewModel)
                        {
                            var designerDoc = _docksViewModel.SelectedDocument as DesignerDocumentViewModel;
                            designerDoc.InsertActivity(Name,AssemblyQualifiedName);
                        }
                    }));
            }
        }



        private RelayCommand _addToFavoritesCommand;


        public RelayCommand AddToFavoritesCommand
        {
            get
            {
                return _addToFavoritesCommand
                    ?? (_addToFavoritesCommand = new RelayCommand(
                    () =>
                    {
                        _activitiesViewModel.AddToFavorites(this);
                    }));
            }
        }




        private RelayCommand _removeFromFavoritesCommand;

        public RelayCommand RemoveFromFavoritesCommand
        {
            get
            {
                return _removeFromFavoritesCommand
                    ?? (_removeFromFavoritesCommand = new RelayCommand(
                    () =>
                    {
                        _activitiesViewModel.RemoveFromFavorites(this);
                    }));
            }
        }





    }
}