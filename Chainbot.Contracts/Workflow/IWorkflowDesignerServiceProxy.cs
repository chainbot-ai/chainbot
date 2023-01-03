using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Chainbot.Contracts.Workflow
{
    public interface IWorkflowDesignerServiceProxy
    {
        event EventHandler ModelChangedEvent;
        event EventHandler CanExecuteChanged;
        event EventHandler<string> ModelAddedEvent;

        string XamlText { get;}

        void Init(string path);

        void UpdatePath(string path);

        FrameworkElement GetDesignerView();
        FrameworkElement GetPropertyView();
        FrameworkElement GetOutlineView();

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

        void ShowCurrentLocation(string locationId);
        void ShowCurrentLocation(string locationId, string workflowFilePath);
        void HideCurrentLocation();
        void HideCurrentLocation(string workflowFilePath);

        void ShowBreakpoints();

        string GetActivityIdJsonArray();
        string GetBreakpointIdJsonArray();
        string GetTrackerVars();
        void SetReadOnly(bool isReadOnly);
        void FlushDesigner();
        void RefreshArgumentsView();
        void UpdateCurrentSelecteddDesigner();

        JArray GetAllActivities();
        JArray GetAllVariables();
        JArray GetAllArguments();

        void FocusActivity(string idRef);
        void FocusVariable(string variableName, string idRef);
        void FocusArgument(string argumentName);

        bool CheckUnusedVariables();
        bool CheckUnusedArguments();

        JArray GetUnusedVariables();
        JArray GetUnusedArguments();

        JArray GetUsedVariables();
        JArray GetUsedArguments();

        JArray GetUnsetOutArgumentActivities();

        JArray GetAbnormalActivities();

        JArray GetErrLocationActivities();

        void RemoveUnusedVariables();
        void RemoveUnusedArguments();

        JArray GetXamlValidInfo();

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
