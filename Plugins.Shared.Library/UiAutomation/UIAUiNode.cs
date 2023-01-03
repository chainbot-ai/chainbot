using System;
using System.Collections.Generic;
using System.Drawing;
using FlaUI.UIA3;
using WindowsAccessBridgeInterop;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using System.Diagnostics;

namespace Plugins.Shared.Library.UiAutomation
{
    public class UIAUiNode : UiNode
    {
        private static ITreeWalker _treeWalker;

        internal static AutomationBase uia3Automation = new UIA3Automation();
        internal static AutomationBase uia2Automation = new UIA2Automation();

        internal AutomationElement automationElement;

        private string cachedProcessName;
        private string cachedProcessFullPath;
        private UIAUiNode cachedParent;
        private UIAUiNode automationElementParent;

        public UIAUiNode(AutomationElement element, UIAUiNode parent = null)
        {
            this.automationElement = element;
            this.cachedParent = parent;
        }

        private AutomationElement getCorrectParent(AutomationElement element)
        {
            return TreeWalker.GetParent(element);
        }

        private AutomationElement[] getCorrectChildren(AutomationElement element)
        {
            //return element.FindAllChildren();

            var children = new List<AutomationElement>();
            var child = TreeWalker.GetFirstChild(element);
            if(child != null)
            {
                children.Add(child);

                while ((child = TreeWalker.GetNextSibling(child)) != null)
                {
                    children.Add(child);
                }
            }

            return children.ToArray();
        }



        public static AutomationBase UIAAutomation
        {
            get
            {
                return uia2Automation;
            }
        }

        private static ITreeWalker TreeWalker
        {
            get
            {
                if (_treeWalker == null)
                {
                    _treeWalker = UIAAutomation.TreeWalkerFactory.GetControlViewWalker();
                }

                return _treeWalker;
            }
        }

        public string Sel { get; set; }

