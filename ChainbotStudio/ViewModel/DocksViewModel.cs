using ActiproSoftware.Windows.Controls.Docking;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using Chainbot.Cores.Classes;
using ChainbotStudio.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocksViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;

        private IWorkflowStateService _workflowStateService;
        private ICommonService _commonService;
        private IProjectManagerService _projectManagerService;
        private IDispatcherService _dispatcherService;

        public event EventHandler DocumentSelectChangeEvent;

        public DocksView View { get; set; }

        /// <summary>
        /// Initializes a new instance of the DocksViewModel class.
        /// </summary>
        public DocksViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();

            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();

            _projectManagerService.ProjectPreviewCloseEvent += _projectManagerService_ProjectPreviewCloseEvent;
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
                        View = (DocksView)p.Source;
                    }));
            }
        }


        private RelayCommand<DockingWindowsEventArgs> _windowsClosingCommandCommand;

        /// <summary>
        /// Gets the WindowClosingCommand.
        /// </summary>
        public RelayCommand<DockingWindowsEventArgs> WindowsClosingCommand
        {
            get
            {
                return _windowsClosingCommandCommand
                    ?? (_windowsClosingCommandCommand = new RelayCommand<DockingWindowsEventArgs>(
                    p =>
                    {
                        foreach (var dockingWindow in p.Windows)
                        {
                            var documentViewModel = dockingWindow.DataContext as DocumentViewModel;
                            if(documentViewModel != null)
                            {
                                bool isClosed = documentViewModel.CloseQuery();
                                if (!isClosed)
                                {
                                    p.Cancel = true;
                                }
                            }
                        }
                    }));
            }
        }



        /// <summary>
        /// The <see cref="IsAppDomainViewsVisible" /> property's name.
        /// </summary>
        public const string IsAppDomainViewsVisiblePropertyName = "IsAppDomainViewsVisible";

        private bool _isAppDomainViewsVisibleProperty = true;

        /// <summary>
        /// Sets and gets the IsAppDomainViewsVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAppDomainViewsVisible
        {
            get
            {
                return _isAppDomainViewsVisibleProperty;
            }

            set
            {
                if (_isAppDomainViewsVisibleProperty == value)
                {
                    return;
                }

                _isAppDomainViewsVisibleProperty = value;
                RaisePropertyChanged(IsAppDomainViewsVisiblePropertyName);
            }
        }





        private void _projectManagerService_ProjectPreviewCloseEvent(object sender, CancelEventArgs e)
        {
            e.Cancel = !CloseAllQuery();
        }


        public void RaiseDocumentSelectChangeEvent(DocumentViewModel documentViewModel)
        {
            DocumentSelectChangeEvent?.Invoke(documentViewModel, EventArgs.Empty);
        }



        public DocumentViewModel SelectedDocument
        {
            get
            {
                foreach (var doc in Documents)
                {
                    if (doc.IsSelected)
                    {
                        return doc;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The <see cref="Documents" /> property's name.
        /// </summary>
        public const string DocumentsPropertyName = "Documents";

        private ObservableCollection<DocumentViewModel> _documentsProperty = new ObservableCollection<DocumentViewModel>();

        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                return _documentsProperty;
            }

            set
            {
                if (_documentsProperty == value)
                {
                    return;
                }

                _documentsProperty = value;
                RaisePropertyChanged(DocumentsPropertyName);
            }
        }

        public DocumentViewModel NewDesignerDocument(string path)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            var doc = _serviceLocator.ResolveType<DesignerDocumentViewModel>();
            doc.Path = path;
            doc.RelativePath = _commonService.MakeRelativePath(_projectManagerService.CurrentProjectPath, path);
            doc.Title = name;
            doc.IsShowIcon = false;
            doc.ToolTip = path;

            doc.MakeView();

            if (_workflowStateService.IsRunningOrDebugging)
            {
                doc.IsReadOnly = true;
            }

            Documents.Add(doc);

            doc.IsSelected = true;


            return doc;
        }


        public bool CloseAllQuery()
        {
            var docList = Documents.ToList();
            foreach (var doc in docList)
            {
                if (!doc.CloseQuery())
                {
                    return false;
                }
            }

            return true;
        }

        public void OnDeleteFile(string path)
        {
            var docList = Documents.ToList();
            foreach (var doc in docList)
            {
                if (!System.IO.File.Exists(doc.Path))
                {
                    Documents.Remove(doc);
                }
            }
        }


        public void OnDeleteDir(string path)
        {
            var docList = Documents.ToList();
            foreach (var doc in docList)
            {
                if (!System.IO.File.Exists(doc.Path))
                {
                    Documents.Remove(doc);
                }
            }
        }


        public bool IsDocumentExist<T>(string path, out T retDoc) where T : DocumentViewModel
        {
            foreach (var doc in Documents)
            {
                if (doc is T)
                {
                    if (doc.Path == path)
                    {
                        retDoc = (T)doc;
                        return true;
                    }
                }
            }

            retDoc = null;
            return false;
        }

        public void OnRename(RenameViewModel renameViewModel)
        {
            var docList = Documents.ToList();
            foreach (var doc in docList)
            {
                if (renameViewModel.IsDirectory)
                {
                    if (doc.Path.ContainsIgnoreCase(renameViewModel.Path))
                    {
                        doc.Path = doc.Path.Replace(renameViewModel.Path + @"\", renameViewModel.NewPath + @"\");
                        doc.ToolTip = doc.Path;
                        doc.UpdatePathCrossDomain(doc.Path);
                    }
                }
                else
                {
                    if (doc.Path.EqualsIgnoreCase(renameViewModel.Path))
                    {
                        doc.Path = doc.Path.Replace(renameViewModel.Path, renameViewModel.NewPath);
                        doc.ToolTip = doc.Path;
                        doc.Title = System.IO.Path.GetFileNameWithoutExtension(renameViewModel.NewPath);
                        doc.UpdatePathCrossDomain(doc.Path);
                    }
                }
            }
        }




    }
}