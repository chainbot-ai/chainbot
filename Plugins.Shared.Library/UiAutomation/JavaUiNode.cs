using FlaUI.Core.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using WindowsAccessBridgeInterop;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Linq;
using System.IO;

namespace Plugins.Shared.Library.UiAutomation
{
    class JavaUiNode : UiNode
    {
        internal static readonly AccessBridge accessBridge = new AccessBridge();
        private static List<AccessibleJvm> cachedJvms;
        internal AccessibleNode accessibleNode;

        private Dictionary<string, string> propDict = new Dictionary<string, string>();
        private Dictionary<string, object> propObjectDict = new Dictionary<string, object>();

        
        public bool IsTableCell { get; private set; }

        private UiNode cachedParent;

        static JavaUiNode()
        {
            try
            {
                ProcessWindowsAccessBridgeDlls();
                accessBridge.Initialize();
            }
            catch (Exception)
            {
               
            }
        }

        private static void ProcessWindowsAccessBridgeDlls()
        {
            var windowsAccessBridgeDll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"WindowsAccessBridge.dll");
            var windowsAccessBridge32Dll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"WindowsAccessBridge-32.dll");
            var windowsAccessBridge64Dll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"WindowsAccessBridge-64.dll");

            var jabWindowsAccessBridgeDll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"JAB\WindowsAccessBridge.dll");
            var jabWindowsAccessBridge32Dll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"JAB\WindowsAccessBridge-32.dll");
            var jabWindowsAccessBridge64Dll = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"JAB\WindowsAccessBridge-64.dll");

            try
            {
                File.Delete(windowsAccessBridgeDll);
                File.Delete(windowsAccessBridge32Dll);
                File.Delete(windowsAccessBridge64Dll);
            }
            catch (Exception)
            {

            }

            try
            {
                if(!Directory.Exists(Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"JAB")))
                {
                    SharedObject.Instance.Output(SharedObject.enOutputType.Error, "当前程序所在目录不存在JAB文件夹，请检查！");
                }

                if (Environment.Is64BitOperatingSystem)
                {
                    File.Copy(jabWindowsAccessBridge32Dll, windowsAccessBridge32Dll, true);
                    File.Copy(jabWindowsAccessBridge64Dll, windowsAccessBridge64Dll, true);
                }
                else
                {
                    File.Copy(jabWindowsAccessBridgeDll, windowsAccessBridgeDll, true);
                }
            }
            catch (Exception)
            {

            }            
        }

        public JavaUiNode(AccessibleNode accessibleNode)
        {
            this.accessibleNode = accessibleNode;

            //var propertyList = accessibleNode.GetProperties(
            //      PropertyOptions.AccessibleContextInfo
            //    | PropertyOptions.ObjectDepth
            //    | PropertyOptions.ParentContext
            //    | PropertyOptions.TopLevelWindowInfo
            //    | PropertyOptions.ActiveDescendent
            //    | PropertyOptions.VisibleChildren
            //    | PropertyOptions.AccessibleActions
            //    | PropertyOptions.AccessibleKeyBindings
            //    | PropertyOptions.AccessibleIcons
            //    | PropertyOptions.AccessibleRelationSet
            //    | PropertyOptions.AccessibleText
            //    | PropertyOptions.AccessibleHyperText
            //    | PropertyOptions.AccessibleValue
            //    | PropertyOptions.AccessibleSelection
            //    | PropertyOptions.AccessibleTable
            //    | PropertyOptions.AccessibleTableCells
            //    | PropertyOptions.AccessibleTableCellsSelect
            //    | PropertyOptions.Children
            //    );

            var propertyList = accessibleNode.GetProperties(PropertyOptions.AccessibleContextInfo | PropertyOptions.ObjectDepth);
            foreach (var item in propertyList)
            {
                if (item.Name != null)
                {
                    propObjectDict[item.Name] = item.Value;
                    propDict[item.Name] = item.Value == null ? "" : item.Value.ToString();
                }
            }
        }

        public string Sel { get; set; }
        public string AutomationId
        {
            get
            {
                return "";
            }
        }

        public string UserDefineId
        {
            get
            {
                if (propDict.ContainsKey("Index in parent"))
                {
                    return propDict["Index in parent"];
                }
                else
                {
                    return "";
                }
            }
        }

        public string Idx
        {
            get
            {
                return "";
            }
        }


        public UiNode GetChildByIdx(int idx)
        {
            if (this.ControlType == "JavaNode" && this.Role == "table")
            {
                foreach(var child in VisibleChildren)
                {
                    if(child.UserDefineId == idx.ToString())
                    {
                        var javaChild = child as JavaUiNode;
                        javaChild.IsTableCell = true;

                        return javaChild;
                    }
                }

                return null;
            }
            else
            {
            return new JavaUiNode(accessibleNode.GetChildAt(idx));
            } 
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                Rectangle rect = accessibleNode.GetScreenRectangle() ?? new Rectangle();
                return rect;
            }
        }



        public List<UiNode> VisibleChildren
        {
            get
            {
                var list = new List<UiNode>();

                if (this.ControlType == "JavaNode" && this.Role == "table")
                {
                    if (accessibleNode is AccessibleContextNode)
                    {
                        var node = accessibleNode as AccessibleContextNode;
                        VisibleChildrenInfo childrenInfo;
                        if (node.AccessBridge.Functions.GetVisibleChildren(node.JvmId, node.AccessibleContextHandle, 0, out childrenInfo))
                        {
                            foreach (var child in childrenInfo.children)
                            {
                                var acNode = new AccessibleContextNode(JavaUiNode.accessBridge, child);

                                var javaCellNode = new JavaUiNode(acNode);
                                javaCellNode.IsTableCell = true;

                                list.Add(javaCellNode);
                            }
                        }
                    }
                }
                else
                {
                    var children = accessibleNode.GetChildren();
                    foreach (var child in children)
                    {
                        list.Add(new JavaUiNode(child));
                    }
                }

                return list;
            }
        }


        public List<UiNode> Children
        {
            get
            {
                var list = new List<UiNode>();

                if (this.ControlType == "JavaNode" && this.Role == "table")
                {
                    var children = accessibleNode.GetChildren();
                    foreach (var child in children)
                    {
                        var javaCellNode = new JavaUiNode(child);
                        javaCellNode.IsTableCell = true;

                        list.Add(javaCellNode);
                    }
                }
                else
                {
                var children = accessibleNode.GetChildren();
                    foreach (var child in children)
                {
                    list.Add(new JavaUiNode(child));
                }
                }

                return list;
            }
        }

        public string ClassName
        {
            get
            {
                return "";
            }
        }

        public string ControlType
        {
            get
            {
                return "JavaNode";
            }
        }

        public bool IsTopLevelWindow
        {
            get
            {
                return WindowHandle != IntPtr.Zero;
            }
        }

        public string Name
        {
            get
            {
                if (propDict.ContainsKey("Name"))
                {
                    return propDict["Name"];
                }

                return "";
            }
        }

        public UiNode Parent
        {
            get
            {
                if(cachedParent == null)
                {
                    var parent = accessibleNode.GetParent();
                    if (parent != null)
                    {
                        UiNode parentUiNode = new JavaUiNode(parent);
                        if (parentUiNode.IsTopLevelWindow)
                        {
                            parentUiNode = new UIAUiNode(UIAUiNode.UIAAutomation.FromHandle(parentUiNode.WindowHandle));
                        }

                        cachedParent = parentUiNode;
                    }
                }

                return cachedParent;
            }
        }

        public string ProcessName
        {
            get
            {
                return "";
            }
        }

        public string ProcessFullPath
        {
            get
            {
                return "";
            }
        }

        public string Role
        {
            get
            {
                if(propDict.ContainsKey("Role"))
                {
                    return propDict["Role"];
                }

                return "";
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                if (propDict.ContainsKey("WindowHandle"))
                {
                    return (IntPtr)Convert.ToInt32(propDict["WindowHandle"]);
                }

                return IntPtr.Zero;
            }
        }

        public string Description
        {
            get
            {
                if (propDict.ContainsKey("Description"))
                {
                    return propDict["Description"];
                }

                return "";
            }
        }

        public UiNode AutomationElementParent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Text
        {
            get
            {
                if (Role == "combo box")
                {
                    if (accessibleNode is AccessibleContextNode)
                    {
                        var node = accessibleNode as AccessibleContextNode;

                        var handle = node.AccessBridge.Functions.GetAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle, 0);

                        AccessibleContextInfo info;
                        node.AccessBridge.Functions.GetAccessibleContextInfo(node.JvmId, handle, out info);

                        return info.name;
                    }
                    else
                    {
                        return "";
                    }
                }else if(Role == "check box")
                {
                    return GetElementChecked().ToString();
                }
                else if (Role == "table")
                {
                    if (accessibleNode is AccessibleContextNode)
                    {
                        var node = accessibleNode as AccessibleContextNode;

                        AccessibleTableInfo tableInfo;
                        if(node.AccessBridge.Functions.GetAccessibleTableInfo(node.JvmId, node.AccessibleContextHandle,out tableInfo))
                        {
                            int rowCount = tableInfo.rowCount;
                            int columnCount = tableInfo.columnCount;

                            var rowList = new List<string>();

                            for (int row = 0; row < rowCount; row++)
                            {
                                var colList = new List<string>();
                                for (int column = 0; column < columnCount; column++)
                                {
                                    int idx = row * columnCount + column;

                                    var child = node.GetChildAt(idx);
                                    var javaCellNode = new JavaUiNode(child);
                                    javaCellNode.IsTableCell = true;

                                    colList.Add(javaCellNode.Text);
                                }

                                var colStr = String.Join("\t", colList);

                                rowList.Add(colStr);
                            }

                            var rowStr = String.Join("\n", rowList);

                            return rowStr;
                        }
                    }

                    return "";
                }
                else
                {
                    if (accessibleNode is AccessibleContextNode)
                    {
                        var node = accessibleNode as AccessibleContextNode;

                        AccessibleContextInfo info;
                        node.AccessBridge.Functions.GetAccessibleContextInfo(node.JvmId, node.AccessibleContextHandle, out info);

                        if(info.accessibleText > 0)
                        {
                            var point = new Point(0, 0);
                            AccessibleTextInfo textInfo;

                            var ret = "";
                            if (node.AccessBridge.Functions.GetAccessibleTextInfo(node.JvmId, node.AccessibleContextHandle, out textInfo, point.X, point.Y))
                            {
                                var reader = new AccessibleTextReader(node, textInfo.charCount);
                                var lines = reader
                                  .ReadFullLines(node.AccessBridge.TextLineLengthLimit)
                                  .Where(x => !x.IsContinuation)
                                  .Take(node.AccessBridge.TextLineCountLimit);
                                foreach (var lineData in lines)
                                {
                                    ret += lineData.Text;
                                }

                                return ret;
                            }
                        }
                    }
                    
                    return Name;
                }
            }
        }


        internal static List<AccessibleJvm> EnumJvms(bool bRefresh = false)
        {
            if(bRefresh)
            {
                cachedJvms = null;
            }

            if(cachedJvms == null || cachedJvms.Count == 0)
            {
                cachedJvms = accessBridge.EnumJvms(hwnd => accessBridge.CreateAccessibleWindow(hwnd));
            }
            return cachedJvms;
        }

        public void SetForeground()
        {
            if(WindowHandle != IntPtr.Zero)
            {
                UiCommon.SetForegroundWindow(WindowHandle);
            }
        }

        public void MouseClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            var clickablePoint = GetClickablePoint();
            if (clickParams.moveMouse)
            {
                Mouse.MoveTo(clickablePoint);
            }
            else
            {
                Mouse.Position = clickablePoint;
            }

            Mouse.LeftClick();
        }

        public void MouseDoubleClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            var clickablePoint = GetClickablePoint();
            if (clickParams.moveMouse)
            {
                Mouse.MoveTo(clickablePoint);
            }
            else
            {
                Mouse.Position = clickablePoint;
            }

            Mouse.LeftDoubleClick();
        }

        public void MouseRightClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            var clickablePoint = GetClickablePoint();
            if (clickParams.moveMouse)
            {
                Mouse.MoveTo(clickablePoint);
            }
            else
            {
                Mouse.Position = clickablePoint;
            }

            Mouse.RightClick();
        }

        public void MouseRightDoubleClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            var clickablePoint = GetClickablePoint();
            if (clickParams.moveMouse)
            {
                Mouse.MoveTo(clickablePoint);
            }
            else
            {
                Mouse.Position = clickablePoint;
            }

            Mouse.RightDoubleClick();
        }

        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            if (hoverParams == null)
            {
                hoverParams = new UiElementHoverParams();
            }

            var clickablePoint = GetClickablePoint();
            if (hoverParams.moveMouse)
            {
                Mouse.MoveTo(clickablePoint);
            }
            else
            {
                Mouse.Position = clickablePoint;
            }
        }

        public Point GetClickablePoint()
        {
            var point = new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2, BoundingRectangle.Top + BoundingRectangle.Height / 2);
            return point;
        }

        public void Focus()
        {
           
        }

        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            throw new NotImplementedException();
        }

        public UiNode FindRelativeNode(int position, int offsetX, int offsetY)
        {
            throw new NotImplementedException();
        }

        public void SetElementSelect(string[] values)
        {
            if(accessibleNode is AccessibleContextNode)
            {
                var node = accessibleNode as AccessibleContextNode;
                var count = node.AccessBridge.Functions.GetAccessibleSelectionCountFromContext(node.JvmId, node.AccessibleContextHandle);
                
                if(count == 1)
                {
                    for (var i = 0; ; i++)
                    {
                        node.AccessBridge.Functions.AddAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle, i);

                        var handle = node.AccessBridge.Functions.GetAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle, 0);

                        AccessibleContextInfo info;
                        node.AccessBridge.Functions.GetAccessibleContextInfo(node.JvmId, handle, out info);

                        bool hasFound = false;
                        foreach(var val in values)
                        {
                            if(val == info.name)
                            {
                                hasFound = true;
                                break;
                            }
                        }

                        if(hasFound)
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(info.name))
                        {
                            break;
                        }
                    }
                }else if(count > 1)
                {
                    node.AccessBridge.Functions.ClearAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle);
                    for (var i = 0; i < count; i++)
                    {
                        var handle = node.AccessBridge.Functions.GetAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle, i);

                        AccessibleContextInfo info;
                        node.AccessBridge.Functions.GetAccessibleContextInfo(node.JvmId, handle, out info);

                        foreach (var val in values)
                        {
                            if (val == info.name)
                            {
                                node.AccessBridge.Functions.AddAccessibleSelectionFromContext(node.JvmId, node.AccessibleContextHandle, i);
                            }
                        }
                    }
                }
                
            }
        }

        public void SetText(string txt)
        {
            if (accessibleNode is AccessibleContextNode)
            {
                var node = accessibleNode as AccessibleContextNode;
                if(!node.AccessBridge.Functions.SetTextContents(node.JvmId, node.AccessibleContextHandle, txt))
                {
                    throw new Exception($"在该元素上设置文本失败，请改用模拟方式设置，内容：{txt}");
                }
            }
            else
            {
                throw new Exception($"不支持在该元素上设置文本，内容：{txt}");
            }
                
        }

        public string[] GetElementSelectedItems()
        {
            return new string[] { Text };
        }

        public bool GetElementChecked()
        {
            if (accessibleNode is AccessibleContextNode)
            {
                var node = accessibleNode as AccessibleContextNode;
                var info = node.GetInfo();
                if(info.states_en_US.Contains("checked"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasAction(AccessibleContextNode node,string action)
        {
            AccessibleActions actionsGet;
            node.AccessBridge.Functions.GetAccessibleActions(node.JvmId, node.AccessibleContextHandle, out actionsGet);
            for (var i=0;i< actionsGet.actionsCount;i++)
            {
                if(actionsGet.actionInfo[i].name == action)
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoAction(string action)
        {
            if(action == "click")
            {
                if(!DoActions(action))
                {
                    return DoActions("单击");
                }
                else
                {
                    return true;
                }
            }else if (action == "togglePopup")
            {
                if (!DoActions(action))
                {
                    return DoActions("切换键弹出");
                }
                else
                {
                    return true;
                }
            }

            return DoActions(action);
        }

        private bool DoActions(params string[] actions)
        {
            const int MAX_ACTIONS_TO_DO = 32;

            if (accessibleNode is AccessibleContextNode)
            {
                var node = accessibleNode as AccessibleContextNode;

                AccessibleActionsToDo actionsToDo = new AccessibleActionsToDo()
                {
                    actions = new AccessibleActionInfo[MAX_ACTIONS_TO_DO],
                    actionsCount = actions.Length,
                };

                for (int i = 0, n = Math.Min(actions.Length, MAX_ACTIONS_TO_DO); i < n; i++)
                    actionsToDo.actions[i].name = actions[i];

                int failure = 0;

                if(node.AccessBridge.Functions.DoAccessibleActions(node.JvmId, node.AccessibleContextHandle,ref actionsToDo, out failure))
                {
                    return true;
                }
            }

            return false;
        }

        public void InternalClick()
        {
            if (Role == "combo box")
            {
                DoAction("togglePopup");
            }else
            {
                DoAction("click");
            }
        }

        public void SetElementChecked(bool isChecked)
        {
            var currentIsChecked = GetElementChecked();
            if(currentIsChecked == isChecked)
            {
                return;
            }
            else
            {
                DoAction("click");
            }
        }

        public string GetElementAttribute(string attrName)
        {
            if(attrName.ToLower() == "Value".ToLower())
            {
                return this.Text;
            }

            var propertyList = accessibleNode.GetProperties(PropertyOptions.AccessibleContextInfo | PropertyOptions.ObjectDepth);
            foreach (var item in propertyList)
            {
                if (item.Name  == attrName)
                {
                    return item.Value.ToString();
                }
            }

            throw new Exception($"找不到属性名{attrName}对应的值，请检查属性名是否存在！");
        }

        public void SetElementAttribute(string attrName, string attrValue)
        {
            if (attrName.ToLower() == "Value".ToLower())
            {
                if (Role == "combo box")
                {
                    SetElementSelect(new string[] { attrValue });
                }
                else if (Role == "check box")
                {
                    if(attrValue.ToLower() == "true")
                    {
                        SetElementChecked(true);
                    }
                }
                else
                {
                    SetText(attrValue);
                }
            }
        }

        public Rectangle GetElementRectangle()
        {
            return BoundingRectangle;
        }

        public bool TableCellContains(Point screenPoint, out UiNode cellUiNode)
        {
            cellUiNode = null;

            if (accessibleNode is AccessibleContextNode)
            {
                var node = accessibleNode as AccessibleContextNode;
                VisibleChildrenInfo childrenInfo;
                if (node.AccessBridge.Functions.GetVisibleChildren(node.JvmId, node.AccessibleContextHandle, 0, out childrenInfo))
                {
                    foreach (var child in childrenInfo.children)
                    {
                        var acNode = new AccessibleContextNode(JavaUiNode.accessBridge, child);
                        Rectangle rect = acNode.GetScreenRectangle() ?? new Rectangle();
                        if (rect.Contains(screenPoint))
                        {
                            var cell = new JavaUiNode(acNode);
                            cell.IsTableCell = true;

                            cellUiNode = cell;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
