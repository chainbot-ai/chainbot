using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using ReflectionMagic;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Presentation.ViewState;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using Chainbot.Cores.Classes;
using Chainbot.Cores.CodeAnalysis;
using Chainbot.Contracts.JumpToChild;
using Chainbot.Cores.JumpToChild;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowActivityService : MarshalByRefServiceBase, IWorkflowActivityService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ILogService _logService;
        private IWorkflowDesignerCollectService _workflowDesignerCollectService;
        private ICommonService _commonService;


        private enum VirtualKey : ushort
        {
            //
            // 摘要:
            //     Left mouse button
            LBUTTON = 1,
            //
            // 摘要:
            //     Right mouse button
            RBUTTON = 2,
            //
            // 摘要:
            //     Control-break processing
            CANCEL = 3,
            //
            // 摘要:
            //     Middle mouse button (three-button mouse)
            MBUTTON = 4,
            //
            // 摘要:
            //     Windows 2000/XP: X1 mouse button
            XBUTTON1 = 5,
            //
            // 摘要:
            //     Windows 2000/XP: X2 mouse button
            XBUTTON2 = 6,
            //
            // 摘要:
            //     BACKSPACE key
            BACK = 8,
            //
            // 摘要:
            //     TAB key
            TAB = 9,
            //
            // 摘要:
            //     CLEAR key
            CLEAR = 12,
            //
            // 摘要:
            //     ENTER key
            ENTER = 13,
            RERETURN = 13,
            //
            // 摘要:
            //     SHIFT key
            SHIFT = 16,
            //
            // 摘要:
            //     CTRL key
            CONTROL = 17,
            //
            // 摘要:
            //     ALT key
            ALT = 18,
            //
            // 摘要:
            //     PAUSE key
            PAUSE = 19,
            //
            // 摘要:
            //     CAPS LOCK key
            // CAPITAL = 20,
            CAPSLOCK = 20,
            //
            // 摘要:
            //     Input Method Editor (IME) Kana mode
            KANA = 21,
            //
            // 摘要:
            //     IME Hangul mode
            HANGUL = 21,
            //
            // 摘要:
            //     IME Junja mode
            JUNJA = 23,
            //
            // 摘要:
            //     IME final mode
            FINAL = 24,
            //
            // 摘要:
            //     IME Hanja mode
            HANJA = 25,
            //
            // 摘要:
            //     IME Kanji mode
            KANJI = 25,
            //
            // 摘要:
            //     ESC key
            //ESCAPE = 27,
            ESC = 27,
            //
            // 摘要:
            //     IME convert
            CONVERT = 28,
            //
            // 摘要:
            //     IME nonconvert
            NONCONVERT = 29,
            //
            // 摘要:
            //     IME accept
            ACCEPT = 30,
            //
            // 摘要:
            //     IME mode change request
            MODECHANGE = 31,
            //
            // 摘要:
            //     SPACEBAR
            SPACE = 32,
            //
            // 摘要:
            //     PAGE UP key
            PAGE_UP = 33,
            //
            // 摘要:
            //     PAGE DOWN key
            PAGE_DOWN = 34,
            //
            // 摘要:
            //     END key
            END = 35,
            //
            // 摘要:
            //     HOME key
            HOME = 36,
            //
            // 摘要:
            //     LEFT ARROW key
            LEFT = 37,
            //
            // 摘要:
            //     UP ARROW key
            UP = 38,
            //
            // 摘要:
            //     RIGHT ARROW key
            RIGHT = 39,
            //
            // 摘要:
            //     DOWN ARROW key
            DOWN = 40,
            //
            // 摘要:
            //     SELECT key
            SELECT = 41,
            //
            // 摘要:
            //     PRINT key
            PRINT = 42,
            //
            // 摘要:
            //     EXECUTE key
            EXECUTE = 43,
            //
            // 摘要:
            //     PRINT SCREEN key
            SNAPSHOT = 44,
            //
            // 摘要:
            //     INS key
            INSERT = 45,
            //
            // 摘要:
            //     DEL key
            DELETE = 46,
            //
            // 摘要:
            //     HELP key
            HELP = 47,
            //
            // 摘要:
            //     0 key
            KEY_0 = 48,
            //
            // 摘要:
            //     1 key
            KEY_1 = 49,
            //
            // 摘要:
            //     2 key
            KEY_2 = 50,
            //
            // 摘要:
            //     3 key
            KEY_3 = 51,
            //
            // 摘要:
            //     4 key
            KEY_4 = 52,
            //
            // 摘要:
            //     5 key
            KEY_5 = 53,
            //
            // 摘要:
            //     6 key
            KEY_6 = 54,
            //
            // 摘要:
            //     7 key
            KEY_7 = 55,
            //
            // 摘要:
            //     8 key
            KEY_8 = 56,
            //
            // 摘要:
            //     9 key
            KEY_9 = 57,
            //
            // 摘要:
            //     A key
            KEY_A = 65,
            //
            // 摘要:
            //     B key
            KEY_B = 66,
            //
            // 摘要:
            //     C key
            KEY_C = 67,
            //
            // 摘要:
            //     D key
            KEY_D = 68,
            //
            // 摘要:
            //     E key
            KEY_E = 69,
            //
            // 摘要:
            //     F key
            KEY_F = 70,
            //
            // 摘要:
            //     G key
            KEY_G = 71,
            //
            // 摘要:
            //     H key
            KEY_H = 72,
            //
            // 摘要:
            //     I key
            KEY_I = 73,
            //
            // 摘要:
            //     J key
            KEY_J = 74,
            //
            // 摘要:
            //     K key
            KEY_K = 75,
            //
            // 摘要:
            //     L key
            KEY_L = 76,
            //
            // 摘要:
            //     M key
            KEY_M = 77,
            //
            // 摘要:
            //     N key
            KEY_N = 78,
            //
            // 摘要:
            //     O key
            KEY_O = 79,
            //
            // 摘要:
            //     P key
            KEY_P = 80,
            //
            // 摘要:
            //     Q key
            KEY_Q = 81,
            //
            // 摘要:
            //     R key
            KEY_R = 82,
            //
            // 摘要:
            //     S key
            KEY_S = 83,
            //
            // 摘要:
            //     T key
            KEY_T = 84,
            //
            // 摘要:
            //     U key
            KEY_U = 85,
            //
            // 摘要:
            //     V key
            KEY_V = 86,
            //
            // 摘要:
            //     W key
            KEY_W = 87,
            //
            // 摘要:
            //     X key
            KEY_X = 88,
            //
            // 摘要:
            //     Y key
            KEY_Y = 89,
            //
            // 摘要:
            //     Z key
            KEY_Z = 90,
            //
            // 摘要:
            //     Left Windows key (Microsoft Natural keyboard)
            LWIN = 91,
            //
            // 摘要:
            //     Right Windows key (Natural keyboard)
            RWIN = 92,
            //
            // 摘要:
            //     Applications key (Natural keyboard)
            APPS = 93,
            //
            // 摘要:
            //     Computer Sleep key
            SLEEP = 95,
            //
            // 摘要:
            //     Numeric keypad 0 key
            NUMPAD0 = 96,
            //
            // 摘要:
            //     Numeric keypad 1 key
            NUMPAD1 = 97,
            //
            // 摘要:
            //     Numeric keypad 2 key
            NUMPAD2 = 98,
            //
            // 摘要:
            //     Numeric keypad 3 key
            NUMPAD3 = 99,
            //
            // 摘要:
            //     Numeric keypad 4 key
            NUMPAD4 = 100,
            //
            // 摘要:
            //     Numeric keypad 5 key
            NUMPAD5 = 101,
            //
            // 摘要:
            //     Numeric keypad 6 key
            NUMPAD6 = 102,
            //
            // 摘要:
            //     Numeric keypad 7 key
            NUMPAD7 = 103,
            //
            // 摘要:
            //     Numeric keypad 8 key
            NUMPAD8 = 104,
            //
            // 摘要:
            //     Numeric keypad 9 key
            NUMPAD9 = 105,
            //
            // 摘要:
            //     Multiply key
            MULTIPLY = 106,
            //
            // 摘要:
            //     Add key
            ADD = 107,
            //
            // 摘要:
            //     Separator key
            SEPARATOR = 108,
            //
            // 摘要:
            //     Subtract key
            SUBTRACT = 109,
            //
            // 摘要:
            //     Decimal key
            DECIMAL = 110,
            //
            // 摘要:
            //     Divide key
            DIVIDE = 111,
            //
            // 摘要:
            //     F1 key
            F1 = 112,
            //
            // 摘要:
            //     F2 key
            F2 = 113,
            //
            // 摘要:
            //     F3 key
            F3 = 114,
            //
            // 摘要:
            //     F4 key
            F4 = 115,
            //
            // 摘要:
            //     F5 key
            F5 = 116,
            //
            // 摘要:
            //     F6 key
            F6 = 117,
            //
            // 摘要:
            //     F7 key
            F7 = 118,
            //
            // 摘要:
            //     F8 key
            F8 = 119,
            //
            // 摘要:
            //     F9 key
            F9 = 120,
            //
            // 摘要:
            //     F10 key
            F10 = 121,
            //
            // 摘要:
            //     F11 key
            F11 = 122,
            //
            // 摘要:
            //     F12 key
            F12 = 123,
            //
            // 摘要:
            //     F13 key
            F13 = 124,
            //
            // 摘要:
            //     F14 key
            F14 = 125,
            //
            // 摘要:
            //     F15 key
            F15 = 126,
            //
            // 摘要:
            //     F16 key
            F16 = 127,
            //
            // 摘要:
            //     F17 key
            F17 = 128,
            //
            // 摘要:
            //     F18 key
            F18 = 129,
            //
            // 摘要:
            //     F19 key
            F19 = 130,
            //
            // 摘要:
            //     F20 key
            F20 = 131,
            //
            // 摘要:
            //     F21 key
            F21 = 132,
            //
            // 摘要:
            //     F22 key, (PPC only) Key used to lock device.
            F22 = 133,
            //
            // 摘要:
            //     F23 key
            F23 = 134,
            //
            // 摘要:
            //     F24 key
            F24 = 135,
            //
            // 摘要:
            //     NUM LOCK key
            NUMLOCK = 144,
            //
            // 摘要:
            //     SCROLL LOCK key
            SCROLL = 145,
            //
            // 摘要:
            //     Left SHIFT key
            LSHIFT = 160,
            //
            // 摘要:
            //     Right SHIFT key
            RSHIFT = 161,
            //
            // 摘要:
            //     Left CONTROL key
            LCONTROL = 162,
            //
            // 摘要:
            //     Right CONTROL key
            RCONTROL = 163,
            //
            // 摘要:
            //     Left MENU key
            LMENU = 164,
            //
            // 摘要:
            //     Right MENU key
            RMENU = 165,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Back key
            BROWSER_BACK = 166,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Forward key
            BROWSER_FORWARD = 167,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Refresh key
            BROWSER_REFRESH = 168,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Stop key
            BROWSER_STOP = 169,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Search key
            BROWSER_SEARCH = 170,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Favorites key
            BROWSER_FAVORITES = 171,
            //
            // 摘要:
            //     Windows 2000/XP: Browser Start and Home key
            BROWSER_HOME = 172,
            //
            // 摘要:
            //     Windows 2000/XP: Volume Mute key
            VOLUME_MUTE = 173,
            //
            // 摘要:
            //     Windows 2000/XP: Volume Down key
            VOLUME_DOWN = 174,
            //
            // 摘要:
            //     Windows 2000/XP: Volume Up key
            VOLUME_UP = 175,
            //
            // 摘要:
            //     Windows 2000/XP: Next Track key
            MEDIA_NEXT_TRACK = 176,
            //
            // 摘要:
            //     Windows 2000/XP: Previous Track key
            MEDIA_PREV_TRACK = 177,
            //
            // 摘要:
            //     Windows 2000/XP: Stop Media key
            MEDIA_STOP = 178,
            //
            // 摘要:
            //     Windows 2000/XP: Play/Pause Media key
            MEDIA_PLAY_PAUSE = 179,
            //
            // 摘要:
            //     Windows 2000/XP: Start Mail key
            LAUNCH_MAIL = 180,
            //
            // 摘要:
            //     Windows 2000/XP: Select Media key
            LAUNCH_MEDIA_SELECT = 181,
            //
            // 摘要:
            //     Windows 2000/XP: Start Application 1 key
            LAUNCH_APP1 = 182,
            //
            // 摘要:
            //     Windows 2000/XP: Start Application 2 key
            LAUNCH_APP2 = 183,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_1 = 186,
            //
            // 摘要:
            //     Windows 2000/XP: For any country/region, the '+' key
            OEM_PLUS = 187,
            //
            // 摘要:
            //     Windows 2000/XP: For any country/region, the ',' key
            OEM_COMMA = 188,
            //
            // 摘要:
            //     Windows 2000/XP: For any country/region, the '-' key
            OEM_MINUS = 189,
            //
            // 摘要:
            //     Windows 2000/XP: For any country/region, the '.' key
            OEM_PERIOD = 190,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_2 = 191,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_3 = 192,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_4 = 219,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_5 = 220,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_6 = 221,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_7 = 222,
            //
            // 摘要:
            //     Used for miscellaneous characters; it can vary by keyboard.
            OEM_8 = 223,
            //
            // 摘要:
            //     Windows 2000/XP: Either the angle bracket key or the backslash key on the RT
            //     102-key keyboard
            OEM_102 = 226,
            //
            // 摘要:
            //     Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            PROCESSKEY = 229,
            //
            // 摘要:
            //     Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
            //     The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard
            //     input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN,
            //     and WM_KEYUP
            PACKET = 231,
            //
            // 摘要:
            //     Attn key
            ATTN = 246,
            //
            // 摘要:
            //     CrSel key
            CRSEL = 247,
            //
            // 摘要:
            //     ExSel key
            EXSEL = 248,
            //
            // 摘要:
            //     Erase EOF key
            EREOF = 249,
            //
            // 摘要:
            //     Play key
            PLAY = 250,
            //
            // 摘要:
            //     Zoom key
            ZOOM = 251,
            //
            // 摘要:
            //     Reserved
            NONAME = 252,
            //
            // 摘要:
            //     PA1 key
            PA1 = 253,
            //
            // 摘要:
            //     Clear key
            OEM_CLEAR = 254
        }


        public WorkflowActivityService(ILogService logService, IWorkflowDesignerCollectService workflowDesignerCollectService, ICommonService commonService)
        {
            _logService = logService;
            _workflowDesignerCollectService = workflowDesignerCollectService;
            _commonService = commonService;
        }

        public ModelItem GetSelectedModelItem(string path)
        {
            WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
            if (workflowDesigner != null)
            {
                return workflowDesigner.Context.Items.GetValue<Selection>()?.PrimarySelection;
            }
            else
            {
                return null;
            }        
        }

        public void SurroundWithTryCatch(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);
            using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
            {
                ModelItem modelItem = ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), new TryCatch() { DisplayName = "异常处理" });
                MorphHelper.MorphObject(selectedModelItem, modelItem);
                modelItem.Properties["Try"].SetValue(selectedModelItem.GetCurrentValue());
                modelEditingScope.Complete();
            }
        }


        public bool IsActivityDisabled(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);
            if(selectedModelItem == null)
            {
                return false;
            }

            var typeStr = "Chainbot.Core.Activities.DebugActivity.CommentOutActivity,Chainbot.Core.Activities";
            Type activityType = Type.GetType(typeStr);
            if (activityType == null)
            {
                return false;
            }

            dynamic activity = Activator.CreateInstance(activityType);

            var sel = selectedModelItem != null ? selectedModelItem.GetCurrentValue() : null;
            return sel?.GetType().Equals(activity.GetType());
        }


        public void RemoveTryCatchActivity(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);

            dynamic tryCatchActivity = selectedModelItem.GetCurrentValue();

            if(tryCatchActivity.Try != null)
            {
                using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), tryCatchActivity.Try));
                    modelEditingScope.Complete();
                }
            }
        }


        public void EnableActivity(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);

            dynamic commentOut = selectedModelItem.GetCurrentValue();
            Sequence sequence = commentOut.Body as Sequence;
            if (sequence == null)
            {
                using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), commentOut.Body));
                    modelEditingScope.Complete();
                }
                return;
            }
            if (sequence.Activities.Count == 1)
            {
                using (ModelEditingScope modelEditingScope2 = selectedModelItem.BeginEdit())
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), sequence.Activities[0]));
                    modelEditingScope2.Complete();
                }
                return;
            }
            try
            {
                using (ModelEditingScope modelEditingScope3 = selectedModelItem.BeginEdit())
                {
                    sequence.DisplayName = Chainbot.Resources.Properties.Resources.SequenceDisplayName;
                    ModelItemCollection collection = selectedModelItem.Parent.Parent.Properties["Activities"].Collection;
                    int index = collection.IndexOf(selectedModelItem);
                    collection.RemoveAt(index);
                    foreach (Activity value in sequence.Activities.Reverse())
                    {
                        collection.Insert(index, value);
                    }
                    modelEditingScope3.Complete();
                }
            }
            catch
            {
                MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), sequence));
            }
        }

        public void DisableActivity(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);

            using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
            {
                var typeStr = "Chainbot.Core.Activities.DebugActivity.CommentOutActivity,Chainbot.Core.Activities";
                Type activityType = Type.GetType(typeStr);
                if (activityType == null)
                {
                    return;
                }

                dynamic activity = Activator.CreateInstance(activityType);
                activity.DisplayName = Chainbot.Resources.Properties.Resources.CommentOutDisplayName;

                (activity.Body as Sequence).Activities.Add(selectedModelItem.GetCurrentValue() as Activity);
                MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), activity));
                modelEditingScope.Complete();
            }
        }


        public string GetSelectedActivityId(ModelItem modelItem)
        {
            return TryGetActivityId(modelItem) ?? GetParentWithId(modelItem);
        }


        public string GetActivityId(ModelItem modelItem)
        {
            Activity activity = ((modelItem != null) ? modelItem.GetCurrentValue() : null) as Activity;
            if (activity == null)
            {
                return null;
            }
            return activity.Id;
        }


        protected string TryGetActivityId(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }
            string text = GetActivityId(modelItem);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
            object currentValue = modelItem.GetCurrentValue();
            FlowDecision flowDecision = currentValue as FlowDecision;
            string text3;
            if (flowDecision != null)
            {
                string text2;
                if (flowDecision == null)
                {
                    text2 = null;
                }
                else
                {
                    Activity<bool> condition = flowDecision.Condition;
                    text2 = ((condition != null) ? condition.Id : null);
                }
                text = text2;
            }
            else if (TryGetFlowSwitchFirstCaseId(currentValue, out text3))
            {
                text = text3;
            }
            else
            {
                State state = currentValue as State;
                if (state != null)
                {
                    Activity entry = state.Entry;
                    string text4;
                    if ((text4 = ((entry != null) ? entry.Id : null)) == null)
                    {
                        Activity exit = state.Exit;
                        text4 = ((exit != null) ? exit.Id : null);
                    }
                    text = text4;
                }
            }
            return text;
        }

        private bool TryGetFlowSwitchFirstCaseId(object element, out string expressionId)
        {
            expressionId = null;
            if (element == null)
            {
                return false;
            }
            if (!element.GetType().IsGenericType || !(element.GetType().GetGenericTypeDefinition() == typeof(FlowSwitch<>)))
            {
                return false;
            }

            dynamic ele = element;
            dynamic obj = ele.Expression;
            expressionId = (string)((obj != null ? obj.Id : null));
            return expressionId != null;
        }


        public string GetParentWithId(ModelItem modelItem)
        {
            ModelItem modelItem2 = modelItem;
            string text;
            do
            {
                modelItem2 = ((modelItem2 != null) ? modelItem2.Parent : null);
                text = ((modelItem2 != null) ? GetActivityId(modelItem2) : null);
            }
            while (text == null && modelItem2 != null);
            return text;
        }



        public void StartRunFromHere(string path)
        {
            ModelItem selectedModelItem = this.GetSelectedModelItem(path);

            IJumpToChildHandler jumpToChildHandler = null;
            var childId = GetSelectedActivityId(selectedModelItem);

            Activity activity = selectedModelItem.GetCurrentValue() as Activity;
            Sequence sequence = activity as Sequence;
            if (sequence == null)
            {
                Flowchart flowchart = activity as Flowchart;
                if (flowchart != null)
                {
                    jumpToChildHandler = new FlowChartJumpHandler(flowchart);
                }
                else
                {
                    
                }
            }
            else
            {
                jumpToChildHandler = new SequenceJumpHandler(sequence);
            }

            jumpToChildHandler?.SetStartActivity(childId);
        }

        public bool CheckUnusedVariables(string path)
        {
            try
            {
                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                if(workflowDesigner == null)
                {
                    return false;
                }

                var modelService = workflowDesigner.GetModelService();

                HashSet<ModelItem> inUseVariables = this.GetInUseVariables(modelService);
                foreach (ModelItem modelItem in modelService.GetVariables())
                {
                    if (!inUseVariables.Contains(modelItem))
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }
        }

        public bool CheckUnusedArguments(string path)
        {
            try
            {
                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                if (workflowDesigner == null)
                {
                    return false;
                }

                var modelService = workflowDesigner.GetModelService();

                IEnumerable<ModelItem> enumerable = this.GetArguments(modelService);

                if (enumerable != null)
                {
                    HashSet<ModelItem> inUseArguments = this.GetInUseArguments(modelService, enumerable);
                    foreach (ModelItem item in enumerable)
                    {
                        if (!inUseArguments.Contains(item))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }
        }

        public string GetUnusedVariables(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                HashSet<ModelItem> inUseVariables = this.GetInUseVariables(modelService);
                foreach (ModelItem modelItem in modelService.GetVariables())
                {
                    if (!inUseVariables.Contains(modelItem))
                    {
                        var varItem = (modelItem.GetCurrentValue()) as Variable;
                        var activity = varItem.AsDynamic().Owner.RealObject as Activity;

                        var jObj = new JObject();
                        jObj["DisplayName"] = varItem.Name;
                        jObj["IdRef"] = WorkflowViewState.GetIdRef(activity);
                        jObj["Path"] = path;
                        jObj["Location"] = location;
                        retArr.Add(jObj);
                    }
                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public string GetUnusedArguments(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                IEnumerable<ModelItem> enumerable = this.GetArguments(modelService);
                HashSet<ModelItem> inUseArguments = this.GetInUseArguments(modelService, enumerable);
                foreach (ModelItem modelItem in enumerable)
                {
                    if (!inUseArguments.Contains(modelItem))
                    {
                        var jObj = new JObject();
                        jObj["DisplayName"] = GetName(modelItem);
                        jObj["Path"] = path;
                        jObj["Location"] = location;
                        retArr.Add(jObj);
                    }
                }

                return JsonConvert.SerializeObject(retArr);

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public string GetUsedVariables(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
                foreach (ModelItem activity in activities)
                {
                    if (activity.IsActivity() && !activity.IsSequence())
                    {
                        List<InvokableArgument> usedVariables = this.GetUsedVariablesFromActivity(activity);
                        if (usedVariables.Count != 0)
                        {
                            IEnumerable<ModelItem> variables = (from v in this.GetVariablesInScope(activity)
                                                                where usedVariables.Any((InvokableArgument u) => u.Name.Equals(v.GetName(), StringComparison.OrdinalIgnoreCase))
                                                                select v);
                            if (variables != null && variables.Count() > 0)
                            {
                                var val = activity.GetCurrentValue();
                                var activityIdRef = WorkflowViewState.GetIdRef(val);
                                var activityName = (val as Activity).DisplayName;
                                var locationInfo = location + " > " + activityName;

                                JArray variableJarray = new JArray();
                                foreach (ModelItem variable in variables)
                                {
                                    var varItem = (variable.GetCurrentValue()) as Variable;
                                    if (varItem == null)
                                    {
                                        continue;
                                    }

                                    var jObj = new JObject();
                                    jObj["IdRef"] = activityIdRef;
                                    jObj["Path"] = path;
                                    jObj["Location"] = locationInfo;  
                                    jObj["DisplayName"] = varItem.Name;
                                    retArr.Add(jObj);
                                }
                            }
                        }
                    }
                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public string GetUsedArguments(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                IEnumerable<ModelItem> allArguments = this.GetArguments(modelService);

                var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
                foreach (ModelItem activity in activities)
                {
                    if (activity.IsActivity() && !activity.IsSequence())
                    {
                        List<InvokableArgument> usedArguments = this.GetUsedVariablesFromActivity(activity);
                        if (usedArguments.Count != 0)
                        {
                            IEnumerable<ModelItem> arguments = (from v in allArguments
                                                                where usedArguments.Any((InvokableArgument u) => u.Name.Equals(v.GetName(), StringComparison.OrdinalIgnoreCase))
                                                                select v);

                            if (arguments != null && arguments.Count() > 0)
                            {
                                var val = activity.GetCurrentValue();
                                var activityIdRef = WorkflowViewState.GetIdRef(val);
                                var activityName = (val as Activity).DisplayName;
                                var locationInfo = location + " > " + activityName;

                                JArray variableJarray = new JArray();
                                foreach (ModelItem argument in arguments)
                                {
                                    var jObj = new JObject();
                                    jObj["IdRef"] = activityIdRef;
                                    jObj["Path"] = path;
                                    jObj["Location"] = locationInfo;
                                    jObj["DisplayName"] = GetName(argument);
                                    retArr.Add(jObj);
                                }
                            }
                        }
                    }
                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        private string GetName(ModelItem modelItem)
        {
            object obj;
            if (modelItem == null)
            {
                obj = null;
            }
            else
            {
                ModelPropertyCollection properties = modelItem.Properties;
                if (properties == null)
                {
                    obj = null;
                }
                else
                {
                    ModelProperty modelProperty = properties["Name"];
                    obj = ((modelProperty != null) ? modelProperty.ComputedValue : null);
                }
            }
            return obj as string;
        }


        public string GetUnsetOutArgumentActivities(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();
                var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
                foreach (ModelItem modelItem in activities)
                {
                    ModelPropertyCollection modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                    if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                    {
                        foreach (ModelProperty modelProperty in modelPropertyCollection)
                        {
                            if (modelProperty.PropertyType.BaseType.Equals(typeof(OutArgument)) && !modelProperty.IsSet)
                            {    
                                var jObj = new JObject();
                                var val = modelItem.GetCurrentValue();
                                jObj["DisplayName"] = (val as Activity).DisplayName;
                                jObj["ArgumentName"] = modelProperty.Name;
                                jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                jObj["Path"] = path;
                                jObj["Location"] = location;
                                retArr.Add(jObj);
                            }
                        }
                    }  
                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public string GetAbnormalActivities(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();
                var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);

                ModelPropertyCollection modelPropertyCollection = null;
                ModelProperty modelProperty = null; 
                foreach (ModelItem modelItem in activities)
                {
                    if (modelItem.ItemType.Name == "KillProcessActivity")
                    {
                        modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                        if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                        {
                            modelProperty = modelPropertyCollection.Find("ProcessName");
                            if (modelProperty.IsSet)
                            {
                                try
                                {
                                    var argument = modelProperty.Value.GetCurrentValue() as Argument;
                                    var expression = WorkflowInvoker.Invoke(argument.Expression);
                                    var value = expression["Result"].ToString();

                                    if (System.IO.Path.GetExtension(value) != "" && System.IO.Path.GetExtension(value).ToLower() != ".exe")
                                    {
                                        var jObj = new JObject();
                                        var val = modelItem.GetCurrentValue();
                                        jObj["Type"] = "1";
                                        jObj["DisplayName"] = (val as Activity).DisplayName;
                                        jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                        jObj["Path"] = path;
                                        jObj["Location"] = location;
                                        retArr.Add(jObj);
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                        }
                    }

                    else if (modelItem.ItemType.Name == "Delay")
                    {
                        modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                        if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                        {
                            modelProperty = modelPropertyCollection.Find("Duration");
                            if (modelProperty.IsSet)
                            {
                                try
                                {
                                    var argument = modelProperty.Value.GetCurrentValue() as Argument;
                                    var expression = WorkflowInvoker.Invoke(argument.Expression);
                                    TimeSpan value = TimeSpan.Parse(expression["Result"].ToString());

                                    if (value.TotalSeconds >= 1* 60 * 60)
                                    {
                                        var jObj = new JObject();
                                        var val = modelItem.GetCurrentValue();
                                        jObj["Type"] = "2";
                                        jObj["DisplayName"] = (val as Activity).DisplayName;
                                        jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                        jObj["Path"] = path;
                                        jObj["Location"] = location;
                                        retArr.Add(jObj);
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                        }
                    }

                    else if (modelItem.ItemType.Name == "HotKeyActivity")
                    {
                        modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                        if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                        {
                            modelProperty = modelPropertyCollection.Find("SelectedKey");
                            if (modelProperty.IsSet)
                            {
                                try
                                {
                                    var value = modelProperty.Value.ToString();

                                    bool bResult = false;
                                    foreach (VirtualKey item in Enum.GetValues(typeof(VirtualKey)))
                                    {
                                        if (value == item.ToString())
                                        {
                                            bResult = true;
                                            break;
                                        }
                                    }

                                    if (!bResult)
                                    {
                                        var jObj = new JObject();
                                        var val = modelItem.GetCurrentValue();
                                        jObj["Type"] = "3";
                                        jObj["DisplayName"] = (val as Activity).DisplayName;
                                        jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                        jObj["Path"] = path;
                                        jObj["Location"] = location;
                                        retArr.Add(jObj);
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                        }
                    }

                    else
                    {
                        if (modelItem.ItemType.Name == "AttachBrowser" || modelItem.ItemType.Name.StartsWith("OfflineImage"))
                        {
                            
                            continue;
                        }

                        modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                        if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                        {
                            modelProperty = modelPropertyCollection.Find("Selector");
                            if (modelProperty != null)
                            {
                                if (!modelProperty.IsSet)
                                {
                                    var jObj = new JObject();
                                    var val = modelItem.GetCurrentValue();
                                    jObj["Type"] = "4";
                                    jObj["DisplayName"] = (val as Activity).DisplayName;
                                    jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                    jObj["Path"] = path;
                                    jObj["Location"] = location;
                                    retArr.Add(jObj);
                                }
                                else
                                {
                                    try
                                    {
                                        var argument = modelProperty.Value.GetCurrentValue() as Argument;
                                        var expression = WorkflowInvoker.Invoke(argument.Expression);
                                        var value = expression["Result"].ToString();

                                        if (value == "" || value == "<IENode />" || value == "<ChromeNode />" || value == "<FirefoxNode />")
                                        {
                                            var jObj = new JObject();
                                            var val = modelItem.GetCurrentValue();
                                            jObj["Type"] = "4";
                                            jObj["DisplayName"] = (val as Activity).DisplayName;
                                            jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                            jObj["Path"] = path;
                                            jObj["Location"] = location;
                                            retArr.Add(jObj);
                                        }
                                    }
                                    catch
                                    {
                                        
                                    }
                                }
                            } 
                        }
                    }

                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public string GetErrLocationActivities(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();
                var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
                foreach (ModelItem modelItem in activities)
                {
                    if (modelItem.ItemType.FullName.StartsWith("Chainbot.Integration.Activities.WordPlugins.") && modelItem.ItemType.FullName != "Chainbot.Integration.Activities.WordPlugins.WordCreate")
                    {
                        var parent = modelItem.Parent.Parent.Parent.Parent;

                        if (!(parent.GetCurrentValue() is Activity) || parent.ItemType.FullName != "Chainbot.Integration.Activities.WordPlugins.WordCreate")
                        {
                            var jObj = new JObject();
                            var val = modelItem.GetCurrentValue();
                            jObj["Type"] = "1";
                            jObj["DisplayName"] = (val as Activity).DisplayName;
                            jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                            jObj["Path"] = path;
                            jObj["Location"] = location;
                            retArr.Add(jObj);
                        }
                    }

                    else if (modelItem.ItemType.FullName.StartsWith("Chainbot.Integration.Activities.ExcelPlugins.") && modelItem.ItemType.FullName != "Chainbot.Integration.Activities.ExcelPlugins.ExcelCreate" && !modelItem.ItemType.Name.Contains("CSV"))
                    {
                        var parent = modelItem.Parent.Parent.Parent.Parent;

                        if (!(parent.GetCurrentValue() is Activity) || parent.ItemType.FullName != "Chainbot.Integration.Activities.ExcelPlugins.ExcelCreate")
                        {
                            var jObj = new JObject();
                            var val = modelItem.GetCurrentValue();
                            jObj["Type"] = "2";
                            jObj["DisplayName"] = (val as Activity).DisplayName;
                            jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                            jObj["Path"] = path;
                            jObj["Location"] = location;
                            retArr.Add(jObj);
                        }
                    }

                    else if (modelItem.ItemType.FullName.StartsWith("Chainbot.Integration.Activities.WordPlugins_NPOI") && modelItem.ItemType.FullName != "Chainbot.Integration.Activities.WordPlugins_NPOI.WordCreateNpoi")
                    {
                        var parent = modelItem.Parent.Parent.Parent.Parent;

                        if (!(parent.GetCurrentValue() is Activity) || parent.ItemType.FullName != "Chainbot.Integration.Activities.WordPlugins_NPOI.WordCreateNpoi")
                        {
                            var jObj = new JObject();
                            var val = modelItem.GetCurrentValue();
                            jObj["Type"] = "3";
                            jObj["DisplayName"] = (val as Activity).DisplayName;
                            jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                            jObj["Path"] = path;
                            jObj["Location"] = location;
                            retArr.Add(jObj);
                        }
                    }

                    else if (modelItem.ItemType.FullName.StartsWith("Chainbot.Integration.Activities.ExcelPlugins_Npoi") && modelItem.ItemType.FullName != "Chainbot.Integration.Activities.ExcelPlugins_Npoi.ExcelCreateNpoi")
                    {
                        var parent = modelItem.Parent.Parent.Parent.Parent;

                        if (!(parent.GetCurrentValue() is Activity) || parent.ItemType.FullName != "Chainbot.Integration.Activities.ExcelPlugins_Npoi.ExcelCreateNpoi")
                        {
                            var jObj = new JObject();
                            var val = modelItem.GetCurrentValue();
                            jObj["Type"] = "4";
                            jObj["DisplayName"] = (val as Activity).DisplayName;
                            jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                            jObj["Path"] = path;
                            jObj["Location"] = location;
                            retArr.Add(jObj);
                        }
                    }

                    else if (modelItem.ItemType.FullName.StartsWith("Chainbot.UIAutomation.Activities.Browser")  && modelItem.ItemType.Name != "OpenBrowser" && modelItem.ItemType.Name != "AttachBrowser")
                    {
                        var parent = modelItem.Parent.Parent.Parent.Parent;
                         
                        if (!(parent.GetCurrentValue() is Activity) || (parent.ItemType.Name != "OpenBrowser" && parent.ItemType.Name != "AttachBrowser"))
                        {
                            ModelPropertyCollection modelPropertyCollection = (modelItem != null) ? modelItem.Properties : null;
                            if (modelPropertyCollection != null && modelPropertyCollection.Any<ModelProperty>())
                            {
                                ModelProperty modelProperty = modelPropertyCollection.Find("currBrowser");
                                if (!modelProperty.IsSet)
                                {
                                    var jObj = new JObject();
                                    var val = modelItem.GetCurrentValue();
                                    jObj["Type"] = "5";
                                    jObj["DisplayName"] = (val as Activity).DisplayName;
                                    jObj["IdRef"] = WorkflowViewState.GetIdRef(val);
                                    jObj["Path"] = path;
                                    jObj["Location"] = location;
                                    retArr.Add(jObj);
                                }  
                            }         
                        }
                    }

                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return "[]";
            }
        }

        public void RemoveUnusedVariables(string path)
        {
            try
            {
                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                bool changeFlg = false;
                HashSet<ModelItem> inUseVariables = this.GetInUseVariables(modelService);
                foreach (ModelItem modelItem in modelService.GetVariables())
                {
                    try
                    {
                        if (!inUseVariables.Contains(modelItem))
                        {
                            ModelItemHelper.RemoveVariableFromParent(modelItem);
                            changeFlg = true;
                        }
                    }
                    catch
                    {
                    }
                }

                //DesignerView service = workflowDesigner.Context.Services.GetService<DesignerView>();
                //((RoutedCommand)DesignerView.ToggleVariableDesignerCommand).Execute(null, service);

                if (changeFlg)
                {
                    DesignerView designerView = workflowDesigner.Context.Services.GetService<DesignerView>();
                    ContentControl contentControl = (ContentControl)designerView.FindName("variables1");

                    ModelItem selectModelItem = workflowDesigner.Context.Items.GetValue<Selection>().PrimarySelection;
                    string itemTypeName = selectModelItem?.ItemType?.Name;

                    if (itemTypeName == null || itemTypeName == "DesignTimeVariable")
                    {
                        if (modelService.GetVariables().Count() > 0)
                        {
                            ((dynamic)contentControl.AsDynamic()).Populate(modelService.GetVariables().First<ModelItem>());
                        }
                        else
                        {
                            ((dynamic)contentControl.AsDynamic()).Populate(null);
                        }
                    }
                    else if (itemTypeName != "DesignTimeArgument")
                    {
                        ((dynamic)contentControl.AsDynamic()).Populate(selectModelItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw new InvalidOperationException(ex.Message);
            }
        }

        public void RemoveUnusedArguments(string path)
        {
            try
            {
                WorkflowDesigner workflowDesigner = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();
                var modelService = workflowDesigner.GetModelService();

                IEnumerable<ModelItem> enumerable = this.GetArguments(modelService);

                if (enumerable != null)
                {
                    HashSet<ModelItem> inUseArguments = this.GetInUseArguments(modelService, enumerable);
                    HashSet<ModelItem> deleteArguments = new HashSet<ModelItem>();
                    foreach (ModelItem item in enumerable)
                    {
                        try
                        {
                            if (!inUseArguments.Contains(item))
                            {
                                deleteArguments.Add(item);
                            }
                        }
                        catch
                        {
                        }
                    }

                    bool changeFlg = false;
                    ModelItemCollection modelItemCollection = modelService.Root.Properties["Properties"].Collection;
                    foreach (ModelItem item in deleteArguments)
                    {
                        try
                        {
                            modelItemCollection.Remove(item);
                            changeFlg = true;
                        }
                        catch
                        {
                        }
                       
                    }

                    if (changeFlg)
                    {
                        DesignerView designerView = workflowDesigner.Context.Services.GetService<DesignerView>();
                        ContentControl contentControl = (ContentControl)designerView.FindName("arguments1");

                        ((dynamic)contentControl.AsDynamic()).isCollectionLoaded = false;
                        ((dynamic)contentControl.AsDynamic()).Populate();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw new InvalidOperationException(ex.Message);
            }
        }

        public string GetXamlValidInfo(string path)
        {
            try
            {
                JArray retArr = new JArray();
                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, path);
                var location = relativeFile.Replace("\\", " > ");

                Activity workflow = ActivityXamlServices.Load(path);
                var result = ActivityValidationServices.Validate(workflow, new System.Activities.Validation.ValidationSettings
                {
                    SkipValidatingRootConfiguration = true
                });

                foreach (System.Activities.Validation.ValidationError errItem in result.Errors)
                {
                    var jObj = new JObject();
                    //jObj["DisplayName"] = (errItem.Source as Activity).DisplayName;
                    jObj["IdRef"] = WorkflowViewState.GetIdRef(errItem.Source) != null ? WorkflowViewState.GetIdRef(errItem.Source) : WorkflowViewState.GetIdRef(errItem.Source.GetParent());
                    jObj["ErrorContent"] = errItem.Message;
                    jObj["Path"] = path;
                    jObj["Location"] = location;
                    retArr.Add(jObj);
                }

                return JsonConvert.SerializeObject(retArr);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return null;
            }
        }

        private IReadOnlyCollection<InvokableArgument> GetVariablesFromExpression(ArgumentDirection direction, ITextExpression textExpression)
        {
            return (from o in new ExpressionAnalyzerFactory().GetIExpressionAnalyzer(textExpression.Language).GetUniqueIdentifiers(textExpression.ExpressionText)
                    select new InvokableArgument(o, direction, textExpression.GetType().GenericTypeArguments.First<Type>())).ToList<InvokableArgument>();
        }

        public List<InvokableArgument> GetUsedVariablesFromActivity(ModelItem activity)
        {
            List<InvokableArgument> list = new List<InvokableArgument>();
            ModelPropertyCollection modelPropertyCollection = (activity != null) ? activity.Properties : null;
            if (modelPropertyCollection == null || !modelPropertyCollection.Any<ModelProperty>())
            {
                return list;
            }
            try
            {
                foreach (ModelProperty modelProperty in modelPropertyCollection)
                {
                    if (modelProperty.IsSet)
                    {
                        ICollection<ModelItem> collection = null;
                        if (modelProperty.IsDictionary)
                        {
                            collection = modelProperty.Dictionary.Values;
                        }
                        else if (modelProperty.IsCollection)
                        {
                            collection = modelProperty.Collection;
                        }
                        if (collection != null)
                        {
                            using (IEnumerator<ModelItem> enumerator2 = collection.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    ModelItem modelItem = enumerator2.Current;
                                    if (modelItem != null)
                                    {
                                        list = list.Concat(this.GetUsedVariablesFromActivity(modelItem)).ToList<InvokableArgument>();
                                    }
                                }
                                continue;
                            }
                        }
                        Type baseType = modelProperty.PropertyType.BaseType;
                        if (baseType != null)
                        {
                            if (baseType.Equals(typeof(OutArgument)) || baseType.Equals(typeof(InArgument)) || baseType.Equals(typeof(InOutArgument)) || baseType.Equals(typeof(Argument)) || baseType.Equals(typeof(ActivityWithResult)) || modelProperty.PropertyType.Equals(typeof(Argument)))
                            {
                                ModelItem value = modelProperty.Value;
                                if (!baseType.Equals(typeof(ActivityWithResult)))
                                {
                                    value = modelProperty.Value.Properties["Expression"].Value;
                                    if (value == null)
                                    {
                                        continue;
                                    }
                                }
                                ITextExpression textExpression = value.GetCurrentValue() as ITextExpression;
                                if (textExpression != null)
                                {
                                    list.AddRange(this.GetVariablesFromExpression(textExpression.GetArgumentDirectionFromExpression(), textExpression));
                                }
                            }
                            else if (modelProperty.PropertyType.IsExpandableProperty())
                            {
                                list.AddRange(this.GetUsedVariablesFromActivity(modelProperty.Value));
                            }
                        }
                    }
                }
            }
            catch (Exception arg)
            {
                Trace.TraceWarning(string.Format("{0} exception: {1}", "GetUsedVariablesFromActivity", arg));
            }
            return list;
        }

        public IEnumerable<ModelItem> GetVariablesInScope(ModelItem modelItem)
        {
            return ModelItemHelper.GetVariablesInScope(modelItem);
        }

        public IEnumerable<ModelItem> GetArguments(ModelService modelService)
        {
            return ModelItemHelper.GetProperties((modelService != null) ? modelService.Root : null);
        }

        private HashSet<ModelItem> GetInUseVariables(ModelService modelService)
        {
            HashSet<ModelItem> hashSet = new HashSet<ModelItem>();

            var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
            foreach (ModelItem modelItem in activities)
            {
                List<InvokableArgument> usedVariables = this.GetUsedVariablesFromActivity(modelItem);
                if (usedVariables.Count != 0)
                {
                    hashSet.UnionWith(from v in this.GetVariablesInScope(modelItem)
                                      where usedVariables.Any((InvokableArgument u) => u.Name.Equals(v.GetName(), StringComparison.OrdinalIgnoreCase))
                                      select v);
                }
            }
            return hashSet;
        }

        private HashSet<ModelItem> GetInUseArguments(ModelService modelService, IEnumerable<ModelItem> arguments)
        {
            HashSet<ModelItem> hashSet = new HashSet<ModelItem>();

            var activities = modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
            foreach (ModelItem modelItem in activities)
            {
                List<InvokableArgument> usedVariables = this.GetUsedVariablesFromActivity(modelItem);
                if (usedVariables.Count != 0)
                {
                    hashSet.UnionWith(from v in arguments
                                      where usedVariables.Any((InvokableArgument u) => u.Name.Equals(v.GetName(), StringComparison.OrdinalIgnoreCase))
                                      select v);
                }
            }
            return hashSet;
        }

    }
}
