using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plugins.Shared.Library.UiAutomation
{
    public interface UiNode
    {
        string UserDefineId { get; }

        string Idx { get; }

        string AutomationId { get; }

        string ClassName { get;}

        string ControlType { get; }

        string Name { get; }

        string Role { get; }

        string Text { get; }


        string Description { get; }

        string ProcessName { get; }

        string ProcessFullPath { get; }

        string Sel { get; set; }

        IntPtr WindowHandle { get; }

        UiNode Parent { get; }

        UiNode AutomationElementParent { get; }

        Rectangle BoundingRectangle { get;}

        List<UiNode> Children { get; }

        bool IsTopLevelWindow { get;}

        UiNode FindRelativeNode(int position, int offsetX, int offsetY);

        List<UiNode> FindAll(TreeScope scope, ConditionBase condition);

        void MouseClick(UiElementClickParams clickParams = null);

        void MouseDoubleClick(UiElementClickParams clickParams = null);

        void MouseRightClick(UiElementClickParams clickParams = null);

        void MouseRightDoubleClick(UiElementClickParams clickParams = null);

        void MouseHover(UiElementHoverParams hoverParams = null);

        void Focus();

        void SetForeground();

        Point GetClickablePoint();

        UiNode GetChildByIdx(int idx);

        void SetElementSelect(string[] values);

        void SetText(string txt);

        string[] GetElementSelectedItems();

        bool GetElementChecked();

        void SetElementChecked(bool isChecked);

        string GetElementAttribute(string attrName);

        void SetElementAttribute(string attrName, string attrValue);

        Rectangle GetElementRectangle();
       
    }
}
