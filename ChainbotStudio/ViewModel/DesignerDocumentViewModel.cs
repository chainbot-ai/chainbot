using System;
using Chainbot.Contracts.App;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using GalaSoft.MvvmLight;
using System.AddIn.Pipeline;
using System.Windows;
using System.Windows.Input;
using Chainbot.Contracts.UI;
using log4net;
using Chainbot.Contracts.Log;
using Plugins.Shared.Library;
using Newtonsoft.Json.Linq;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DesignerDocumentViewModel : DocumentViewModel
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IAppDomainControllerService _appDomainControllerService;
        private IWorkflowDesignerServiceProxy _workflowDesignerServiceProxy;
        private IWorkflowDesignerCollectServiceProxy _workflowDesignerCollectServiceProxy;
        private IServiceLocator _serviceLocator;
        private IWorkflowStateService _workflowStateService;
        private IDispatcherService _dispatcherService;
        private IMessageBoxService _messageBoxService;

        private ActivitiesViewModel _activitiesViewModel;

        public DesignerDocumentViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _appDomainControllerService = _serviceLocator.ResolveType<IAppDomainControllerService>();

            _workflowDesignerServiceProxy = _serviceLocator.ResolveType<IWorkflowDesignerServiceProxy>();
            _workflowDesignerCollectServiceProxy = _serviceLocator.ResolveType<IWorkflowDesignerCollectServiceProxy>();
            _workflowStateService = _serviceLocator.ResolveType<IWorkflowStateService>();
            _dispatcherService = _serviceLocator.ResolveType<IDispatcherService>();

            _activitiesViewModel = _serviceLocator.ResolveType<ActivitiesViewModel>();

            _messageBoxService = _serviceLocator.ResolveType<IMessageBoxService>();
            _logService = _serviceLocator.ResolveType<ILogService>();

            _workflowDesignerServiceProxy.ModelChangedEvent -= _workflowDesignerServiceProxy_ModelChangedEvent;
            _workflowDesignerServiceProxy.ModelChangedEvent += _workflowDesignerServiceProxy_ModelChangedEvent;

            _workflowDesignerServiceProxy.CanExecuteChanged -= _workflowDesignerServiceProxy_CanExecuteChanged;
            _workflowDesignerServiceProxy.CanExecuteChanged += _workflowDesignerServiceProxy_CanExecuteChanged;

            _workflowDesignerServiceProxy.ModelAddedEvent -= _workflowDesignerServiceProxy_ModelAddedEvent;
            _workflowDesignerServiceProxy.ModelAddedEvent += _workflowDesignerServiceProxy_ModelAddedEvent;

            _workflowStateService.BeginDebugEvent += _workflowStateService_BeginDebugEvent;
            _workflowStateService.EndDebugEvent += _workflowStateService_EndDebugEvent;

            _workflowStateService.BeginRunEvent += _workflowStateService_BeginRunEvent;
            _workflowStateService.EndRunEvent += _workflowStateService_EndRunEvent;
        }

       

        private void _workflowStateService_BeginRunEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(()=> {
                IsReadOnly = true;
            });
        }

        private void _workflowStateService_EndRunEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsReadOnly = false;
            });
        }

        private void _workflowStateService_BeginDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsReadOnly = true;
            });
        }

        private void _workflowStateService_EndDebugEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                IsReadOnly = false;
            });
        }


        protected override void OnReadOnlySet(bool isReadOnly)
        {
            _workflowDesignerServiceProxy.SetReadOnly(isReadOnly);
        }

        private void _workflowDesignerServiceProxy_CanExecuteChanged(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void _workflowDesignerServiceProxy_ModelChangedEvent(object sender, EventArgs e)
        {
            _dispatcherService.InvokeAsync(() => {
                IsDirty = true;
            });
        }

        private void _workflowDesignerServiceProxy_ModelAddedEvent(object sender, string e)
        {
            _dispatcherService.InvokeAsync(() => {
                if(!string.IsNullOrEmpty(e))
                {
                    _activitiesViewModel.AddToRecent(e);
                }
            });
        }

        public IWorkflowDesignerServiceProxy GetWorkflowDesignerServiceProxy()
        {
            return _workflowDesignerServiceProxy;
        }


        /// <summary>
        /// The <see cref="WorkflowDesignerView" /> property's name.
        /// </summary>
        public const string WorkflowDesignerViewPropertyName = "WorkflowDesignerView";

        private FrameworkElement _workflowDesignerViewProperty = null;

        public FrameworkElement WorkflowDesignerView
        {
            get
            {
                return _workflowDesignerViewProperty;
            }

            set
            {
                if (_workflowDesignerViewProperty == value)
                {
                    return;
                }

                _workflowDesignerViewProperty = value;
                RaisePropertyChanged(WorkflowDesignerViewPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WorkflowPropertyView" /> property's name.
        /// </summary>
        public const string WorkflowPropertyViewPropertyName = "WorkflowPropertyView";

        private FrameworkElement _workflowPropertyViewProperty = null;

        public FrameworkElement WorkflowPropertyView
        {
            get
            {
                return _workflowPropertyViewProperty;
            }

            set
            {
                if (_workflowPropertyViewProperty == value)
                {
                    return;
                }

                _workflowPropertyViewProperty = value;
                RaisePropertyChanged(WorkflowPropertyViewPropertyName);
            }
        }




        /// <summary>
        /// The <see cref="WorkflowOutlineView" /> property's name.
        /// </summary>
        public const string WorkflowOutlineViewPropertyName = "WorkflowOutlineView";

        private FrameworkElement _workflowOutlineViewProperty = null;

        /// <summary>
        /// Sets and gets the WorkflowOutlineView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FrameworkElement WorkflowOutlineView
        {
            get
            {
                return _workflowOutlineViewProperty;
            }

            set
            {
                if (_workflowOutlineViewProperty == value)
                {
                    return;
                }

                _workflowOutlineViewProperty = value;
                RaisePropertyChanged(WorkflowOutlineViewPropertyName);
            }
        }


        public void HideCurrentDebugArrow()
        {
            _workflowDesignerServiceProxy.HideCurrentLocation();
        }

        public void InsertActivity(string name,string assemblyQualifiedName)
        {
            _workflowDesignerServiceProxy.InsertActivity(name, assemblyQualifiedName);
        }

        public override void MakeView()
        {
            _workflowDesignerServiceProxy.Init(Path);

            WorkflowDesignerView = _workflowDesignerServiceProxy.GetDesignerView();
            WorkflowPropertyView = _workflowDesignerServiceProxy.GetPropertyView();
            WorkflowOutlineView = _workflowDesignerServiceProxy.GetOutlineView();
        }


        protected override void OnClose()
        {
            _workflowDesignerCollectServiceProxy.Remove(Path);
        }

        public override void Save()
        {
            if(IsReadOnly || !IsDirty)
            {
                return;
            }

            try
            {
                _workflowDesignerServiceProxy.Save();
                IsDirty = false;
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, Chainbot.Resources.Properties.Resources.Message_SaveDocumentError, err);
                _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.Message_SaveDocumentError);
            }      
        }

        protected override void FlushDesigner()
        {
            _workflowDesignerServiceProxy.FlushDesigner();
        }


        public override string XamlText
        {
            get
            {
                return _workflowDesignerServiceProxy.XamlText;
            }
        }


        public override void UpdatePathCrossDomain(string path)
        {
            _workflowDesignerServiceProxy.UpdatePath(path);
        }


        public override bool CanUndo()
        {
            return _workflowDesignerServiceProxy.CanUndo();
        }

        public override bool CanRedo()
        {
            return _workflowDesignerServiceProxy.CanRedo();
        }

        public override bool CanCut()
        {
            return _workflowDesignerServiceProxy.CanCut();
        }

        public override bool CanCopy()
        {
            return _workflowDesignerServiceProxy.CanCopy();
        }

        public override bool CanPaste()
        {
            return _workflowDesignerServiceProxy.CanPaste();
        }

        public override bool CanDelete()
        {
            return _workflowDesignerServiceProxy.CanDelete();
        }



        public override void Redo()
        {
            _workflowDesignerServiceProxy.Redo();
        }


        public override void Undo()
        {
            _workflowDesignerServiceProxy.Undo();
        }

        public override void Cut()
        {
            _workflowDesignerServiceProxy.Cut();
        }


        public override void Copy()
        {
            _workflowDesignerServiceProxy.Copy();
        }


        public override void Paste()
        {
            _workflowDesignerServiceProxy.Paste();
        }


        public override void Delete()
        {
            _workflowDesignerServiceProxy.Delete();
        }



        public override bool CanSave()
        {
            return true;
        }

         public JArray GetAllActivities()
        {
            return _workflowDesignerServiceProxy.GetAllActivities();
        }

        public JArray GetAllVariables()
        {
            return _workflowDesignerServiceProxy.GetAllVariables();
        }

        public JArray GetAllArguments()
        {
            return _workflowDesignerServiceProxy.GetAllArguments();
        }

        public JArray GetXamlValidInfo()
        {
            return _workflowDesignerServiceProxy.GetXamlValidInfo();
        }

        public void FocusActivity(string idRef)
        {
            _workflowDesignerServiceProxy.FocusActivity(idRef);
        }

        public void FocusVariable(string variableName, string idRef)
        {
            _workflowDesignerServiceProxy.FocusVariable(variableName,idRef);
        }

        public void FocusArgument(string argumentName)
        {
            _workflowDesignerServiceProxy.FocusArgument(argumentName);
        }

        public bool CheckUnusedVariables()
        {
            return _workflowDesignerServiceProxy.CheckUnusedVariables();
        }

        public bool CheckUnusedArguments()
        {
            return _workflowDesignerServiceProxy.CheckUnusedArguments();
        }

        public JArray GetUnusedVariables()
        {
            return _workflowDesignerServiceProxy.GetUnusedVariables();
        }

        public JArray GetUnusedArguments()
        {
            return _workflowDesignerServiceProxy.GetUnusedArguments();
        }

        public JArray GetUsedVariables()
        {
            return _workflowDesignerServiceProxy.GetUsedVariables();
        }

        public JArray GetUsedArguments()
        {
            return _workflowDesignerServiceProxy.GetUsedArguments();
        }

        public JArray GetUnsetOutArgumentActivities()
        {
            return _workflowDesignerServiceProxy.GetUnsetOutArgumentActivities();
        }

        public JArray GetAbnormalActivities()
        {
            return _workflowDesignerServiceProxy.GetAbnormalActivities();
        }

        public JArray GetErrLocationActivities()
        {
            return _workflowDesignerServiceProxy.GetErrLocationActivities();
        }

        public void RemoveUnusedVariables()
        {
            _workflowDesignerServiceProxy.RemoveUnusedVariables();
        }

        public void RemoveUnusedArguments()
        {
            _workflowDesignerServiceProxy.RemoveUnusedArguments();
        }

        public void ZoomIn()
        {
            _workflowDesignerServiceProxy.ZoomIn();
        }

        public void ZoomOut()
        {
            _workflowDesignerServiceProxy.ZoomOut();
        }
    }
}