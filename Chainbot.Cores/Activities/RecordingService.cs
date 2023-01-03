using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Project;
using Chainbot.Contracts.Workflow;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Activities
{
    public class RecordingService : MarshalByRefServiceBase, IRecordingService
    {
        private IWorkflowDesignerCollectService _workflowDesignerCollectService;
        private IProjectManagerService _projectManagerService;

        private static int Order = 1;

        public event EventHandler BeginEvent;
        public event EventHandler EndEvent;

        public event EventHandler RecordEvent;

        public event EventHandler SaveEvent;

        public bool IsRecordingWindowOpened
        {
            get
            {
                return UiElement.IsRecordingWindowOpened;
            }

            set
            {
                UiElement.IsRecordingWindowOpened = value;
            }
        }

        public RecordingService(IWorkflowDesignerCollectService workflowDesignerCollectService, IProjectManagerService projectManagerService)
        {
            _workflowDesignerCollectService = workflowDesignerCollectService;
            _projectManagerService = projectManagerService;
        }



        struct stuActivityInfo
        {
            public Activity activity;
            public Action<Activity> postAction;
        }


        private List<stuActivityInfo> _activityRecordingList = new List<stuActivityInfo>();

        private void DoMouseSelect(UiElement uiElement, string type, Action<object> action = null)
        {
            RecordEvent?.Invoke(this, EventArgs.Empty);
            EndEvent?.Invoke(this, EventArgs.Empty);

            var typeStr = $"Chainbot.UIAutomation.Activities.Mouse.{type},Chainbot.UIAutomation.Activities";
            Type activityType = Type.GetType(typeStr);
            if(activityType == null)
            {
                return;
            }

            dynamic activity = Activator.CreateInstance(activityType);
            action?.Invoke(activity);
            activity.SourceImgPath = uiElement.CaptureInformativeScreenshotToFile();
            activity.Selector = uiElement.Selector;
            activity.visibility = System.Windows.Visibility.Visible;
            activity.offsetX = uiElement.GetClickablePoint().X;
            activity.offsetY = uiElement.GetClickablePoint().Y;

            activity.Left = uiElement.BoundingRectangle.Left;
            activity.Right = uiElement.BoundingRectangle.Right;
            activity.Top = uiElement.BoundingRectangle.Top;
            activity.Bottom = uiElement.BoundingRectangle.Bottom;

            var append_displayName = " \"" + uiElement.ProcessName + " " + uiElement.Name + "\"";

            stuActivityInfo info = new stuActivityInfo();
            info.activity = activity;
            info.postAction = (activityItem) =>
            {
                activityItem.DisplayName = _projectManagerService.ActivitiesTypeOfDict[typeStr].Name + append_displayName;
            };
            _activityRecordingList.Add(info);

            
        }

        private void UiElement_OnMouseLeftClickSelected(UiElement uiElement)
        {
            DoMouseSelect(uiElement, "ClickActivity", (activity) =>
            {
                dynamic _activity = activity;
                _activity.SetMouseLeftClick();
            });

            uiElement.MouseClick(null);
        }


        private void UiElement_OnMouseRightClickSelected(UiElement uiElement)
        {
            DoMouseSelect(uiElement, "ClickActivity", (activity) =>
            {
                dynamic _activity = activity;
                _activity.SetMouseRightClick();
            });

            uiElement.MouseRightClick(null);

        }

        private void UiElement_OnMouseDoubleLeftClickSelected(UiElement uiElement)
        {
            DoMouseSelect(uiElement, "DoubleClickActivity", (activity) =>
            {
                dynamic _activity = activity;
                _activity.SetMouseDoubleLeftClick();
            });

            uiElement.MouseDoubleClick(null);

        }

        private void UiElement_OnMouseHoverSelected(UiElement uiElement)
        {
            DoMouseSelect(uiElement, "HoverClickActivity");

            uiElement.MouseHover(null);
        }


        private void UiElement_OnKeyboardInputSelected(UiElement uiElement)
        {
            RecordEvent?.Invoke(this, EventArgs.Empty);
            EndEvent?.Invoke(this, EventArgs.Empty);

            var typeStr = "Chainbot.UIAutomation.Activities.Keyboard.TypeIntoActivity,Chainbot.UIAutomation.Activities";
            Type activityType = Type.GetType(typeStr);
            if (activityType == null)
            {
                return;
            }

            dynamic activity = Activator.CreateInstance(activityType);
            activity.SourceImgPath = uiElement.CaptureInformativeScreenshotToFile();
            activity.Selector = uiElement.Selector;
            activity.visibility = System.Windows.Visibility.Visible;
            activity.offsetX = uiElement.GetClickablePoint().X;
            activity.offsetY = uiElement.GetClickablePoint().Y;

            var append_displayName = " \"" + uiElement.ProcessName + " " + uiElement.Name + "\"";

            stuActivityInfo info = new stuActivityInfo();
            info.activity = activity;
            info.postAction = (activityItem) =>
            {
                activityItem.DisplayName = _projectManagerService.ActivitiesTypeOfDict[typeStr].Name + append_displayName;
            };
            _activityRecordingList.Add(info);
        }


        private void UiElement_OnKeyboardHotKeySelected(UiElement uiElement)
        {
            RecordEvent?.Invoke(this, EventArgs.Empty);
            EndEvent?.Invoke(this, EventArgs.Empty);

            var typeStr = "Chainbot.UIAutomation.Activities.Keyboard.HotKeyActivity,Chainbot.UIAutomation.Activities";
            Type activityType = Type.GetType(typeStr);
            if (activityType == null)
            {
                return;
            }

            dynamic activity = Activator.CreateInstance(activityType);
            activity.SourceImgPath = uiElement.CaptureInformativeScreenshotToFile();
            activity.Selector = uiElement.Selector;
            activity.visibility = System.Windows.Visibility.Visible;
            activity.offsetX = uiElement.GetClickablePoint().X;
            activity.offsetY = uiElement.GetClickablePoint().Y;

            var append_displayName = " \"" + uiElement.ProcessName + " " + uiElement.Name + "\"";

            stuActivityInfo info = new stuActivityInfo();
            info.activity = activity;
            info.postAction = (activityItem) =>
            {
                activityItem.DisplayName = _projectManagerService.ActivitiesTypeOfDict[typeStr].Name + append_displayName;
            };
            _activityRecordingList.Add(info);
        }


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public void MouseLeftClick()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnMouseLeftClickSelected;
            UiElement.StartElementHighlight();
        }

        public void MouseRightClick()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnMouseRightClickSelected;
            UiElement.StartElementHighlight();
        }

        public void MouseDoubleLeftClick()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnMouseDoubleLeftClickSelected;
            UiElement.StartElementHighlight();
        }

        public void MouseHover()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnMouseHoverSelected;
            UiElement.StartElementHighlight();
        }

       

        public void KeyboardInput()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnKeyboardInputSelected;
            UiElement.StartElementHighlight();
        }

        public void KeyboardHotKey()
        {
            BeginEvent?.Invoke(this, EventArgs.Empty);

            UiElement.OnSelected = UiElement_OnKeyboardHotKeySelected;
            UiElement.StartElementHighlight();
        }


        public void Save(string path)
        {
            WorkflowDesigner wd = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();

            if(wd != null)
            {
                ModelService modelService = wd.Context.Services.GetService<ModelService>();
                ModelItem rootModelItem = modelService.Root.Properties["Implementation"].Value;
               
                var seq = new Sequence() { DisplayName = "录制序列#"+(Order++).ToString() };

                foreach (var item in _activityRecordingList)
                {
                    seq.Activities.Add(item.activity);
                    item.postAction?.Invoke(item.activity);
                }

                if (rootModelItem == null)
                {
                    modelService.Root.Content.SetValue(seq);
                }
                else
                {
                    rootModelItem.AddActivity(seq);
                }

                SaveEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