        private bool IsNeedAutomationId(AutomationElement[] children)
        {
            int matchCount = 0;
            foreach (var item in children)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Name) && Name != item.Name)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(ClassName) && ClassName != item.ClassName)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(Description) && item.Patterns.LegacyIAccessible.Pattern.Description.IsSupported && Description != item.Patterns.LegacyIAccessible.Pattern.Description.ValueOrDefault)
                    {
                        continue;
                    }

                    matchCount++;
                }
                catch
                {
                }
            }

            if (matchCount > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string AutomationId
        {
            get
            {
                string automationId = null;
                try
                {
                    automationId = automationElement.AutomationId;
                    if (!string.IsNullOrEmpty(automationId))
                    {
                        var realParent = getCorrectParent(automationElement);
                        var children = realParent.FindAllChildren();
                        if (children != null && children.Length > 1 && IsNeedAutomationId(children))
                        {
                            return automationId;
                        }
                    }

                    return "";
                }
                catch
                {
                    return "";
                }
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                try
                {
                    return automationElement.BoundingRectangle;
                }
                catch (Exception err)
                {
                    Trace.WriteLine("@@@@@@@@@@@@@@@@BoundingRectangle:" + err);
                    return new Rectangle();
                }

            }
        }

        public string ClassName
        {
            get
            {
                try
                {
                    return automationElement.ClassName;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public string ControlType
        {
            get
            {
                try
                {
                    return automationElement.ControlType.ToString();
                }
                catch (Exception)
                {

                    return "";
                }
            }
        }


        public string Name
        {
            get
            {
                try
                {
                    return automationElement.Name;
                }
                catch (Exception)
                {

                    return "";
                }
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                try
                {
                    if (automationElement.Properties.NativeWindowHandle.IsSupported)
                    {
                        var windowHandle = automationElement.Properties.NativeWindowHandle.ValueOrDefault;
                        return windowHandle;
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }
                }
                catch (Exception)
                {
                    return IntPtr.Zero;
                }
                
            }
        }

        

        public UiNode Parent
        {
            get
            {
                if (cachedParent == null)
                {
                    var realParent = getCorrectParent(automationElement);

                    if (realParent != null)
                    {
                        cachedParent = new UIAUiNode(realParent);
                    }
                }

                return cachedParent;
            }
        }

        public UiNode AutomationElementParent
        {
            get
            {
                automationElementParent = new UIAUiNode(automationElement.Parent);
                return automationElementParent;
            }
        }

        public string Role
        {
            get
            {
                return "";
            }
        }

        public string ProcessName
        {
            get
            {
                if (cachedProcessName == null)
                {
                    try
                    {
                        var processId = automationElement.Properties.ProcessId.Value;
                        var name = UiCommon.GetProcessName(processId);

                        cachedProcessName = name;
                    }
                    catch (Exception)
                    {
                        cachedProcessName = "";
                    }
                }

                return cachedProcessName;
            }
        }

        public string ProcessFullPath
        {
            get
            {
                if (cachedProcessFullPath == null)
                {
                    try
                    {
                        var processId = automationElement.Properties.ProcessId.Value;
                        var path = UiCommon.GetProcessFullPath(processId);

                        cachedProcessFullPath = path;
                    }
                    catch (Exception)
                    {
                        cachedProcessFullPath = "";
                    }
                }

                return cachedProcessFullPath;
            }
        }

        public List<UiNode> Children
        {
            get
            {
                var list = new List<UiNode>();

                var children = getCorrectChildren(automationElement);

                foreach (var item in children)
                {
                    list.Add(new UIAUiNode(item, this));
                }

                if (JavaUiNode.accessBridge.Functions.IsJavaWindow(this.WindowHandle))
                {
                    int vmid;
                    JavaObjectHandle ac;
                    JavaUiNode.accessBridge.Functions.GetAccessibleContextFromHWND(this.WindowHandle, out vmid, out ac);

                    if (!ac.IsNull)
                    {
                        var acNode = new AccessibleContextNode(JavaUiNode.accessBridge, ac);
                        foreach(var child in acNode.GetChildren())
                        {
                            list.Add(new JavaUiNode(child));
                        }
                    }
                }
                return list;
            }
        }

        public bool IsTopLevelWindow
        {
            get
            {
                try
                {
                    return automationElement.Parent != null && automationElement.Parent.Parent == null;
                    
                }
                catch (Exception)
                {
                    return false;
                }
               
            }
        }

        public string UserDefineId
        {
            get
            {
                return "";
            }
        }

        
        public string Description
        {
            get
            {

                try
                {
                    if (automationElement.Patterns.LegacyIAccessible.Pattern.Description.IsSupported)
                    {
                        return automationElement.Patterns.LegacyIAccessible.Pattern.Description.ValueOrDefault;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        private bool IsNeedIdx(AutomationElement[] children)
        {
            int matchCount = 0;
            foreach (var item in children)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Name) && Name != item.Name)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(AutomationId) && AutomationId != item.AutomationId)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(ClassName) && ClassName != item.ClassName)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(Description) && item.Patterns.LegacyIAccessible.Pattern.Description.IsSupported && Description != item.Patterns.LegacyIAccessible.Pattern.Description.ValueOrDefault)
                    {
                        continue;
                    }

                    matchCount++;
                }
                catch
                {
                }
            }

            if (matchCount > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Idx
        {
            get
            {
                try
                {
                    var realParent = getCorrectParent(automationElement);
                    var children = realParent.FindAllChildren();

                    if (children != null && children.Length > 1 && IsNeedIdx(children))
                    {
                        int index = 0;
                        foreach (var item in children)
                        {
                            if (item.Equals(automationElement))
                            {
                                return index.ToString();
                            }

                            index++;
                        }
                    }

                    return "";
                }
                catch
                {
                    return "";
                }
            }
        }

        public string Text
        {
            get
            {
                if (ControlType == "Edit")
                {
                    return automationElement.Patterns.Value.Pattern.Value;
                }
                return Name;
            }
        }

        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            var list = new List<UiNode>();
            var elements = automationElement.FindAll(scope, condition);
            foreach (var item in elements)
            {
                list.Add(new UIAUiNode(item, this));
            }
            return list;
        }

        public UiNode FindRelativeNode(int position, int offsetX, int offsetY)
        {
            UIAUiNode relativeNode = null ;
            Rectangle rect = automationElement.BoundingRectangle;
            int posX, posY;
            Point point;
            switch (position)
            {
                case 0:     
                    posX = rect.Location.X + rect.Width / 2;
                    posY = rect.Location.Y + rect.Height / 2;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
                case 1:     
                    posX = rect.Location.X;
                    posY = rect.Location.Y;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
                case 2:     
                    posX = rect.Location.X + rect.Width;
                    posY = rect.Location.Y;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
                case 3:     
                    posX = rect.Location.X;
                    posY = rect.Location.Y + rect.Height;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
                case 4:     
                    posX = rect.Location.X + rect.Width;
                    posY = rect.Location.Y + rect.Height;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
                default:    
                    posX = rect.Location.X + rect.Width / 2;
                    posY = rect.Location.Y + rect.Height / 2;
                    point = new Point(posX + offsetX, posY + offsetY);
                    break;
            }


            try
            {
                this.automationElement = UIAUiNode.UIAAutomation.FromPoint(point);
            }
            catch (Exception)
            {
                if (this.automationElement == null)
                {
                    IntPtr hWnd = UiCommon.WindowFromPoint(point);
                    if (hWnd != IntPtr.Zero)
                    {
                        this.automationElement = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                    }
                }
            }
            relativeNode = new UIAUiNode(automationElement);
            return relativeNode;
        }


        public UiNode GetChildByIdx(int idx)
        {
            var item = automationElement.FindChildAt(idx);
            if (item != null)
            {
                return new UIAUiNode(item, this);
            }
            else
            {
                return null;
            }
        }

        public void MouseClick(UiElementClickParams clickParams = null)
        {
            if(clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            try
            {
                automationElement.Click(clickParams.moveMouse);
            }
            catch (Exception)
            {
            }
        }

        public void MouseDoubleClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            try
            {
                automationElement.DoubleClick(clickParams.moveMouse);
            }
            catch (Exception)
            {
            }
        }

        public void MouseRightClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            try
            {
                automationElement.RightClick(clickParams.moveMouse);
            }
            catch (Exception)
            {
            }
        }

        public void MouseRightDoubleClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            try
            {
                automationElement.RightDoubleClick(clickParams.moveMouse);
            }
            catch (Exception)
            {
            }
        }

        public void SetForeground()
        {
            this.automationElement.SetForeground();
        }

        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            if (hoverParams == null)
            {
                hoverParams = new UiElementHoverParams();
            }

            try
            {
                var clickablePoint = GetClickablePoint();
                if(hoverParams.moveMouse)
                {
                    FlaUI.Core.Input.Mouse.MoveTo(clickablePoint);
                }
                
                FlaUI.Core.Input.Mouse.Position = clickablePoint;
            }
            catch (Exception)
            {

            }
            
        }

        public Point GetClickablePoint()
        {
            try
            {
                return automationElement.GetClickablePoint();
            }
            catch (Exception err)
            {
                Trace.WriteLine("@@@@@@@@@@@@@@@@GetClickablePoint:" + err);

                return new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2,
                                    BoundingRectangle.Top + BoundingRectangle.Height / 2);
            }
        }

        public void Focus()
        {
            automationElement.Focus();
        }

        public void SetElementSelect(string[] values)
        {
            var controlType = automationElement.ControlType.ToString();
            List<string> result = new List<string>();
            if (controlType == "ComboBox")
            {
                ComboBox cbBox = automationElement.AsComboBox();
                ComboBoxItem[] items = cbBox.Items;
                foreach (var item in items)
                {
                    foreach(var value in values)
                    {
                        if(item.Text == value)
                        {
                            item.IsSelected = true;
                        }
                    }
                }
            }
            else if (controlType == "List")
            {
                ListBox listBox = automationElement.AsListBox();
                ListBoxItem[] items = listBox.Items;
                
                foreach(var item in items)
                {
                    listBox.RemoveFromSelection(item.Text);
                }

                foreach (var value in values)
                {
                    listBox.AddToSelection(value);
                }
            }
        }

        public void SetText(string txt)
        {
            if (ControlType == "Edit")
            {
                automationElement.Patterns.Value.Pattern.SetValue(txt);
            }
        }

        public string[] GetElementSelectedItems()
        {
            var controlType = automationElement.ControlType.ToString();
            List<string> result = new List<string>();
            if (controlType == "ComboBox")
            {
                ComboBox cbBox =  automationElement.AsComboBox();
                ComboBoxItem[] items = cbBox.SelectedItems;
                
                foreach (var item in items)
                {
                    result.Add(item.Text);
                }
            }
            else if(controlType == "List")
            {
                ListBox listBox = automationElement.AsListBox();
                ListBoxItem[] items = listBox.SelectedItems;
                foreach (var item in items)
                {
                    result.Add(item.Text);
                }
            }
            return result.ToArray();
        }

        public bool GetElementChecked()
        {
            var controlType = automationElement.ControlType.ToString();
            if (controlType == "RadioButton")
            {
                RadioButton radio = automationElement.AsRadioButton();
                return radio.IsChecked;
            }
            else if(controlType == "CheckBox")
            {
                CheckBox ckbox = automationElement.AsCheckBox();
                return ckbox.IsChecked.GetValueOrDefault(false);
            }
            else
            {
                throw new Exception($"元素没有check属性");
            }
        }

        public void SetElementChecked(bool isChecked)
        {
            var controlType = automationElement.ControlType.ToString();
            if (controlType == "RadioButton")
            {
                RadioButton radio = automationElement.AsRadioButton();
                radio.IsChecked = isChecked;
            }
            else if (controlType == "CheckBox")
            {
                CheckBox ckbox = automationElement.AsCheckBox();
                ckbox.IsChecked = isChecked;
            }
            else
            {
                throw new Exception($"元素没有check属性");
            }
        }

        public string GetElementAttribute(string attrName)
        {
            try
            {
                if (attrName.ToLower() == "Value".ToLower())
                {
                    return automationElement.Patterns.Value.Pattern.Value;
                }
                if (attrName.ToLower() == "Text".ToLower())
                {
                    return Text;
                }
                if (attrName.ToLower() == "Idx".ToLower())
                {
                    return Idx;
                }
                if (attrName.ToLower() == "Description".ToLower())
                {
                    return Description;
                }
                if (attrName.ToLower() == "UserDefineId".ToLower())
                {
                    return UserDefineId;
                }
                if (attrName.ToLower() == "ProcessFullPath".ToLower())
                {
                    return ProcessFullPath;
                }
                if (attrName.ToLower() == "ProcessName".ToLower())
                {
                    return ProcessName;
                }
                if (attrName.ToLower() == "Role".ToLower())
                {
                    return Role;
                }
                if (attrName.ToLower() == "Name".ToLower())
                {
                    return Name;
                }
                if (attrName.ToLower() == "ControlType".ToLower())
                {
                    return ControlType;
                }
                if (attrName.ToLower() == "ClassName".ToLower())
                {
                    return ClassName;
                }
            }
            catch(Exception e)
            {
                throw new Exception($"元素不支持获取属性名称{attrName}的值");
            }
            return ""; 
        }

        public void SetElementAttribute(string attrName, string attrValue)
        {
            try
            {
                if(ControlType == "Edit")
                {
                    automationElement.Patterns.Value.Pattern.SetValue(attrValue);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"元素不支持设置属性名称{attrName}的值");
            }
        }

        public Rectangle GetElementRectangle()
        {
            return BoundingRectangle;
        }
    }
}
