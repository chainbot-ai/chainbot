using System;
using System.Collections.Generic;
using System.Drawing;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace Plugins.Shared.Library.UiAutomation
{
    class SAPUiNode : UiNode
    {
        static SAPUiNode()
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }

        public SAPUiNode(Rectangle rect, IntPtr hWnd)
        {
            this.BoundingRectangle = rect;
            this.WindowHandle = hWnd;
        }

        public SAPUiNode()
        {
        }

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
                return "";
            }
        }

        public string Idx
        {
            get
            {
                return "";
            }
        }

        public string Sel { get; set; }

        public UiNode GetChildByIdx(int idx)
        {
            return null;
        }

        public Rectangle BoundingRectangle { get; set; }

        public List<UiNode> Children
        {
            get
            {
                return null;
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
                return "SAPNode";
            }
        }

        public bool IsTopLevelWindow
        {
            get
            {
                return WindowHandle != IntPtr.Zero;
            }
        }

        public string Name { get; set; }

        public UiNode Parent
        {
            get
            {
                return null;
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
                return "";
            }
        }

        public IntPtr WindowHandle { get; set; }

        public string Description
        {
            get
            {
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
                return Name;
            }
        }

        public void SetForeground()
        {
            if (WindowHandle != IntPtr.Zero)
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
            //TODO WJF
        }

        public void SetText(string txt)
        {
            throw new NotImplementedException();
        }

        public string[] GetElementSelectedItems()
        {
            throw new NotImplementedException();
        }

        public bool GetElementChecked()
        {
            throw new NotImplementedException();
        }

        public void SetElementChecked(bool isChecked)
        {
            throw new NotImplementedException();
        }

        public string GetElementAttribute(string attrName)
        {
            throw new NotImplementedException();
        }

        public void SetElementAttribute(string attrName, string attrValue)
        {
            throw new NotImplementedException();
        }

        public Rectangle GetElementRectangle()
        {
            throw new NotImplementedException();
        }
    }
}