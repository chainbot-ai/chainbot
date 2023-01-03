using System;
using System.Activities.Presentation;
using System.AddIn.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerService
    {
        event EventHandler ModelChangedEvent;
        event EventHandler CanExecuteChanged;
        event EventHandler<string> ModelAddedEvent;

        string Path { get; }

        string XamlText { get;}

        void Init(string path);

        void UpdatePath(string path);

        INativeHandleContract GetDesignerView();
        INativeHandleContract GetPropertyView();
        INativeHandleContract GetOutlineView();

        void Save();

        bool CanUndo();
        bool CanRedo();
        bool CanCut();
        bool CanCopy();
        bool CanPaste();
        bool CanDelete();


        void Undo();
        void Redo();
        void Cut();
        void Copy();
        void Paste();
        void Delete();

        WorkflowDesigner GetWorkflowDesigner();
        
        void ShowCurrentLocation(string locationId);
        void ShowCurrentLocation(string locationId, string workflowFilePath);
        void HideCurrentLocation();
        void HideCurrentLocation(string workflowFilePath);

        string GetActivityIdJsonArray();
        string GetBreakpointIdJsonArray();
        string GetTrackerVars();

        void ShowBreakpoints();
        void SetReadOnly(bool isReadOnly);
        void FlushDesigner();
        void RefreshArgumentsView();
        void UpdateCurrentSelecteddDesigner();

        string GetAllActivities();
        string GetAllVariables();
        string GetAllArguments();

        void FocusActivity(string idRef);
        void FocusVariable(string variableName, string idRef);
        void FocusArgument(string argumentName);

        bool CheckUnusedVariables();
        bool CheckUnusedArguments();

        string GetUnusedVariables();
        string GetUnusedArguments();

        string GetUsedVariables();
        string GetUsedArguments();

        string GetUnsetOutArgumentActivities();

        string GetAbnormalActivities();

        string GetErrLocationActivities();

        void RemoveUnusedVariables();
        void RemoveUnusedArguments();

        string GetXamlValidInfo();

        void InsertActivity(string name, string assemblyQualifiedName);

        void ResetZoom();
        void ZoomIn();
        void ZoomOut();
        void ToggleMiniMap();
        void FitToScreen();
        void ExpandAll();
        void CollapseAl();
        void ToggleVariableDesigner();
        void ToggleArgumentDesigne();
        void ToggleImportsDesigner();
    }
}
