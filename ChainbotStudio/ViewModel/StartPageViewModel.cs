using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.UI;
using ChainbotStudio.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class StartPageViewModel : ViewModelBase
    {
        private IWindowService _windowService;

        private IServiceLocator _serviceLocator;
        private IRecentProjectsConfigService _recentProjectsConfigService;

        /// <summary>
        /// Initializes a new instance of the StartPageViewModel class.
        /// </summary>
        public StartPageViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _windowService = _serviceLocator.ResolveType<IWindowService>();
            _recentProjectsConfigService = _serviceLocator.ResolveType<IRecentProjectsConfigService>();

            reloadRecentProjects();

            _recentProjectsConfigService.ChangeEvent += _recentProjectsConfigService_ChangeEvent;
        }

        private void reloadRecentProjects()
        {
            var list = _recentProjectsConfigService.Load();
            UpdateRecentUsedProjects(list);
        }

        private void _recentProjectsConfigService_ChangeEvent(object sender, EventArgs e)
        {
            reloadRecentProjects();
        }

        public void UpdateRecentUsedProjects(List<object> list)
        {
            RecentUsedProjects.Clear();

            foreach (dynamic item in list)
            {
                var name = item.Name;
                var description = item.Description;
                var filePath = item.FilePath;

                var itemVM = _serviceLocator.ResolveType<RecentUsedProjectItemViewModel>();
                itemVM.ProjectOrder = list.IndexOf(item) + 1;
                itemVM.ProjectName = name;
                itemVM.ProjectConfigFilePath = filePath;
                itemVM.ProjectDescription = description;
                itemVM.ProjectToolTip = string.Format(Chainbot.Resources.Properties.Resources.StartPage_ProcessToolTip, description, filePath);

                itemVM.ProjectHeader = $"{itemVM.ProjectOrder}  {itemVM.ProjectName}";
                RecentUsedProjects.Add(itemVM);

            }
        }


        private RelayCommand _newProjectCommand;

        /// <summary>
        /// Gets the NewProjectCommand.
        /// </summary>
        public RelayCommand NewProjectCommand
        {
            get
            {
                return _newProjectCommand
                    ?? (_newProjectCommand = new RelayCommand(
                    () =>
                    {
                        var window = new NewProjectWindow();
                        var vm = window.DataContext as NewProjectViewModel;
                        vm.Window = window;
                        _windowService.ShowDialog(window);
                    }));
            }
        }


        /// <summary>
        /// The <see cref="RecentUsedProjects" /> property's name.
        /// </summary>
        public const string RecentUsedProjectsPropertyName = "RecentUsedProjects";

        private ObservableCollection<RecentUsedProjectItemViewModel> _recentUsedProjectsProperty = new ObservableCollection<RecentUsedProjectItemViewModel>();

        /// <summary>
        /// Sets and gets the RecentUsedProjects property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<RecentUsedProjectItemViewModel> RecentUsedProjects
        {
            get
            {
                return _recentUsedProjectsProperty;
            }

            set
            {
                if (_recentUsedProjectsProperty == value)
                {
                    return;
                }

                _recentUsedProjectsProperty = value;
                RaisePropertyChanged(RecentUsedProjectsPropertyName);
            }
        }








    }
}