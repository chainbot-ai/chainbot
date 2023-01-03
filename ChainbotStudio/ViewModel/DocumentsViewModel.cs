using Chainbot.Contracts.App;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System;
using Chainbot.Contracts.Workflow;
using Chainbot.Contracts.Utils;
using Plugins.Shared.Library;
using Chainbot.Contracts.Project;
using System.Threading.Tasks;
using Chainbot.Contracts.UI;
using System.Linq;
using System.ComponentModel;
using Chainbot.Cores.Classes;
using Chainbot.Contracts.Classes;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocumentsViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;
        private IWorkflowStateService _workflowStateService;
        private ICommonService _commonService;
        private IProjectManagerService _projectManagerService;
        private IDispatcherService _dispatcherService;

        public event EventHandler DocumentSelectChangeEvent;

        /// <summary>
        /// Initializes a new instance of the DocumentsViewModel class.
        /// </summary>
        public DocumentsViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();

            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();

            _projectManagerService.ProjectPreviewCloseEvent += _projectManagerService_ProjectPreviewCloseEvent;
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
                foreach(var doc in Documents)
                {
                    if(doc.IsSelected)
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

        public void NewDesignerDocument(string path)
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

            doc.IsSelected = true;

            Documents.Add(doc);
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


        public bool IsDocumentExist<T>(string path, out T retDoc) where T: DocumentViewModel
        {
            foreach (var doc in Documents)
            {
                if(doc is T)
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
                    if(doc.Path.ContainsIgnoreCase(renameViewModel.Path))
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