using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.AddIn.Contract;
using System.Activities.Presentation;
using System.Windows;
using System.AddIn.Pipeline;
using System.Xaml;
using System.Collections;
using System.Activities.Presentation.View;
using System.Activities.Presentation.Services;
using Chainbot.Contracts.Classes;
using Chainbot.Cores.ExpressionEditor;
using Chainbot.Contracts.Config;
using System.Activities;
using System.Activities.Statements;
using Chainbot.Contracts.Activities;
using Chainbot.Cores.Classes;
using System.Activities.Presentation.Model;
using Chainbot.Contracts.Project;
using System.Activities.Debugger;
using Chainbot.Contracts.UI;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.Activities.Presentation.Debug;
using Chainbot.Contracts.Utils;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Validation;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Controls;
using Chainbot.Cores.Commands;
using System.Windows.Input;
using ReflectionMagic;
using System.Activities.Expressions;
using System.Activities.XamlIntegration;
using System.Activities.Presentation.ViewState;
using Plugins.Shared.Library;
using System.Collections.ObjectModel;
using System.Activities.Core.Presentation;
using Newtonsoft.Json;
using Chainbot.Shared.Extensions;
using Chainbot.Contracts.Log;
using System.Globalization;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowDesignerService : MarshalByRefServiceBase, IWorkflowDesignerService
    {
        private readonly DesignerThemeBehavior _designerThemeBehavior = new DesignerThemeBehavior();

        private IProjectManagerService _projectManagerService;
        private IPathConfigService _pathConfigService;
        private IActivitiesDefaultAttributesService _activitiesDefaultAttributesService;
        private IWorkflowDesignerCollectService _workflowDesignerCollectService;
        private IDispatcherService _dispatcherService;
        private IWorkflowBreakpointsService _workflowBreakpointsService;
        private ICommonService _commonService;
        private IWorkflowDesignerJumpService _workflowDesignerJumpService;
        private IServerSettingsService _serverSettingsService;
        private IWorkflowActivityService _workflowActivityService;

        private WorkflowDesigner _designer;
        private FrameworkElement _view;

        private EventHandler _validationServiceValidationCompletedCallback;

        Dictionary<string, SourceLocation> _activityIdToSourceLocationMapping = new Dictionary<string, SourceLocation>();

        public string Path { get; private set; }

        private string _currentDraggingDisplayName = "";
        private string _currentDraggingAssemblyQualifiedName = "";
        private string _currentDraggingTypeOf = "";

        public event EventHandler ModelChangedEvent;
        public event EventHandler CanExecuteChanged;
        public event EventHandler<string> ModelAddedEvent;


        private SurroundWithTryCatchCommand _surroundWithTryCatchCommand;
        private RemoveTryCatchCommand _removeTryCatchCommand;
        private EnableActivityCommand _enableActivityCommand;
        private DisableActivityCommand _disableActivityCommand;
        private ToggleBreakpointCommand _toggleBreakpointCommand;
        private StartRunFromHereCommand _startRunFromHereCommand;
        private HelpActivityCommand _helpActivityCommand;
        private UnusedDeleteCommand _unusedDeleteCommand;
        private RunThisActivityCommand _runThisActivityCommand;

        public WorkflowDesignerService(IProjectManagerService projectManagerService, IPathConfigService pathConfigService
            , IActivitiesDefaultAttributesService activitiesDefaultAttributesService
            , IWorkflowDesignerCollectService workflowDesignerCollectService, IDispatcherService dispatcherService
            , IWorkflowBreakpointsService workflowBreakpointsService, ICommonService commonService
            , IWorkflowDesignerJumpService workflowDesignerJumpService
            , IServerSettingsService serverSettingsService
            , IWorkflowActivityService workflowActivityService)
        {
            _projectManagerService = projectManagerService;
            _pathConfigService = pathConfigService;
            _workflowDesignerJumpService = workflowDesignerJumpService;
            _serverSettingsService = serverSettingsService;
            _workflowActivityService = workflowActivityService;

            _activitiesDefaultAttributesService = activitiesDefaultAttributesService;
            _activitiesDefaultAttributesService.Register();

            _workflowDesignerCollectService = workflowDesignerCollectService;
            _workflowDesignerCollectService.Add(this);

            _dispatcherService = dispatcherService;

            _workflowBreakpointsService = workflowBreakpointsService;
            _workflowActivityService = workflowActivityService;
            _commonService = commonService;

         

            this._designer = new WorkflowDesigner();
            TryInitConfigurationService(_designer);

            _workflowDesignerJumpService.Init(_designer);

            this._designer.PropertyInspectorFontAndColorData = XamlServices.Save(GetThemeHashTable());

            _view = this._designer.View as FrameworkElement;

            _view.PreviewDragEnter += _view_PreviewDragEnter;

            DesignerView.UndoCommand.CanExecuteChanged += UndoCommand_CanExecuteChanged;
            DesignerView.RedoCommand.CanExecuteChanged += RedoCommand_CanExecuteChanged;
        }


        private void TryInitConfigurationService(WorkflowDesigner designer)
        {
            try
            {
                DesignerConfigurationService requiredService = designer.Context.Services.GetRequiredService<DesignerConfigurationService>();
                requiredService.TargetFrameworkName = new FrameworkName(".NETFramework,Version=v4.6.1");
                requiredService.MultipleItemsContextMenuEnabled = true;
                requiredService.LoadingFromUntrustedSourceEnabled = true;
            }
            catch (Exception exception)
            {
                exception.Trace("TryInitConfigurationService");
            }
        }

        private void _view_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FilePath"))
            {
                var filePath = e.Data.GetData("FilePath") as string;

                if (e.Data.GetDataPresent("FileType"))
                {
                    var fileType = e.Data.GetData("FileType") as string;
                    if (fileType == "Xaml")
                    {
                        InvokeWorkflowFileFactory.FilePath = filePath;
                    }
                    else if (fileType == "Python")
                    {
                        InvokePythonFileFactory.FilePath = filePath;
                    }
                    else if (fileType == "JavaScript")
                    {
                        InjectJavaScriptFileFactory.FilePath = filePath;
                    }
                    else if (fileType == "Snippet")
                    {
                        InsertSnippetItemFactory.FilePath = filePath;
                    }
                }
            }


            if (e.Data.GetDataPresent("DisplayName"))
            {
                _currentDraggingDisplayName = e.Data.GetData("DisplayName") as string;
            }
            else
            {
                _currentDraggingDisplayName = "";
            }

            if (e.Data.GetDataPresent("AssemblyQualifiedName"))
            {
                _currentDraggingAssemblyQualifiedName = e.Data.GetData("AssemblyQualifiedName") as string;
            }
            else
            {
                _currentDraggingAssemblyQualifiedName = "";
            }

            if (e.Data.GetDataPresent("TypeOf"))
            {
                _currentDraggingTypeOf = e.Data.GetData("TypeOf") as string;
            }
            else
            {
                _currentDraggingTypeOf = "";
            }
        }

        private void RedoCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UndoCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Init(string path)
        {
            Path = path;

            _workflowDesignerJumpService.Path = path;
        }

        public void UpdatePath(string path)
        {
            Path = path;

            _workflowDesignerJumpService.Path = path;

            BindCommondUpdate(path);
        }

        private void BindCommondUpdate(string path)
        {
            _surroundWithTryCatchCommand.Path = path;
            _removeTryCatchCommand.Path = path;
            _enableActivityCommand.Path = path;
            _disableActivityCommand.Path = path;
            _toggleBreakpointCommand.Path = path;
            _startRunFromHereCommand.Path = path;
            _unusedDeleteCommand.Path = path;
        }

        private object GetThemeHashTable()
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary();

            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/PropertyInspectorFontAndColorData.Dark.xaml");
            }
            else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/PropertyInspectorFontAndColorData.Light.xaml");
            }

            return new Hashtable(resourceDictionary);
        }

        public INativeHandleContract GetDesignerView()
        {
            PublishExpressionEditorService();

            var designerConfigurationService = _designer.Context.Services.GetService<DesignerConfigurationService>();

            designerConfigurationService.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 6));
            designerConfigurationService.AutoSurroundWithSequenceEnabled = true;
            designerConfigurationService.AnnotationEnabled = true;

            this._designer.Load(Path);

            if (this._designer.IsInErrorState())
            {
                throw new Exception(Chainbot.Resources.Properties.Resources.Message_Exception1);
            }

            _designerThemeBehavior.Attach(this._designer);

            _view.Loaded -= _view_Loaded;
            _view.Loaded += _view_Loaded;


            InitContextMenu(this._designer.ContextMenu.Items);

            return FrameworkElementAdapters.ViewToContractAdapter((FrameworkElement)this._designer.View);
        }

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            var designerView = _designer.Context.Services.GetService<DesignerView>();

            designerView.WorkflowShellBarItemVisibility =
                ShellBarItemVisibility.All;

            SubscribeDesignerEvents();

            ShowBreakpointsOnValidationCompleted();
        }

        private void ShowBreakpointsOnValidationCompleted()
        {
            ValidationService validationService = _designer.Context.Services.GetService<ValidationService>();
            if (validationService != null)
            {
                this._validationServiceValidationCompletedCallback = new EventHandler(this._validationServiceValidationComplete);
                EventInfo @event = validationService.GetType().GetEvent("ValidationCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
                EventHandler validationServiceValidationCompletedCallback = this._validationServiceValidationCompletedCallback;
                MethodBase addMethod = @event.GetAddMethod(true);
                object obj = validationService;
                object[] parameters = new EventHandler[]
                {
                    validationServiceValidationCompletedCallback
                };
                addMethod.Invoke(obj, parameters);
            }
        }

        private void _validationServiceValidationComplete(object sender, EventArgs e)
        {
            ShowBreakpoints();

            if (SharedObject.Instance.CurrentDebugWorkflowFilePath != null && SharedObject.Instance.CurrentDebugLocationId != null)
            {
                ShowCurrentLocation(SharedObject.Instance.CurrentDebugLocationId, SharedObject.Instance.CurrentDebugWorkflowFilePath);
                SharedObject.Instance.CurrentDebugLocationId = null;
                SharedObject.Instance.CurrentDebugWorkflowFilePath = null;    
            }
        }

        private void SubscribeDesignerEvents()
        {
            var modelService = _designer.Context.Services.GetService<ModelService>();
            modelService.ModelChanged -= ModelService_ModelChanged;
            modelService.ModelChanged += ModelService_ModelChanged;

            _designer.ModelChanged -= _designer_ModelChanged;
            _designer.ModelChanged += _designer_ModelChanged;
        }

        private void ClearCurrentDraggingInfo()
        {
            _currentDraggingDisplayName = "";
            _currentDraggingAssemblyQualifiedName = "";
            //_currentDraggingTypeOf = "";
        }

        private void _designer_ModelChanged(object sender, EventArgs e)
        {
            ModelChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        private ModelItem RootModelItem
        {
            get
            {
                return this._designer.Context.GetRootModelItem();
            }
        }

        public string XamlText
        {
            get
            {
                this._designer.Flush();
                return this._designer.Text;
            }
        }

        private void ModelService_ModelChanged(object sender, ModelChangedEventArgs e)
        {
            ModelChangedEvent?.Invoke(this, EventArgs.Empty);

            ModelChangeInfo modelChangeInfo = (e != null) ? e.ModelChangeInfo : null;
            ModelChangeType? modelChangeType = (modelChangeInfo != null) ? new ModelChangeType?(modelChangeInfo.ModelChangeType) : null;
            if (modelChangeType != null)
            {
                switch (modelChangeType.GetValueOrDefault())
                {
                    case ModelChangeType.PropertyChanged:
                        PropertyChanged(modelChangeInfo);

                        ModelAddedEvent?.Invoke(this, _currentDraggingTypeOf);
                        break;
                    case ModelChangeType.CollectionItemAdded:
                        CollectionItemAdded(modelChangeInfo);

                        ModelAddedEvent?.Invoke(this, _currentDraggingTypeOf);

                        break;
                    case ModelChangeType.CollectionItemRemoved:
                        return;
                    default:
                        return;
                }

                return;
            }
        }




        private void OnCollectionItemAdded(object obj, ModelItem modelItem)
        {
            OnRenameActivityDisplayName(obj, modelItem);

            OnSetActivityDefaultProperties(obj);
        }


        private void CollectionItemAdded(ModelChangeInfo changeInfo)
        {
            var obj = changeInfo.Value.GetCurrentValue();

            if (obj is Activity)
            {
                var activity = obj as Activity;

                OnCollectionItemAdded(activity, changeInfo.Value);
            }

            if (obj is FlowDecision)
            {
                var activity = obj as FlowDecision;

                OnCollectionItemAdded(activity, changeInfo.Value);
            }

            if (obj is FlowStep)
            {
                var flowStep = obj as FlowStep;
                var activity = flowStep.Action;
                OnCollectionItemAdded(activity, changeInfo.Value);
            }

            if (obj is FlowSwitch<object>)
            {
                var flowSwitch = obj as FlowSwitch<object>;
                var activity = flowSwitch;
                OnCollectionItemAdded(activity, changeInfo.Value);
            }

            if (obj is PickBranch)
            {
                var pickBranch = obj as PickBranch;
                var activity = pickBranch;
                OnCollectionItemAdded(activity, changeInfo.Value);
            }
        }


        private void PropertyChanged(ModelChangeInfo changeInfo)
        {
            if (changeInfo.PropertyName.Equals("Implementation"))
            {
                ModelItem rootModelItem = this.RootModelItem;
                if (rootModelItem != null)
                {
                    var sequenceDisplayName = _projectManagerService.ActivitiesTypeOfDict["Sequence"].Name;

                    if (rootModelItem.IsSequence())
                    {
                        DesignerWrapper.SetDisplayName(changeInfo.Value, sequenceDisplayName);
                    }

                    if (changeInfo.Value.Parent == changeInfo.Value.Root)
                    {
                        if (!changeInfo.Value.IsSequence() && !changeInfo.Value.IsFlowchart())
                        {
                            ActivityHelper.SurroundItemWithSequence(rootModelItem, this._designer.Context, sequenceDisplayName);
                        }
                    }
                }
            }

            if (changeInfo.Value == null)
            {
                return;
            }
            if (DesignerWrapper.IsSpecialProperty(changeInfo))
            {
                if (!string.IsNullOrEmpty(_currentDraggingDisplayName))
                {
                    DesignerWrapper.SetDisplayName(changeInfo.Value, _currentDraggingDisplayName);
                    ClearCurrentDraggingInfo();
                }
            }
        }

        public void ShowBreakpoints()
        {
            _workflowBreakpointsService.ShowBreakpoints(Path);
        }


        private void PublishExpressionEditorService()
        {
            SyntaxService.CacheInFolder = _pathConfigService.AppDataDir;
            _designer.Context.Services.Publish<IExpressionEditorService>(new ActiproExpressionEditorService(_designer.Context));
        }

        public INativeHandleContract GetPropertyView()
        {
            var view = this._designer.PropertyInspectorView as FrameworkElement;

            FrameworkElement typeLabel = view.FindName("_typeLabel") as FrameworkElement;
            var border = (typeLabel.Parent as FrameworkElement).Parent as FrameworkElement;
            border.Visibility = Visibility.Collapsed;

            UIElement propertyToolBar = view.FindName("_propertyToolBar") as UIElement;
            if (propertyToolBar != null)
            {
                propertyToolBar.Visibility = Visibility.Collapsed;
            }

            return FrameworkElementAdapters.ViewToContractAdapter(view);
        }

        public INativeHandleContract GetOutlineView()
        {
            var view = this._designer.OutlineView as FrameworkElement;
            return FrameworkElementAdapters.ViewToContractAdapter(view);
        }

        public void Save()
        {
            _designer.Flush();
            var xamlText = _designer.Text;

            if (_designer.IsInErrorState())
            {
                throw new Exception(Chainbot.Resources.Properties.Resources.Message_Exception2);
            }
            else if (string.IsNullOrEmpty(xamlText.Trim()) ||
                string.IsNullOrEmpty(xamlText.Replace("\0", "").Trim()) ||
                (!xamlText.StartsWith("<") && !xamlText.EndsWith(">")))
            {
                throw new Exception(Chainbot.Resources.Properties.Resources.Message_Exception3);
            }

            System.IO.File.WriteAllText(Path, xamlText);
        }

        public void FlushDesigner()
        {
            _designer.Flush();
        }

        public WorkflowDesigner GetWorkflowDesigner()
        {
            return _designer;
        }

        public bool CanUndo()
        {
            return DesignerView.UndoCommand.CanExecute(null);
        }

        public bool CanRedo()
        {
            return DesignerView.RedoCommand.CanExecute(null);
        }


        public bool CanCut()
        {
            return DesignerView.CutCommand.CanExecute(null);
        }

        public bool CanCopy()
        {
            return DesignerView.CopyCommand.CanExecute(null);
        }

        public bool CanPaste()
        {
            return DesignerView.PasteCommand.CanExecute(null);
        }

        public bool CanDelete()
        {
            return DesignerView.CutCommand.CanExecute(null);
        }

        public void Undo()
        {
            DesignerView.UndoCommand.Execute(null);
        }

        public void Redo()
        {
            DesignerView.RedoCommand.Execute(null);
        }


        public void Cut()
        {
            DesignerView.CutCommand.Execute(null);
        }

        public void Copy()
        {
            DesignerView.CopyCommand.Execute(null);
        }

        public void Paste()
        {
            DesignerView.PasteCommand.Execute(null);
        }

        public void Delete()
        {
            var selection = _designer.Context.Items.GetValue<Selection>();
            DesignerWrapper.RemoveActivity(selection.PrimarySelection);
        }

        public void ShowCurrentLocation(string locationId)
        {
            if (!_activityIdToSourceLocationMapping.ContainsKey(locationId))
            {
                return;
            }

            SourceLocation srcLoc = _activityIdToSourceLocationMapping[locationId];
            _dispatcherService.InvokeAsync(() =>
            {
                _designer.DebugManagerView.CurrentLocation = srcLoc;
            }, DispatcherPriority.Render);
        }

        public void ShowCurrentLocation(string locationId, string workflowFilePath)
        {
            if (!System.IO.Path.IsPathRooted(workflowFilePath))
            {
                workflowFilePath = System.IO.Path.Combine(SharedObject.Instance.ProjectPath, workflowFilePath);
            }

            var workflowDesignerServer = _workflowDesignerCollectService.Get(workflowFilePath);
            if (workflowDesignerServer == null)
            {
                return;
            }

            var workflowDesigner = workflowDesignerServer.GetWorkflowDesigner();
            _dispatcherService.InvokeAsync(() =>
            {
                Dictionary<string, SourceLocation> _activityIdToSourceLocationMapping = new Dictionary<string, SourceLocation>();
                ChainbotDebuggerService.BuildSourceLocationMappings(workflowDesigner, ref _activityIdToSourceLocationMapping);

                if (!_activityIdToSourceLocationMapping.ContainsKey(locationId))
                {
                    SharedObject.Instance.CurrentDebugWorkflowFilePath = workflowFilePath;
                    SharedObject.Instance.CurrentDebugLocationId = locationId;
                    return;
                }

                SourceLocation srcLoc = _activityIdToSourceLocationMapping[locationId];
                workflowDesigner.DebugManagerView.CurrentLocation = srcLoc;
            }, DispatcherPriority.Render);

        }

        public void HideCurrentLocation()
        {
            _dispatcherService.InvokeAsync(() =>
            {
                _designer.DebugManagerView.CurrentLocation = null;
            }, DispatcherPriority.Render);
        }

        public void HideCurrentLocation(string workflowFilePath)
        {
            if (!System.IO.Path.IsPathRooted(workflowFilePath))
            {
                workflowFilePath = System.IO.Path.Combine(SharedObject.Instance.ProjectPath, workflowFilePath);
            }

            var workflowDesignerServer = _workflowDesignerCollectService.Get(workflowFilePath);
            if (workflowDesignerServer == null)
            {
                return;
            }

            var workflowDesigner = workflowDesignerServer.GetWorkflowDesigner();
            _dispatcherService.InvokeAsync(() =>
            {
                workflowDesigner.DebugManagerView.CurrentLocation = null;
            }, DispatcherPriority.Render);
        }

        public string GetActivityIdJsonArray()
        {
            ChainbotDebuggerService.BuildSourceLocationMappings(_designer, ref _activityIdToSourceLocationMapping);

            JArray jarrayJsonObj = new JArray();
            foreach (var key in _activityIdToSourceLocationMapping.Keys.ToArray())
            {
                jarrayJsonObj.Add(key);
            }

            return jarrayJsonObj.ToString();
        }

        public string GetBreakpointIdJsonArray()
        {
            JArray breakpointIdJsonObj = new JArray();
            var breakpointLocations = _designer.DebugManagerView.GetBreakpointLocations();
            foreach (var item in _activityIdToSourceLocationMapping)
            {
                var id = item.Key;
                SourceLocation srcLoc = item.Value;
                if (breakpointLocations.ContainsKey(srcLoc))
                {
                    var types = breakpointLocations[srcLoc];
                    if (types == (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                    {
                        breakpointIdJsonObj.Add(id);
                    }
                }
            }

            return breakpointIdJsonObj.ToString();
        }

        public string GetTrackerVars()
        {
            List<string> varNameLsit = new List<string>();

            ModelService modelService = _designer.Context.Services.GetService<ModelService>();

            IEnumerable<ModelItem> flowcharts = modelService.Find(modelService.Root, typeof(Flowchart));
            IEnumerable<ModelItem> sequences = modelService.Find(modelService.Root, typeof(Sequence));

            foreach (var modelItem in flowcharts)
            {
                foreach (var varItem in modelItem.Properties["Variables"].Collection)
                {
                    var varName = varItem.Properties["Name"].ComputedValue as string;
                    varNameLsit.Add(varName);
                }
            }

            foreach (var modelItem in sequences)
            {
                foreach (var varItem in modelItem.Properties["Variables"].Collection)
                {
                    var varName = varItem.Properties["Name"].ComputedValue as string;
                    varNameLsit.Add(varName);
                }
            }

            JArray jarr = new JArray();
            foreach (var item in varNameLsit)
            {
                jarr.Add(item);
            }

            return jarr.ToString();
        }

        public void SetReadOnly(bool isReadOnly)
        {
            _designer.Context.Items.GetValue<ReadOnlyState>().IsReadOnly = isReadOnly;
            _dispatcherService.InvokeAsync(() => {
                var designView = _designer.Context.Services.GetService<DesignerView>();
                if (designView != null)
                {
                    designView.IsReadOnly = isReadOnly;
                }
                else
                {

                }
            });

        }



        private void OnRenameActivityDisplayName(object obj, ModelItem modelItem)
        {
            dynamic activity = obj;

            var assemblyQualifiedName = activity.GetType().AssemblyQualifiedName;
            if (assemblyQualifiedName == _currentDraggingAssemblyQualifiedName)
            {
                if (!string.IsNullOrEmpty(_currentDraggingDisplayName))
                {
                    DesignerWrapper.SetDisplayName(modelItem, _currentDraggingDisplayName);

                    ClearCurrentDraggingInfo();
                }
            }
            else
            {
                if (_currentDraggingAssemblyQualifiedName.Contains("WithBodyFactory"))
                {
                    if (!string.IsNullOrEmpty(_currentDraggingDisplayName))
                    {
                        DesignerWrapper.SetDisplayName(modelItem, _currentDraggingDisplayName);

                        ClearCurrentDraggingInfo();
                    }
                }
            }
        }

        private void OnSetActivityDefaultProperties(object obj)
        {
            if (obj is Delay)
            {
                var activity = obj as Delay;
                activity.Duration = TimeSpan.FromSeconds(3);
            }
        }

        private MenuItem MakeMenuItem(string header, ICommand cmd)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = header;
            menuItem.Command = cmd;
            return menuItem;
        }

        private void InitContextMenu(ItemCollection items)
        {
            try
            {
                BindCommondInitialize();

                items.Add(new Separator());
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header1, _surroundWithTryCatchCommand));
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header2, _removeTryCatchCommand));
                items.Add(new Separator());
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header3, _enableActivityCommand));
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header4, _disableActivityCommand));
                items.Add(new Separator());
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header5, _toggleBreakpointCommand));
                items.Add(new Separator());
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header6, _unusedDeleteCommand));
                //items.Add(new Separator());
                //items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header7, _startRunFromHereCommand));
                //items.Add(new Separator());
                //items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header8, _runThisActivityCommand));
                items.Add(new Separator());
                items.Add(MakeMenuItem(Chainbot.Resources.Properties.Resources.ContextMenu_Header9, _helpActivityCommand));
            }
            catch (Exception)
            {
            }
        }

        private void BindCommondInitialize()
        {
            _surroundWithTryCatchCommand = new SurroundWithTryCatchCommand(_workflowActivityService, Path);
            _removeTryCatchCommand = new RemoveTryCatchCommand(_workflowActivityService, Path);
            _enableActivityCommand = new EnableActivityCommand(_workflowActivityService, Path);
            _disableActivityCommand = new DisableActivityCommand(_workflowActivityService, Path);
            _toggleBreakpointCommand = new ToggleBreakpointCommand(_workflowBreakpointsService, Path);
            _startRunFromHereCommand = new StartRunFromHereCommand(_workflowActivityService, Path);
            _helpActivityCommand = new HelpActivityCommand(this._designer, _serverSettingsService.HelpLinkUrl);
            _unusedDeleteCommand = new UnusedDeleteCommand(_workflowActivityService, Path);
            _runThisActivityCommand = new RunThisActivityCommand(_workflowActivityService);
        }

        public void RefreshArgumentsView()
        {
            DesignerView designerView = this._designer.Context.Services.GetService<DesignerView>();
            ContentControl contentControl = (ContentControl)designerView.FindName("arguments1");

            ((dynamic)contentControl.AsDynamic()).isCollectionLoaded = false;
            ((dynamic)contentControl.AsDynamic()).Populate();
        }

        public void UpdateCurrentSelecteddDesigner()
        {
            GlobalVars.CurrentWorkflowDesignerService = this;
        }

        public string GetAllActivities()
        {
            return _workflowDesignerJumpService.GetAllActivities();
        }

        public string GetAllVariables()
        {
            return _workflowDesignerJumpService.GetAllVariables();
        }

        public string GetAllArguments()
        {
            return _workflowDesignerJumpService.GetAllArguments();
        }

        public void FocusActivity(string idRef)
        {
            _workflowDesignerJumpService.FocusActivity(idRef);
        }

        public void FocusVariable(string variableName, string idRef)
        {
            _workflowDesignerJumpService.FocusVariable(variableName,idRef);
        }

        public void FocusArgument(string argumentName)
        {
            _workflowDesignerJumpService.FocusArgument(argumentName);
        }

        public bool CheckUnusedVariables()
        {
            return _workflowActivityService.CheckUnusedVariables(Path);
        }

        public bool CheckUnusedArguments()
        {
            return _workflowActivityService.CheckUnusedArguments(Path);
        }

        public string GetUnusedVariables()
        {
            return _workflowActivityService.GetUnusedVariables(Path);
        }

        public string GetUnusedArguments()
        {
            return _workflowActivityService.GetUnusedArguments(Path);
        }

        public string GetUsedVariables()
        {
            return _workflowActivityService.GetUsedVariables(Path);
        }

        public string GetUsedArguments()
        {
            return _workflowActivityService.GetUsedArguments(Path);
        }

        public string GetUnsetOutArgumentActivities()
        {
            return _workflowActivityService.GetUnsetOutArgumentActivities(Path);
        }

        public string GetAbnormalActivities()
        {
            return _workflowActivityService.GetAbnormalActivities(Path);
        }

        public string GetErrLocationActivities()
        {
            return _workflowActivityService.GetErrLocationActivities(Path);
        }

        public void RemoveUnusedVariables()
        {
            _workflowActivityService.RemoveUnusedVariables(Path);
        }

        public void RemoveUnusedArguments()
        {
            _workflowActivityService.RemoveUnusedArguments(Path);
        }

        public string GetXamlValidInfo()
        {
            return _workflowActivityService.GetXamlValidInfo(Path);
        }

        public void InsertActivity(string name, string assemblyQualifiedName)
        {
            if (_designer != null)
            {
                ModelService modelService = _designer.Context.Services.GetService<ModelService>();
                ModelItem rootModelItem = modelService.Root.Properties["Implementation"].Value;

                var assemblyQualifiedNameWithoutBodyFactory = assemblyQualifiedName.Replace("WithBodyFactory", "");
                var type = Type.GetType(assemblyQualifiedNameWithoutBodyFactory);
                if(type == null && assemblyQualifiedName.Contains("WithBodyFactory"))
                {
                    assemblyQualifiedNameWithoutBodyFactory = assemblyQualifiedNameWithoutBodyFactory.Replace("System.Activities.Core.Presentation.Factories", "System.Activities.Statements");
                    assemblyQualifiedNameWithoutBodyFactory = assemblyQualifiedNameWithoutBodyFactory.Replace("System.Activities.Core.Presentation", "System.Activities");
                    
                    type = Type.GetType(assemblyQualifiedNameWithoutBodyFactory);
                }

                var instance = Activator.CreateInstance(type);
                object activity = null;

                if(instance is Activity)
                {
                    activity = instance as Activity;
                    (instance as Activity).DisplayName = name;

                    if(assemblyQualifiedName.Contains("ForEachWithBodyFactory"))
                    {
                        dynamic obj = instance;
                        obj.Body = new ActivityAction<object>
                        {
                            Argument = new DelegateInArgument<object>
                            {
                                Name = "item"
                            },
                            Handler = new Sequence
                            {
                                DisplayName = Chainbot.Resources.Properties.Resources.Body_DiaplayName
                            }
                        };
                    }else if (assemblyQualifiedName.Contains("ParallelForEachWithBodyFactory"))
                    {
                        dynamic obj = instance;
                        obj.Body = new ActivityAction<object>
                        {
                            Argument = new DelegateInArgument<object>()
                            {
                                Name = "item"
                            }
                        };
                    }
                }
                else if(instance is FlowSwitch<object>)
                {
                    activity = instance as FlowSwitch<object>;
                    (instance as FlowSwitch<object>).DisplayName = name;
                }
                else
                {
                    return;
                }
               

                var selectedModelItem = this._designer.Context.Items.GetValue<Selection>().PrimarySelection;

                ModelItem modelItem = selectedModelItem ?? rootModelItem;

                if (modelItem != null)
                {
                    int insertIndex;
                    ModelItem targetInsertSequence = modelItem.GetTargetInsertSequence(out insertIndex, null);
                    if (targetInsertSequence == null)
                    {
                        var modelItem2 = (modelItem.GetParentInsertContainer()?.GetTargetInsertSequence(out insertIndex, modelItem) ?? modelItem);

                        int? num;
                        if (modelItem2 == null)
                        {
                            num = null;
                        }
                        else
                        {
                            ModelProperty modelProperty = modelItem2.Properties["Activities"];
                            if (modelProperty == null)
                            {
                                num = null;
                            }
                            else
                            {
                                ModelItemCollection collection = modelProperty.Collection;
                                num = ((collection != null) ? new int?(collection.IndexOf(modelItem)) : null);
                            }
                        }
                        int? num2 = num;
                        if (num2 != null)
                        {
                            int valueOrDefault = num2.GetValueOrDefault();
                            if (valueOrDefault > -1)
                            {
                                insertIndex = valueOrDefault + 1;
                            }
                        }

                        if(modelItem2 != null)
                        {
                            modelItem2.AddActivity(activity, insertIndex);
                        }
                        else
                        {
                            rootModelItem.AddActivity(activity, insertIndex);
                        }
                    }
                    else
                    {
                        targetInsertSequence.AddActivity(activity, insertIndex);
                    }

                }
            }
        }

        private void HideDesignerSystemButtons()
        {
            var designerView = _designer.Context.Services.GetService<DesignerView>();

            var shellBar = designerView.FindName("shellBar") as FrameworkElement;
            shellBar.Visibility = Visibility.Collapsed;

            var expandAllButton = designerView.FindName("expandAllButton") as FrameworkElement;
            expandAllButton.Visibility = Visibility.Collapsed;
            var collapseAllButton = designerView.FindName("collapseAllButton") as FrameworkElement;
            collapseAllButton.Visibility = Visibility.Collapsed;
        }


        private void SetDesignerFocus()
        {
            var designerView = _designer.Context.Services.GetService<DesignerView>();
            designerView.Focus();
        }

        public void ResetZoom()
        {
            SetDesignerFocus();
            DesignerView.ResetZoomCommand.Execute(null);
        }

        public void ZoomIn()
        {
            SetDesignerFocus();
            DesignerView.ZoomInCommand.Execute(null);
        }

        public void ZoomOut()
        {
            SetDesignerFocus();
            DesignerView.ZoomOutCommand.Execute(null);
        }

        public void ToggleMiniMap()
        {
            SetDesignerFocus();
            DesignerView.ToggleMiniMapCommand.Execute(null);
        }

        public void FitToScreen()
        {
            SetDesignerFocus();
            DesignerView.FitToScreenCommand.Execute(null);
        }

        public void ExpandAll()
        {
            SetDesignerFocus();
            var designerView = _designer.Context.Services.GetService<DesignerView>();
            if (!designerView.ShouldExpandAll)
            {
                DesignerView.ExpandAllCommand.Execute(null);
            }
            else
            {
                DesignerView.RestoreCommand.Execute(null);
            }
        }

        public void CollapseAl()
        {
            SetDesignerFocus();
            var designerView = _designer.Context.Services.GetService<DesignerView>();
            if (!designerView.ShouldCollapseAll)
            {
                DesignerView.CollapseAllCommand.Execute(null);
            }
            else
            {
                DesignerView.RestoreCommand.Execute(null);
            }
        }

        public void ToggleVariableDesigner()
        {
            SetDesignerFocus();
            DesignerView.ToggleVariableDesignerCommand.Execute(null);
        }

        public void ToggleArgumentDesigne()
        {
            SetDesignerFocus();
            DesignerView.ToggleArgumentDesignerCommand.Execute(null);
        }

        public void ToggleImportsDesigner()
        {
            SetDesignerFocus();
            DesignerView.ToggleImportsDesignerCommand.Execute(null);
        }

    }
}
