using Chainbot.Contracts.App;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.UI;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using ChainbotStudio.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Plugins.Shared.Library.Executor;
using System;
using System.Activities.Tracking;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Plugins.Shared.Library;
using System.Windows.Threading;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DebugViewModel : ViewModelBase
    {
        private IServiceLocator _serviceLocator;
        private ICommonService _commonService;
        private IWorkflowStateService _workflowStateService;
        private IDispatcherService _dispatcherService;

        private IProjectManagerService _projectManagerService;

        private IPathConfigService _pathConfigService;

        private DocksViewModel _docksViewModel;

        /// <summary>
        /// Initializes a new instance of the DebugViewModel class.
        /// </summary>
        public DebugViewModel(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _projectManagerService = _serviceLocator.ResolveType<IProjectManagerService>();
            _commonService = _serviceLocator.ResolveType<ICommonService>();
            _pathConfigService = _serviceLocator.ResolveType<IPathConfigService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();
            
            _docksViewModel = _serviceLocator.ResolveType<DocksViewModel>();

            _projectManagerService.ProjectPreviewOpenEvent += _projectManagerService_ProjectPreviewOpenEvent;
            _projectManagerService.ProjectPreviewCloseEvent += _projectManagerService_ProjectPreviewCloseEvent;

            _workflowStateService.ShowLocalsEvent += _workflowStateService_ShowLocalsEvent;

            _workflowStateService.BeginDebugEvent += _workflowStateService_BeginDebugEvent;
            _workflowStateService.EndDebugEvent += _workflowStateService_EndDebugEvent;

            _workflowStateService.CheckDocumentExistEvent += _workflowStateService_CheckDocumentExistEvent;

        }

        private void _workflowStateService_BeginDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                TrackerItems.Clear();
            });
        }

        private void _workflowStateService_EndDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                TrackerItems.Clear();
            });
        }

        private void _projectManagerService_ProjectPreviewOpenEvent(object sender, EventArgs e)
        {

        }

        private void _projectManagerService_ProjectPreviewCloseEvent(object sender, CancelEventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                TrackerItems.Clear();
            });
        }



        public string GetFriendlyTypeName(Type type)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var typeReferenceExpression = new CodeTypeReferenceExpression(new CodeTypeReference(type));
            using (var writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, new CodeGeneratorOptions());
                return writer.GetStringBuilder().ToString();
            }
        }


        
        private void _workflowStateService_ShowLocalsEvent(object sender, object e)
        {
            if(e is ShowLocalsJsonMessage)
            {
                var showLocalsJsonMessage = e as ShowLocalsJsonMessage;

                var vars = showLocalsJsonMessage.Variables;
                var args = showLocalsJsonMessage.Arguments;

                _dispatcherService.InvokeAsync(() =>
                {
                    TrackerItems.Clear();
                    foreach (var item in vars)
                    {
                        var trackerItem = _serviceLocator.ResolveType<TrackerItemViewModel>();
                        trackerItem.Property = item.Key;
                        trackerItem.Value = item.Value;

                        TrackerItems.Add(trackerItem);
                    }

                    foreach (var item in args)
                    {
                        var trackerItem = _serviceLocator.ResolveType<TrackerItemViewModel>();
                        trackerItem.Property = item.Key;
                        trackerItem.Value = item.Value;

                        TrackerItems.Add(trackerItem);
                    }
                });
            }
            
        }

        private void _workflowStateService_CheckDocumentExistEvent(object sender, string workflowFilePath)
        {
            if (!Path.IsPathRooted(workflowFilePath))
            {
                workflowFilePath = Path.Combine(SharedObject.Instance.ProjectPath, workflowFilePath);
            }

            DesignerDocumentViewModel doc;
            bool isExist = _docksViewModel.IsDocumentExist(workflowFilePath, out doc);

            if (!isExist)
            {
                _dispatcherService.Invoke(() =>
                {
                    _docksViewModel.NewDesignerDocument(workflowFilePath);
                }, DispatcherPriority.Render);
            }
            else
            {
                doc.IsSelected = true;
            }

        }

        /// <summary>
        /// The <see cref="TrackerItems" /> property's name.
        /// </summary>
        public const string TrackerItemsPropertyName = "TrackerItems";

        private ObservableCollection<TrackerItemViewModel> _trackerItemsProperty = new ObservableCollection<TrackerItemViewModel>();

        /// <summary>
        /// Sets and gets the TrackerItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<TrackerItemViewModel> TrackerItems
        {
            get
            {
                return _trackerItemsProperty;
            }

            set
            {
                if (_trackerItemsProperty == value)
                {
                    return;
                }

                _trackerItemsProperty = value;
                RaisePropertyChanged(TrackerItemsPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Value" /> property's name.
        /// </summary>
        public const string ValuePropertyName = "Value";

        private string _valueProperty = "";

        /// <summary>
        /// Sets and gets the Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Value
        {
            get
            {
                return _valueProperty;
            }

            set
            {
                if (_valueProperty == value)
                {
                    return;
                }

                _valueProperty = value;
                RaisePropertyChanged(ValuePropertyName);
            }
        }


       


    }
}