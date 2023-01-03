using System.Collections.Generic;
using System.Xml;
using System;
using System.Drawing;
using FlaUI.Core.WindowsAPI;
using FlaUI.Core.Input;
using FlaUI.Core.Overlay;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using Chainbot.ChromePlugin;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Chainbot.FirefoxPlugin;
using Chainbot.IEPlugin;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Windows.Automation;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.Xml.Linq;
using Plugins.Shared.Library.Extensions;
using System.Drawing.Imaging;
using Chainbot.Sap;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.UiAutomation
{
    public enum AccessibleObjectID : uint
    {
        OBJID_WINDOW = 0u,
        OBJID_SYSMENU = uint.MaxValue,
        OBJID_TITLEBAR = 4294967294u,
        OBJID_MENU = 4294967293u,
        OBJID_CLIENT = 4294967292u,
        OBJID_VSCROLL = 4294967291u,
        OBJID_HSCROLL = 4294967290u,
        OBJID_SIZEGRIP = 4294967289u,
        OBJID_CARET = 4294967288u,
        OBJID_CURSOR = 4294967287u,
        OBJID_ALERT = 4294967286u,
        OBJID_SOUND = 4294967285u
    }

    public enum AccRoles
    {
        ROLE_SYSTEM_ALERT = 8,
        ROLE_SYSTEM_ANIMATION = 54,
        ROLE_SYSTEM_APPLICATION = 14,
        ROLE_SYSTEM_BORDER = 19,
        ROLE_SYSTEM_BUTTONDROPDOWN = 56,
        ROLE_SYSTEM_BUTTONDROPDOWNGRID = 58,
        ROLE_SYSTEM_BUTTONMENU = 57,
        ROLE_SYSTEM_CARET = 7,
        ROLE_SYSTEM_CELL = 29,
        ROLE_SYSTEM_CHARACTER = 0x20,
        ROLE_SYSTEM_CHART = 17,
        ROLE_SYSTEM_CHECKBUTTON = 44,
        ROLE_SYSTEM_CLIENT = 10,
        ROLE_SYSTEM_CLOCK = 61,
        ROLE_SYSTEM_COLUMN = 27,
        ROLE_SYSTEM_COLUMNHEADER = 25,
        ROLE_SYSTEM_COMBOBOX = 46,
        ROLE_SYSTEM_CURSOR = 6,
        ROLE_SYSTEM_DIAGRAM = 53,
        ROLE_SYSTEM_DIAL = 49,
        ROLE_SYSTEM_DIALOG = 18,
        ROLE_SYSTEM_DOCUMENT = 0xF,
        ROLE_SYSTEM_DROPLIST = 47,
        ROLE_SYSTEM_EQUATION = 55,
        ROLE_SYSTEM_GRAPHIC = 40,
        ROLE_SYSTEM_GRIP = 4,
        ROLE_SYSTEM_GROUPING = 20,
        ROLE_SYSTEM_HELPBALLOON = 0x1F,
        ROLE_SYSTEM_HOTKEYFIELD = 50,
        ROLE_SYSTEM_INDICATOR = 39,
        ROLE_SYSTEM_IPADDRESS = 0x3F,
        ROLE_SYSTEM_LINK = 30,
        ROLE_SYSTEM_LIST = 33,
        ROLE_SYSTEM_LISTITEM = 34,
        ROLE_SYSTEM_MENUBAR = 2,
        ROLE_SYSTEM_MENUITEM = 12,
        ROLE_SYSTEM_MENUPOPUP = 11,
        ROLE_SYSTEM_OUTLINE = 35,
        ROLE_SYSTEM_OUTLINEBUTTON = 0x40,
        ROLE_SYSTEM_OUTLINEITEM = 36,
        ROLE_SYSTEM_PAGETAB = 37,
        ROLE_SYSTEM_PAGETABLIST = 60,
        ROLE_SYSTEM_PANE = 0x10,
        ROLE_SYSTEM_PROGRESSBAR = 48,
        ROLE_SYSTEM_PROPERTYPAGE = 38,
        ROLE_SYSTEM_PUSHBUTTON = 43,
        ROLE_SYSTEM_RADIOBUTTON = 45,
        ROLE_SYSTEM_ROW = 28,
        ROLE_SYSTEM_ROWHEADER = 26,
        ROLE_SYSTEM_SCROLLBAR = 3,
        ROLE_SYSTEM_SEPARATOR = 21,
        ROLE_SYSTEM_SLIDER = 51,
        ROLE_SYSTEM_SOUND = 5,
        ROLE_SYSTEM_SPINBUTTON = 52,
        ROLE_SYSTEM_SPLITBUTTON = 62,
        ROLE_SYSTEM_STATICTEXT = 41,
        ROLE_SYSTEM_STATUSBAR = 23,
        ROLE_SYSTEM_TABLE = 24,
        ROLE_SYSTEM_TEXT = 42,
        ROLE_SYSTEM_TITLEBAR = 1,
        ROLE_SYSTEM_TOOLBAR = 22,
        ROLE_SYSTEM_TOOLTIP = 13,
        ROLE_SYSTEM_WHITESPACE = 59,
        ROLE_SYSTEM_WINDOW = 9
    }

    public class UiElement
    {
        [DllImport("user32")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint nCmd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        public static extern int GetDlgCtrlID(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(int uiAction, uint uiParam, string pvParam, int fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(int uiAction, uint uiParam, ref bool pvParam, int fWinIni);

        private static readonly int SPI_GETSCREENREADER = 70;
        private static readonly int SPI_SETSCREENREADER = 71;

        public static void ScreenReaderOn()
        {
            Task.Run(() => {
                bool retVal = false;
                bool bScreenReader = false;
                retVal = SystemParametersInfo(SPI_GETSCREENREADER, 0, ref bScreenReader, 0);
                if (!bScreenReader)
                {
                    retVal = SystemParametersInfo(SPI_SETSCREENREADER, 1, null, 3);
                }
            });
        }

        public static void ScreenReaderOff()
        {
            Task.Run(() => {
                bool retVal = false;
                bool bScreenReader = false;
                retVal = SystemParametersInfo(SPI_GETSCREENREADER, 0, ref bScreenReader, 0);
                if (bScreenReader)
                {
                    retVal = SystemParametersInfo(SPI_SETSCREENREADER, 0, null, 3);
                }
            });
        }

        public enum UiElementType
        {
            IE,
            Chrome,
            Firefox,
            DeskTop,
            Java,
            SAP,
            None
        }

        public delegate void UiElementSelectedEventHandler(UiElement uiElement);

        public delegate void ScreenCaptureSelectedEventHandler(Bitmap captureBitmap,Rectangle rect);

        public delegate void UiElementCancelededEventHandler();

        public static bool IsRecordingWindowOpened { get; set; }

        public static bool IsEnableDragToSelectImage { get; set; }

        public static UiElementSelectedEventHandler OnSelected;


        public static ScreenCaptureSelectedEventHandler OnScreenCaptureSelected;

        public static UiElementCancelededEventHandler OnCanceled;

        internal UiNode uiNode;

        public Point RelativeClickPos { get; set; }

        private static UiElement cacheDesktop;

        private string cachedId;

        private UiElement cachedParent;

        private UiElement automationElementParent;

        private UiElement cachedDirectTopLevelWindow;

        private Rectangle boundingRectangle;

        public static OverlayForm overlayForm;


        internal Bitmap currentInformativeScreenshot;

        internal Bitmap currentScreenshot;

        private Bitmap currentDesktopScreenshot;

        static UiElement()
        {
            overlayForm = new OverlayForm();
        }
        public static void Init()
        {
            JavaUiNode.EnumJvms(true);
        }

        internal UiElement(UiNode node, UiElement parent = null)
        {
            this.uiNode = node;
            this.Parent = parent;
            this.boundingRectangle = node.BoundingRectangle;
        }

        public UiElement Parent
        {
            get
            {
                if (cachedParent == null)
                {
                    if (uiNode.Parent != null)
                    {
                        cachedParent = new UiElement(uiNode.Parent);
                    }
                }

                return cachedParent;
            }

            private set
            {
                cachedParent = value;
            }
        }

        public UiElement AutomationElementParent
        {
            get
            {
                automationElementParent = new UiElement(uiNode.AutomationElementParent);
                return automationElementParent;
            }
        }

        public static UiElement Desktop
        {
            get
            {
                if (cacheDesktop == null)
                {
                    for (int nRetry = 0; nRetry < 20; nRetry++)
                    {
                        try
                        {
                            var _rootElement = UIAUiNode.UIAAutomation.GetDesktop();
                            cacheDesktop = new UiElement(new UIAUiNode(_rootElement));
                            break;
                        }
                        catch (Exception err)
                        {
                            Thread.Sleep(500);
                        }
                    }

                }

                return cacheDesktop;
            }
        }

        public List<UiElement> Children
        {
            get
            {
                var list = new List<UiElement>();
                var children = uiNode.Children;
                foreach (var item in children)
                {
                    list.Add(new UiElement(item, this));
                }

                return list;
            }
        }

        public string ControlType
        {
            get
            {
                if (string.IsNullOrEmpty(uiNode.ControlType))
                {
                    return "Node";
                }
                else
                {
                    return uiNode.ControlType;
                }

            }
        }

        public static bool IsInspectingIE(Point location)
        {
            UIAUiNode node = null;
            return overlayForm.isInspectingIE(location,ref node);
        }

        public string Name
        {
            get
            {
                return uiNode.Name;
            }
        }

        public string AutomationId
        {
            get
            {
                return uiNode.AutomationId;
            }
        }

        public string UserDefineId
        {
            get
            {
                return uiNode.UserDefineId;
            }
        }

        public string ClassName
        {
            get
            {
                return uiNode.ClassName;

            }
        }


        public string ProcessName
        {
            get
            {
                return uiNode.ProcessName;
            }
        }

  
        public string ProcessFullPath
        {
            get
            {
                return uiNode.ProcessFullPath;
            }
        }

        public string Role
        {
            get
            {
                return uiNode.Role;
            }
        }

        public string Description
        {
            get
            {
                return uiNode.Description;
            }
        }

        public string Idx
        {
            get
            {
                return uiNode.Idx;
            }
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
             Replace("\\*", ".*").
             Replace("\\?", ".") + "$";
        }


        public static bool IsWildcardMatchStr(string wildcard,string str)
        {
            return Regex.IsMatch(str, WildcardToRegex(wildcard));
        }

        private static bool isUiElementMatch(UiElement uiElement, XmlElement xmlElement)
        {
             if (xmlElement.Name == "Node" || xmlElement.Name == uiElement.ControlType)
            {
                bool isMatch = true;

                foreach (XmlAttribute attr in xmlElement.Attributes)
                {
                    if (attr.Name == "Name")
                    {
                        if (!IsWildcardMatchStr(RestoreSpecialXmlChar(attr.Value),uiElement.Name))
                        {
                            if (uiElement.uiNode is JavaUiNode)
                            {
                                var javaUiNode = uiElement.uiNode as JavaUiNode;
                                if (javaUiNode.IsTableCell)
                                {
                                    continue;
                                }
                            }

                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "AutomationId")
                    {
                        if (RestoreSpecialXmlChar(attr.Value) != uiElement.AutomationId)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "UserDefineId")
                    {
                        if (RestoreSpecialXmlChar(attr.Value) != uiElement.UserDefineId)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "ClassName")
                    {
                        if (RestoreSpecialXmlChar(attr.Value) != uiElement.ClassName)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "Role")
                    {
                        if (RestoreSpecialXmlChar(attr.Value) != uiElement.Role)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "Description")
                    {
                        if (!IsWildcardMatchStr(RestoreSpecialXmlChar(attr.Value),uiElement.Description))
                        {
                            if (uiElement.uiNode is JavaUiNode)
                            {
                                var javaUiNode = uiElement.uiNode as JavaUiNode;
                                if (javaUiNode.IsTableCell)
                                {
                                    continue;
                                }
                            }

                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "ProcessName")
                    {
                        if (RestoreSpecialXmlChar(attr.Value).ToLower() != uiElement.ProcessName.ToLower())
                        {
                            if((RestoreSpecialXmlChar(attr.Value)+".exe").ToLower() != uiElement.ProcessName.ToLower())
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    else
                    {

                    }
                }

                return isMatch;
            }
            return false;
        }

        private static string TransplateSpecialXmlChar(string str)
        {
            var ret = str;
            ret = ret.Replace("'", "&apos;");
            return ret;
        }


        private static string RestoreSpecialXmlChar(string str)
        {
            var ret = str;
            ret = ret.Replace("&apos;", "'");
            return ret;
        }

        public static string ReplaceXmlRootName(string xmlText,string rootName)
        {
            XDocument doc = XDocument.Parse(xmlText);
            doc.Root.Name = rootName;

            return doc.ToString();
        }

        public string Id
        {
            get
            {
                if (cachedId == null)
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    var itemName = ControlType;
                    var itemElement = xmlDoc.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(Sel))
                    {

                        itemElement = mountChildNodeBySel(xmlDoc, itemElement, Sel);
                    }

                    if (!string.IsNullOrEmpty(Name))
                    {
                        itemElement.SetAttribute("Name", TransplateSpecialXmlChar(Name));
                    }

                    if (!string.IsNullOrEmpty(AutomationId))
                    {
                        itemElement.SetAttribute("AutomationId", TransplateSpecialXmlChar(AutomationId));
                    }

                    if (!string.IsNullOrEmpty(UserDefineId))
                    {
                        itemElement.SetAttribute("UserDefineId", TransplateSpecialXmlChar(UserDefineId));
                    }

                    if (!string.IsNullOrEmpty(ClassName))
                    {
                        itemElement.SetAttribute("ClassName", TransplateSpecialXmlChar(ClassName));
                    }

                    if (!string.IsNullOrEmpty(Role))
                    {
                        itemElement.SetAttribute("Role", TransplateSpecialXmlChar(Role));
                    }
                    
                    if (!string.IsNullOrEmpty(Description))
                    {
                        itemElement.SetAttribute("Description", TransplateSpecialXmlChar(Description));
                    }

                    
                    if (uiNode.IsTopLevelWindow)
                    {
                        if (!string.IsNullOrEmpty(ProcessName))
                        {
                            itemElement.SetAttribute("ProcessName", TransplateSpecialXmlChar(ProcessName));
                        }
                    }

                    if (!string.IsNullOrEmpty(Idx) && Idx != "0")
                    {
                        itemElement.SetAttribute("Idx", TransplateSpecialXmlChar(Idx));
                    }


                    if (ControlType == "IENode" || ControlType == "ChromeNode" || ControlType == "FirefoxNode" || ControlType == "SAPNode")
                    {
                        string buff = itemElement.OuterXml;
                        buff = UiElement.ReplaceXmlRootName(buff, ControlType);
                        //buff = buff.Replace("\"", "\'");
                        cachedId = buff;
                    }
                    else
                        cachedId = itemElement.OuterXml;
                }
                return cachedId;
            }
        }

        public void JavaClick()
        {
            if( uiNode is JavaUiNode)
            {
                var node = uiNode as JavaUiNode;
                node.InternalClick();
            }
        }

        public void SetText(string txt)
        {
            uiNode.SetText(txt);
        }

        private static XmlElement mountChildNodeBySel(XmlDocument xmlDoc,XmlElement xmlNode, string sel)
        {
            XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(sel), new XmlDictionaryReaderQuotas());
            xmlDoc.Load(reader);
            return xmlDoc.DocumentElement;

            //var json = System.Net.WebUtility.HtmlDecode(sel);
            //XmlDocument doc_child = (XmlDocument)JsonConvert.DeserializeXmlNode(json, xmlNode.Name);

            //var tempNode = xmlDoc.ImportNode(doc_child.DocumentElement, true) as XmlElement;

            //foreach (XmlAttribute item in xmlNode.Attributes)
            //{
            //    if (item.Name != "Sel")
            //    {
            //        tempNode.SetAttribute(item.Name, item.Value);
            //    }
            //}

            //return tempNode;
        }


        public bool IsTopLevelWindow
        {
            get
            {
                return uiNode.IsTopLevelWindow;
            }
        }

        public string Sel
        {
            get
            {
                return uiNode.Sel;
            }
        }

        public UiElement DirectTopLevelWindow
        {
            get
            {
                if (cachedDirectTopLevelWindow == null)
                {
                    if (this.IsTopLevelWindow)
                    {
                        cachedDirectTopLevelWindow = this;
                    }
                    else
                    {
                        UiElement topLevelWindowToFind = this.Parent;
                        while (true)
                        {
                            if (topLevelWindowToFind != null)
                            {
                                if (topLevelWindowToFind.IsTopLevelWindow)
                                {
                                    cachedDirectTopLevelWindow = topLevelWindowToFind;
                                    break;
                                }

                                topLevelWindowToFind = topLevelWindowToFind.Parent;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return cachedDirectTopLevelWindow;
            }
        }


        public static Bitmap CaptureDesktop()
        {
            return UIAUiNode.UIAAutomation.GetDesktop().Capture();
        }


        public IntPtr WindowHandle
        {
            get
            {
                return uiNode.WindowHandle;
            }
        }


        public string GlobalId
        {
            get
            {
                return Parent == null ? this.Id : Parent.GlobalId + this.Id;
            }
        }


        public string Selector
        {
            get
            {
                return GlobalId.WrapXmlWithDoubleQuotationMarks();
            }
        }

        public string GlobalIdStyled
        {
            get
            {
                return Parent == null ? this.Id : Parent.GlobalIdStyled + Environment.NewLine + this.Id;
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                return this.boundingRectangle;
            }
            set
            {
                this.boundingRectangle = value;
            }
        }


        public object NativeObject
        {
            get
            {
                if(uiNode is UIAUiNode)
                {
                    return (uiNode as UIAUiNode).automationElement;
                }

                if (uiNode is JavaUiNode)
                {
                    return (uiNode as JavaUiNode).accessibleNode;
                }

                return null;
            }
            
        }

        public bool IsNativeObjectAutomationElement
        {
            get
            {
                return uiNode is UIAUiNode;
            }
        }

        public bool IsNativeObjectAccessibleNode
        {
            get
            {
                return uiNode is JavaUiNode;
            }
        }

        public string Text
        {
            get
            {
                return uiNode.Text;
            }
        }

        public static UiElement FromSelector(string selector, int nRetryNum = 3)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                UiElement ret = null;
                var globalId = selector.Replace("\'", "\"");

                if (nRetryNum == 1)
                {
                    JavaUiNode.EnumJvms(true);
                    ScreenReaderOn();
                }

                for (int nRetry = 0; nRetry < nRetryNum; nRetry++)
                {
                    ret = FromGlobalId(globalId);

                    if (ret != null)
                    {
                        break;
                    }
                    else
                    {
                        JavaUiNode.EnumJvms(true);
                        ScreenReaderOn(); 
                    }

                    Thread.Sleep(500);
                }

                if (nRetryNum == 1)
                {
                    ScreenReaderOff();
                }

                return ret;
            }
            return null;
        }

        public static void MouseWheel(double y)
        {
            Mouse.Scroll(y);
        }
        public static void MouseWheel2(double x)
        {
            Mouse.HorizontalScroll(x);
        }

        private static bool MatchDescendants(IntPtr hWnd, string className, string Name)
        {
            List<IntPtr> list = new List<IntPtr>();
            Queue<IntPtr> queue = new Queue<IntPtr>();
            list.AddRange(UiCommon.FindChildren(hWnd, className, Name));
            queue.Enqueue(hWnd);
            while (queue.Count != 0 && list.Count <= 0)
            {
                IntPtr hWnd2 = queue.Dequeue();
                IntPtr window = UiCommon.GetWindow(hWnd2, 6u);
                if (window != IntPtr.Zero)
                {
                    list.AddRange(UiCommon.FindChildren(window, className, Name));
                    queue.Enqueue(window);
                }
                IntPtr window2 = UiCommon.GetWindow(hWnd2, 5u);
                while (window2 != IntPtr.Zero)
                {
                    list.AddRange(UiCommon.FindChildren(window2, className, Name));
                    queue.Enqueue(window2);
                    window2 = UiCommon.GetWindow(window2, 2u);
                }
            }
            return list.Count > 0;
        }

        private static int MatchCtrlAndAAName(List<IntPtr> children, string ctrlname, int ctrlid, string aaname, int idx)
        {
            if (!string.IsNullOrEmpty(ctrlname))
            {
                string pattern = "^" + Regex.Escape(ctrlname).Replace("\\*", ".*").Replace("\\?", ".");
                for (int i = 0; i < children.Count; i++)
                {
                    try
                    {
                        AutomationElement.AutomationElementInformation automationElementInformation = AutomationElement.FromHandle(children[i]).Current;
                        if (Regex.IsMatch(automationElementInformation.AutomationId, pattern))
                        {
                            return i;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return -1;
            }
            if (ctrlid != 0)
            {
                for (int j = children.Count - 1; j >= 0; j--)
                {
                    if (ctrlid == UiCommon.GetDlgCtrlID(children[j]))
                    {
                        return j;
                    }
                }
            }
            //if (idx == 0 && !string.IsNullOrEmpty(aaname))
            //{
            //    string pattern2 = "^" + Regex.Escape(aaname).Replace("\\*", ".*").Replace("\\?", ".");
            //    for (int k = 0; k < children.Count; k++)
            //    {
            //        //string text = UiCommon.AAWindowNameWapper(children[k]);
            //        //if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text, pattern2))
            //        //{
            //        //    return k;
            //        //}
            //        try
            //        {
            //            using (SystemAccessibleObject systemAccessibleObject = SystemAccessibleObject.FromWindow(children[k], AccessibleObjectID.OBJID_WINDOW))
            //            {
            //                if (!string.IsNullOrEmpty(systemAccessibleObject.Name) && Regex.IsMatch(systemAccessibleObject.Name, pattern2))
            //                {
            //                    return k;
            //                }
            //            }
            //        }
            //        catch (Exception value)
            //        {
            //            Console.WriteLine(value);
            //        }
            //    }
            //    return -1;
            //}
            if (idx < children.Count)
            {
                return idx;
            }
            return -1;
        }


        public static List<IntPtr> FindChildren(IntPtr hWnd, string className, string Name)
        {
            List<IntPtr> list = new List<IntPtr>();
            string pattern = "^" + Regex.Escape(className).Replace("\\*", ".*").Replace("\\?", ".");
            string text = null;
            if (Name != null && Name.Length > 0)
            {
                text = "^" + Regex.Escape(Name).Replace("\\*", ".*").Replace("\\?", ".");
            }
            IntPtr window = GetWindow(hWnd, 6u);
            if (window != IntPtr.Zero)
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                User32.GetClassName(window, stringBuilder, 256);
                if (Regex.IsMatch(stringBuilder.ToString(), pattern))
                {
                    StringBuilder stringBuilder2 = new StringBuilder(256);
                    GetWindowText(window, stringBuilder2, 256);
                    if (text == null || Regex.IsMatch(stringBuilder2.ToString(), text))
                    {
                        list.Add(window);
                    }
                }
            }
            if ((className + Name).IndexOfAny(new char[2] { '*', '?' }) == -1)
            {
                string lpWindowName = ((Name != null && Name.Length > 0) ? Name : null);
                IntPtr hwndChildAfter = IntPtr.Zero;
                while (true)
                {
                    IntPtr intPtr = User32.FindWindowEx(hWnd, hwndChildAfter, className, lpWindowName);
                    if (intPtr == IntPtr.Zero)
                    {
                        break;
                    }
                    list.Add(intPtr);
                    hwndChildAfter = intPtr;
                }
            }
            else
            {
                IntPtr window2 = GetWindow(hWnd, 5u);
                while (window2 != IntPtr.Zero)
                {
                    StringBuilder stringBuilder3 = new StringBuilder(256);
                    User32.GetClassName(window2, stringBuilder3, 256);
                    if (Regex.IsMatch(stringBuilder3.ToString(), pattern))
                    {
                        StringBuilder stringBuilder4 = new StringBuilder(256);
                        GetWindowText(window2, stringBuilder4, 256);
                        if (text == null || Regex.IsMatch(stringBuilder4.ToString(), text))
                        {
                            list.Add(window2);
                        }
                    }
                    window2 = GetWindow(window2, 2u);
                }
            }
            return list;
        }

        public static List<IntPtr> FindDescendants(IntPtr hWnd, string className, string Name)
        {
            List<IntPtr> list = new List<IntPtr>();
            Queue<IntPtr> queue = new Queue<IntPtr>();
            queue.Enqueue(hWnd);
            while (queue.Count != 0)
            {
                IntPtr hWnd2 = queue.Dequeue();
                IntPtr window = GetWindow(hWnd2, 6u);
                if (window != IntPtr.Zero)
                {
                    list.AddRange(FindChildren(window, className, Name));
                    queue.Enqueue(window);
                }
                IntPtr window2 = GetWindow(hWnd2, 5u);
                while (window2 != IntPtr.Zero)
                {
                    list.AddRange(FindChildren(window2, className, Name));
                    queue.Enqueue(window2);
                    window2 = GetWindow(window2, 2u);
                }
            }
            return list;
        }


        public static string Decode(string s)
        {
            if (s == null)
            {
                return s;
            }
            return Regex.Unescape(s.Replace("\\", "\\\\").Replace("%", "\\u"));
        }

        private static List<SystemAccessibleObject> MSAAControlFindChildren(SystemAccessibleObject root, int role, string name)
        {
            List<SystemAccessibleObject> list = new List<SystemAccessibleObject>();
            string text = null;
            if (name != null && name.Length > 0)
            {
                text = name;
            }
            SystemAccessibleObject[] children = root.Children;
            foreach (SystemAccessibleObject systemAccessibleObject in children)
            {
                if (systemAccessibleObject.RoleIndex == role && (text == null || text == systemAccessibleObject.Name))
                {
                    list.Add(systemAccessibleObject);
                }
            }
            return list;
        }

        private static List<SystemAccessibleObject> MSAAControlFindDescendantsByIndex(SystemAccessibleObject root, int role, string name, int nIndex)
        {
            List<SystemAccessibleObject> list = new List<SystemAccessibleObject>();
            Queue<SystemAccessibleObject> queue = new Queue<SystemAccessibleObject>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                SystemAccessibleObject[] children = queue.Dequeue().Children;
                foreach (SystemAccessibleObject systemAccessibleObject in children)
                {
                    list.AddRange(MSAAControlFindChildren(systemAccessibleObject, role, name));
                    queue.Enqueue(systemAccessibleObject);
                }
                if (list.Count > nIndex)
                {
                    break;
                }
            }
            return list;
        }


        private static List<AutomationElement> UIAControlFindChildren(AutomationElement element, int cid, string name, string aid)
        {
            List<AutomationElement> list = new List<AutomationElement>();
            List<Condition> list2 = new List<Condition>();
            list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.LookupById(cid)));
            if (name != null && name.Length > 0)
            {
                list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.NameProperty, name));
            }
            if (aid != null && aid.Length > 0)
            {
                list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.AutomationIdProperty, aid));
                AutomationElement automationElement = element.FindFirst(System.Windows.Automation.TreeScope.Children, new System.Windows.Automation.AndCondition(list2.ToArray()));
                if (automationElement != null)
                {
                    list.Add(automationElement);
                }
                return list;
            }
            Condition condition = list2.First();
            if (list2.Count > 1)
            {
                condition = new System.Windows.Automation.AndCondition(list2.ToArray());
            }
            AutomationElementCollection automationElementCollection = element.FindAll(System.Windows.Automation.TreeScope.Children, condition);
            if (automationElementCollection != null && automationElementCollection.Count > 0)
            {
                foreach (AutomationElement item in automationElementCollection)
                {
                    list.Add(item);
                }
                return list;
            }
            return list;
        }



        private static List<AutomationElement> UIAControlFindDescendants(AutomationElement element, int cid, string name, string aid)
        {
            List<AutomationElement> list = new List<AutomationElement>();
            List<Condition> list2 = new List<Condition>();
            list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.LookupById(cid)));
            if (name != null && name.Length > 0)
            {
                list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.NameProperty, name));
            }
            if (aid != null && aid.Length > 0)
            {
                list2.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.AutomationIdProperty, aid));
                AutomationElement automationElement = element.FindFirst(System.Windows.Automation.TreeScope.Descendants, new System.Windows.Automation.AndCondition(list2.ToArray()));
                if (automationElement != null)
                {
                    list.Add(automationElement);
                }
                return list;
            }
            Condition condition = list2.First();
            if (list2.Count > 1)
            {
                condition = new System.Windows.Automation.AndCondition(list2.ToArray());
            }
            foreach (AutomationElement item in element.FindAll(System.Windows.Automation.TreeScope.Descendants, condition))
            {
                list.Add(item);
            }
            return list;
        }

        private static bool IsValidInternetExplorerServer(IntPtr hanle)
        {
            var checkParent = UiCommon.GetParent((IntPtr)hanle);
            var checkParentParent = UiCommon.GetParent(checkParent);

            StringBuilder sbCheckParent = new StringBuilder(256);
            UiCommon.GetClassName(checkParent, sbCheckParent, sbCheckParent.Capacity);
            if (sbCheckParent.ToString() != "Shell DocObject View")
            {
                return false;
            }

            StringBuilder sbCheckParentParent = new StringBuilder(256);
            UiCommon.GetClassName(checkParentParent, sbCheckParentParent, sbCheckParentParent.Capacity);
            if (sbCheckParentParent.ToString() == "Shell Embedding")
            {
                return false;
            }
            else if (sbCheckParentParent.ToString() == "F12BrowserToolWindow")
            {
                return false;
            }

            return true;
        }

        public static IntPtr GetProcessHandle(string sel)
        {
            try
            {
                JObject jobject = null;
                JArray jarray = null;
                jobject = JObject.Parse(sel);
                jarray = jobject.Value<JArray>("wnd");
                if (jarray.Count == 0)
                {
                    return IntPtr.Zero;
                }
                string text = (string)jarray[0]["app"];
                string className = (string)jarray[0]["cls"];
                string text2 = (string)jarray[0]["title"];
                IntPtr intPtr = UiCommon.GetDesktopWindow();
                List<IntPtr> list = UiCommon.FindChildren(intPtr, className, null);
                if (list.Count == 0)
                {
                    list = UiCommon.FindDescendants(intPtr, className, null);
                }
                string pattern = "^" + Regex.Escape(text).Replace("\\*", ".*").Replace("\\?", ".");
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    int processId;
                    UiCommon.GetWindowThreadProcessId(list[i], out processId);
                    if (!Regex.IsMatch(UiCommon.GetProcessNameWithoutSuffix(processId), pattern) && text != "*")
                    {
                        list.Remove(list[i]);
                    }
                    else
                    {
                        if (text.ToLower() == "iexplore" && (className == "Internet Explorer_Server" || className == "IEFrame"))
                        {
                            StringBuilder sbCheckSelf = new StringBuilder(256);
                            UiCommon.GetClassName(list[i], sbCheckSelf, sbCheckSelf.Capacity);
                            if (sbCheckSelf.ToString() == "Internet Explorer_Server")
                            {
                                if (!IsValidInternetExplorerServer(list[i]))
                                {
                                    list.Remove(list[i]);
                                }
                            }
                        }

                    }
                }
                List<IntPtr> list2 = new List<IntPtr>();
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    long windowLong = UiCommon.GetWindowLong(list[j], -20);
                    if (!UiCommon.IsWindowVisible(list[j]) || (windowLong & 32L) == 32L)
                    {
                        list2.Add(list[j]);
                        list.Remove(list[j]);
                    }
                }
                list.AddRange(list2);
                if (list.Count <= 0)
                {
                    return IntPtr.Zero;
                }
                int num = -1;
                List<int> list3 = new List<int>();
                for (int k = 0; k < list.Count; k++)
                {
                    if (string.IsNullOrEmpty(text2) || text2 == "*")
                    {
                        list3.Add(k);
                        if (jarray.Count > 1 && MatchDescendants(list[k], (string)jarray[1]["cls"], (string)jarray[1]["title"]) && num < 0)
                        {
                            num = k;
                        }
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder(256);
                        UiCommon.GetWindowText(list[k], stringBuilder, 256);
                        string pattern2 = "^" + Regex.Escape(text2).Replace("\\*", ".*").Replace("\\?", ".");
                        if (Regex.IsMatch(stringBuilder.ToString(), pattern2))
                        {
                            list3.Add(k);
                            if (num < 0)
                            {
                                num = k;
                            }
                        }
                    }
                }
                if (list3.Count > 0)
                {
                    if (num < 0)
                    {
                        num = list3.First<int>();
                    }
                    intPtr = list[num];
                    if (UiCommon.IsIconic(intPtr))
                    {
                        UiCommon.ShowWindow(intPtr, 9);
                        Thread.Sleep(100);
                    }
                    for (int l = 1; l < jarray.Count; l++)
                    {
                        string text3 = (string)jarray[l]["cls"];
                        string text4 = (string)jarray[l]["title"];
                        string aaname = UiCommon.Decode((string)jarray[l]["aaname"]);
                        string text5 = UiCommon.Decode((string)jarray[l]["ctrlname"]);
                        int ctrlid = (int)(jarray[l]["ctrlid"] ?? 0);
                        int num2 = (int)(jarray[l]["idx"] ?? 0);
                        List<IntPtr> list4 = UiCommon.FindChildren(intPtr, text3, text4);
                        if (num2 >= list4.Count)
                        {
                            list4.AddRange(UiCommon.FindDescendants(intPtr, text3, text4));
                        }
                        int num3 = -1;
                        if (list4 != null && list4.Count > 0)
                        {
                            num3 = MatchCtrlAndAAName(list4, text5, ctrlid, aaname, num2);
                        }
                        if (num3 < 0 && list4.Count == 0)
                        {
                            List<Condition> list5 = new List<Condition>();
                            list5.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.ClassNameProperty, text3));
                            if (text4 != null && text4.Length > 0)
                            {
                                list5.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.NameProperty, string.IsNullOrEmpty(text5) ? text4 : text5));
                            }
                            Condition condition = list5.First<Condition>();
                            if (list5.Count > 1)
                            {
                                condition = new System.Windows.Automation.AndCondition(list5.ToArray());
                            }
                            AutomationElement automationElement = AutomationElement.FromHandle(intPtr).FindFirst(System.Windows.Automation.TreeScope.Subtree, condition);
                            if (automationElement != null)
                            {
                                List<IntPtr> list6 = list4;
                                AutomationElement.AutomationElementInformation automationElementInformation = automationElement.Current;
                                list6.Add((IntPtr)automationElementInformation.NativeWindowHandle);
                                num3 = list4.Count - 1;
                            }
                            else
                            {
                                foreach (IntPtr intPtr2 in UiCommon.FindChildren(UiCommon.GetDesktopWindow(), text3, text4))
                                {
                                    if (intPtr != intPtr2 && intPtr == UiCommon.GetAncestor(intPtr2, 3u))
                                    {
                                        list4.Add(intPtr2);
                                        num3 = list4.Count - 1;
                                        break;
                                    }
                                }
                            }
                        }
                        if (list4.Count <= 0 || num3 < 0)
                        {
                            return IntPtr.Zero;
                        }
                        intPtr = list4[num3];
                    }
                    return intPtr;
                }
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            return IntPtr.Zero;
        }

        //public static IntPtr GetProcessHandle(string selector)
        //{
        //    JObject jObject = null;
        //    JArray jArray = null;
        //    try
        //    {
        //        jObject = JObject.Parse(selector);
        //        jArray = jObject.Value<JArray>("wnd");
        //    }
        //    catch (Exception)
        //    {
        //        return IntPtr.Zero;
        //    }
        //    if (jArray.Count == 0)
        //    {
        //        return (IntPtr)AutomationElement.RootElement.Current.NativeWindowHandle;
        //    }
        //    string text = (string)jArray[0]["app"];
        //    string className = (string)jArray[0]["cls"];
        //    string text2 = (string)jArray[0]["title"];
        //    IntPtr desktopWindow = User32.GetDesktopWindow();
        //    List<IntPtr> list = FindChildren(desktopWindow, className, null);
        //    if (list.Count == 0)
        //    {
        //        list = FindDescendants(desktopWindow, className, null);
        //    }
        //    if (list.Count > 0)
        //    {
        //        int index = 0;
        //        List<int> list2 = new List<int>();
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            uint procId;
        //            User32.GetWindowThreadProcessId(list[i], out procId);
        //            if (Process.GetProcessById((int)procId).ProcessName != text)
        //            {
        //                continue;
        //            }


        //            if(text.ToLower() == "iexplore" && className == "Internet Explorer_Server")
        //            {
        //                if(!IsValidInternetExplorerServer(list[i]))
        //                {
        //                    continue;
        //                }
        //            }


        //            if (text2 != null && text2.Length > 0)
        //            {
        //                StringBuilder stringBuilder = new StringBuilder(256);
        //                GetWindowText(list[i], stringBuilder, 256);
        //                string text3 = stringBuilder.ToString();
        //                if (text3 != null && text3.Length != 0)
        //                {
        //                    string pattern = "^" + Regex.Escape(text2).Replace("\\*", ".*").Replace("\\?", ".");
        //                    if (Regex.IsMatch(text3, pattern))
        //                    {
        //                        list2.Add(i);
        //                        index = i;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                index = i;
        //                if (jArray.Count > 1 && MatchDescendants(list[i], (string)jArray[1]["cls"], (string)jArray[1]["title"]))
        //                {
        //                    list2.Add(i);
        //                }
        //            }
        //        }
        //        if (list2.Count > 0)
        //        {
        //            index = list2[0];
        //            IntPtr foregroundWindow = GetForegroundWindow();
        //            foreach (int item in list2)
        //            {
        //                if (list[item] == foregroundWindow)
        //                {
        //                    index = item;
        //                    break;
        //                }
        //            }
        //        }
        //        desktopWindow = list[index];
        //        for (int j = 1; j < jArray.Count; j++)
        //        {
        //            string text4 = (string)jArray[j]["cls"];
        //            string text5 = (string)jArray[j]["title"];
        //            string text6 = (string)jArray[j]["aaname"];
        //            int num = (int)(jArray[j]["ctrlid"] ?? ((JToken)0));
        //            int num2 = (int)(jArray[j]["idx"] ?? ((JToken)0));
        //            List<IntPtr> list3 = FindChildren(desktopWindow, text4, text5);
        //            if (list3 == null || list3.Count == 0)
        //            {
        //                list3 = FindDescendants(desktopWindow, text4, text5);
        //                if (list3 == null || list3.Count == 0)
        //                {
        //                    List<Condition> list4 = new List<Condition>();
        //                    list4.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.ClassNameProperty, text4));
        //                    if (text5 != null && text5.Length > 0)
        //                    {
        //                        list4.Add(new System.Windows.Automation.PropertyCondition(AutomationElement.NameProperty, text5));
        //                    }
        //                    Condition condition = list4.First();
        //                    if (list4.Count > 1)
        //                    {
        //                        condition = new System.Windows.Automation.AndCondition(list4.ToArray());
        //                    }
        //                    AutomationElement automationElement = AutomationElement.FromHandle(desktopWindow).FindFirst(System.Windows.Automation.TreeScope.Subtree, condition);
        //                    if (automationElement != null)
        //                    {
        //                        list3.Add((IntPtr)automationElement.Current.NativeWindowHandle);
        //                    }
        //                }
        //            }
        //            if (list3.Count > 0)
        //            {
        //                int index2 = 0;
        //                if (list3.Count > 1)
        //                {
        //                    bool flag = false;
        //                    if (num != 0)
        //                    {
        //                        for (int num3 = list3.Count - 1; num3 >= 0; num3--)
        //                        {
        //                            if (num == GetDlgCtrlID(list3[num3]))
        //                            {
        //                                index2 = num3;
        //                                flag = true;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    if (!flag)
        //                    {
        //                        for (int k = 0; k < list3.Count; k++)
        //                        {
        //                            bool flag2 = true;
        //                            if (text6 != null && text6.Length > 0)
        //                            {
        //                                try
        //                                {
        //                                    SystemAccessibleObject systemAccessibleObject = SystemAccessibleObject.FromWindow(list3[k], AccessibleObjectID.OBJID_WINDOW);
        //                                    if (text6 != systemAccessibleObject.Name)
        //                                    {
        //                                        flag2 = false;
        //                                    }
        //                                    systemAccessibleObject.Dispose();
        //                                }
        //                                catch (Exception value)
        //                                {
        //                                    Console.WriteLine(value);
        //                                }
        //                            }
        //                            if (flag2 && (num2 == 0 || (num2 > 0 && num2 == k)))
        //                            {
        //                                index2 = k;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //                desktopWindow = list3[index2];
        //                continue;
        //            }
        //            return IntPtr.Zero;
        //        }
        //        AutomationElement automationElement2 = AutomationElement.FromHandle(desktopWindow);
        //        if (jObject["ctrl"] != null)
        //        {
        //            JArray jArray2 = jObject.Value<JArray>("ctrl");
        //            if (jArray2[0]["role"] != null)
        //            {
        //                SystemAccessibleObject systemAccessibleObject2 = SystemAccessibleObject.FromWindow(desktopWindow, AccessibleObjectID.OBJID_WINDOW);
        //                for (int l = 0; l < jArray2.Count; l++)
        //                {
        //                    AccRoles result;
        //                    Enum.TryParse<AccRoles>((string)jArray2[l]["role"], out result);
        //                    string name = Decode((string)jArray2[l]["name"]);
        //                    int num4 = (int)(jArray2[l]["idx"] ?? ((JToken)0));
        //                    List<SystemAccessibleObject> list5 = MSAAControlFindChildren(systemAccessibleObject2, (int)result, name);
        //                    if (list5 == null || list5.Count == 0 || num4 >= list5.Count)
        //                    {
        //                        list5 = MSAAControlFindDescendantsByIndex(systemAccessibleObject2, (int)result, name, num4);
        //                    }
        //                    if (list5.Count > 0)
        //                    {
        //                        systemAccessibleObject2 = list5[num4];
        //                        continue;
        //                    }
        //                    return IntPtr.Zero;
        //                }
        //                return (IntPtr)automationElement2.Current.NativeWindowHandle;
        //            }
        //            for (int m = 0; m < jArray2.Count; m++)
        //            {
        //                int cid = (int)(jArray2[m]["cid"] ?? ((JToken)0));
        //                string text7 = (string)jArray2[m]["aid"];
        //                string text8 = Decode((string)jArray2[m]["name"]);
        //                int num5 = (int)(jArray2[m]["idx"] ?? ((JToken)0));
        //                Console.WriteLine("Current ctrl lv {0},{1},{2},{3}", m, text7, text8, num5);
        //                List<AutomationElement> list6 = UIAControlFindChildren(automationElement2, cid, text8, text7);
        //                if (list6 == null || list6.Count == 0 || num5 >= list6.Count)
        //                {
        //                    list6 = UIAControlFindDescendants(automationElement2, cid, text8, text7);
        //                }
        //                if (list6.Count > 0)
        //                {
        //                    int index3 = 0;
        //                    for (int n = 0; n < list6.Count; n++)
        //                    {
        //                        if (num5 == 0 || (num5 > 0 && num5 == n))
        //                        {
        //                            index3 = n;
        //                            break;
        //                        }
        //                    }
        //                    automationElement2 = list6[index3];
        //                    continue;
        //                }

        //                return IntPtr.Zero;
        //            }
        //        }
        //        if (jObject.Property("html") != null)
        //        {
        //            string className2 = automationElement2.Current.ClassName;
        //            try
        //            {
        //                if (className2 == "Internet Explorer_Server")
        //                {
        //                    return (IntPtr)automationElement2.Current.NativeWindowHandle;
        //                }
        //            }
        //            catch (Exception ex2)
        //            {
        //                Console.WriteLine("element null" + ex2.Message);
        //                return IntPtr.Zero;
        //            }
        //        }

        //        string arg = automationElement2.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
        //        Console.WriteLine("result:title={0}", arg);
        //        Console.WriteLine(automationElement2.Current.BoundingRectangle);
        //        return (IntPtr)automationElement2.Current.NativeWindowHandle;
        //    }
        //    return IntPtr.Zero;
        //}

        public static UiElementType GetElementSelectType(string globalId)
        {
            globalId = globalId.Replace(Environment.NewLine, "");
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = globalId;
            if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
            {
                return UiElementType.Chrome;
            }
            else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
            {
                return UiElementType.Firefox;
            }
            else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
            {
                return UiElementType.IE;
            }
            else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
            {
                return UiElementType.SAP;
            }
            return UiElementType.None;
        }


        public static UiElement FromGlobalId(string selector)
        {
            string globalId = selector.Replace(Environment.NewLine, "");
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = globalId;
            UiElement elementToFind = null;
            if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
            {
                //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement,selector);
                try
                {
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    if(szID != null)
                    {
                        string value = Chrome.Instance.GetElementValue(hWnd, szID);
                        string result = Chrome.Instance.GetRectangleFromElem(hWnd, szID);

                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(result))
                        {
                            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                            JObject joValue = (JObject)JsonConvert.DeserializeObject(value);
                            if (jo.Value<int>("retCode") == 0)
                            {
                                Rectangle rect = new Rectangle();
                                rect.X = jo.Value<int>("x");
                                rect.Y = jo.Value<int>("y");
                                rect.Width = jo.Value<int>("width");
                                rect.Height = jo.Value<int>("height");
                                ChromeUiNode node = new ChromeUiNode();
                                node.BoundingRectangle = rect;
                                node.WindowHandle = hWnd;
                                if (joValue.Value<int>("retCode") == 0)
                                    node.Name = joValue.Value<string>("value");
                                else
                                    node.Name = "";
                                elementToFind = new UiElement(node);
                                return elementToFind;
                            }
                        }
                    }
                    return elementToFind;
                }
                catch (Exception e)
                {
                    if (e.Message == "Can't connect to message host!")
                    {
                    }
                }

            }
            if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
            {
                try
                {
                    //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);

                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    if(szID != null)
                    {
                        string value = Firefox.Instance.GetElementValue(hWnd, szID);
                        string result = Firefox.Instance.GetRectangleFromElem(hWnd, szID);
                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(result))
                        {
                            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                            JObject joValue = (JObject)JsonConvert.DeserializeObject(value);
                            if (jo.Value<int>("retCode") == 0)
                            {
                                Rectangle rect = new Rectangle();
                                rect.X = jo.Value<int>("x");
                                rect.Y = jo.Value<int>("y");
                                rect.Width = jo.Value<int>("width");
                                rect.Height = jo.Value<int>("height");
                                FireFoxUiNode node = new FireFoxUiNode();
                                node.BoundingRectangle = rect;
                                node.WindowHandle = hWnd;
                                if (joValue.Value<int>("retCode") == 0)
                                    node.Name = joValue.Value<string>("value");
                                else
                                    node.Name = "";
                                elementToFind = new UiElement(node);
                                return elementToFind;
                            }
                        }
                    }
                    return elementToFind;
                } 
                catch (Exception e)
                {
                    if (e.Message == "Can't connect to message host!")
                    {
                        if ((System.Windows.MessageBox.Show("firefox扩展未安装或禁用，在安装前需要关闭firefox浏览器!点击\"是\"后进行安装。", "警告", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes))
                        {

                        }
                    }
                }
            }
            if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
            {
                try
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }

                    string result = IEServiceWrapper.Instance.GetElementInfo(hWnd, sel);
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                        if (jo.Value<int>("retCode") == 0)
                        {
                            Rectangle rect = new Rectangle();
                            rect.X = jo.Value<int>("x");
                            rect.Y = jo.Value<int>("y");
                            rect.Width = jo.Value<int>("width");
                            rect.Height = jo.Value<int>("height");
                            IEUiNode node = new IEUiNode();
                            node.BoundingRectangle = rect;
                            node.WindowHandle = hWnd;
                            node.Name = jo.Value<string>("value");
                            elementToFind = new UiElement(node);
                            return elementToFind;
                        }
                    }      
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                }
            }
            if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
            {
                //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                try
                {
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    if (szID != null)
                    {
                        string value = Sap.Instance.GetElementValue(hWnd, szID);
                        string result = Sap.Instance.GetRectangleFromElem(hWnd, szID);

                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(result))
                        {
                            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                            JObject joValue = (JObject)JsonConvert.DeserializeObject(value);
                            if (jo.Value<int>("retCode") == 0)
                            {
                                Rectangle rect = new Rectangle();
                                rect.X = jo.Value<int>("x");
                                rect.Y = jo.Value<int>("y");
                                rect.Width = jo.Value<int>("width");
                                rect.Height = jo.Value<int>("height");
                                SAPUiNode node = new SAPUiNode();
                                node.BoundingRectangle = rect;
                                node.WindowHandle = hWnd;
                                if (joValue.Value<int>("retCode") == 0)
                                    node.Name = joValue.Value<string>("value");
                                else
                                    node.Name = "";
                                elementToFind = new UiElement(node);
                                return elementToFind;
                            }
                        }
                    }
                    return elementToFind;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                }

            }

            findUiElement(Desktop, rootXmlElement, ref elementToFind);

            return elementToFind;
        }

        private static bool IsInternetExplorerServerClass(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            UiCommon.GetClassName((IntPtr)hWnd, sb, sb.Capacity);

            if (sb.ToString() == "Internet Explorer_Server")
            {
                return true;
            }

            return false;
        }

        private static string unmountChildFirefoxNodeToSel(XmlElement xmlNode, string selector)
        {
            if (selector.StartsWith("<FirefoxNode>"))
            {
                string sel = JsonConvert.SerializeXmlNode(xmlNode, Newtonsoft.Json.Formatting.None, true);

                var jsonObj = JsonConvert.DeserializeObject(sel) as JObject;

                JObject obj = new JObject();

                if (jsonObj["wnd"] is JArray)
                {
                    obj["wnd"] = jsonObj["wnd"];
                }
                else
                {
                    JArray arr = new JArray();
                    arr.Add(jsonObj["wnd"]);
                    obj["wnd"] = arr;
                }

                if (jsonObj["html"] is JArray)
                {
                    obj["html"] = jsonObj["html"];
                }
                else
                {
                    JArray arr = new JArray();
                    arr.Add(jsonObj["html"]);
                    obj["html"] = arr;
                }

                string output = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
                return output;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                JsonSerializerSettings setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    return setting;
                });

                doc.LoadXml(selector);
                return XmlToJSON(doc);
            }
        }

        private static string unmountChildChromeNodeToSel(XmlElement xmlNode, string selector)
        {
            if (selector.StartsWith("<ChromeNode>"))
            {
                string sel = JsonConvert.SerializeXmlNode(xmlNode, Newtonsoft.Json.Formatting.None, true);

                var jsonObj = JsonConvert.DeserializeObject(sel) as JObject;

                JObject obj = new JObject();

                if (jsonObj["wnd"] is JArray)
                {
                    obj["wnd"] = jsonObj["wnd"];
                }
                else
                {
                    JArray arr = new JArray();
                    arr.Add(jsonObj["wnd"]);
                    obj["wnd"] = arr;
                }

                if (jsonObj["html"] is JArray)
                {
                    obj["html"] = jsonObj["html"];
                }
                else
                {
                    JArray arr = new JArray();
                    arr.Add(jsonObj["html"]);
                    obj["html"] = arr;
                }

                string output = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
                return output;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                JsonSerializerSettings setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    return setting;
                });

                doc.LoadXml(selector);
                return XmlToJSON(doc);
            }
        }
 
        private static string unmountChildSAPNodeToSel(XmlElement xmlNode, string selector)
        {

            XmlDocument doc = new XmlDocument();
            JsonSerializerSettings setting = new JsonSerializerSettings();
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                return setting;
            });

            doc.LoadXml(selector);
            return XmlToJSON(doc);
        }


        private static string XmlToJSON(XmlDocument xmlDoc)
        {
            StringBuilder sbJSON = new StringBuilder();
            //sbJSON.Append("{");
            XmlToJSONnode(sbJSON, xmlDoc.DocumentElement, false);
            //sbJSON.Append("}");
            //sbJSON = sbJSON.Replace("{ \"root\": ", "").Replace("</root>", "");
            return sbJSON.ToString();
        }

        public static void XmlToJSONnode(StringBuilder sbJSON, XmlElement node, bool showNodeName)
        {
            bool isStringFlag = false;
            bool isArrayFlag = false;
            bool isNumberFlag = false;
            if (showNodeName)
                sbJSON.Append("\"" + SafeJSON(node.Name) + "\": ");
            sbJSON.Append("{");

            SortedList childNodeNames = new SortedList();

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.ToLower() == "type" && attr.InnerText.ToLower() == "array")
                    {
                        isArrayFlag = true;
                        sbJSON.Remove(sbJSON.Length - 1, 1);
                    }
                }
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.ToLower() == "type" && attr.InnerText.ToLower() == "number")
                    {
                        isNumberFlag = true;
                    }
                }
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name.ToLower() == "type" && attr.InnerText.ToLower() == "string")
                    {
                        isStringFlag = true;
                    }
                }
            }

            if (node.ChildNodes.Count == 0 && isStringFlag)
            {
                sbJSON.Remove(sbJSON.Length - 2, 2);
                sbJSON.Append("\"\"");
            }
            else
            {

                foreach (XmlNode cnode in node.ChildNodes)
                {
                    if (cnode is XmlText)
                        StoreChildNode(childNodeNames, "value", cnode.InnerText);
                    else if (cnode is XmlElement)
                        StoreChildNode(childNodeNames, cnode.Name, cnode);
                }

                foreach (string childname in childNodeNames.Keys)
                {
                    ArrayList alChild = (ArrayList)childNodeNames[childname];
                    if (alChild.Count == 1 && isArrayFlag)
                    {
                        sbJSON.Append("[ ");
                        OutputNode2(childname, alChild[0], sbJSON, false, ref isStringFlag, isNumberFlag);
                        sbJSON.Append(" ], ");
                    }
                    else if (alChild.Count == 1 && !isArrayFlag)
                        OutputNode1(childname, alChild[0], sbJSON, true, ref isStringFlag, isNumberFlag);
                    else
                    {
                        //if (childname.ToLower() == "item"
                        //    && node.ChildNodes[0].Attributes[0].Name.ToLower() == "type"
                        //     && node.ChildNodes[0].Attributes[0].Value.ToLower() == "object")
                        if (childname.ToLower() == "item"
                         && node.Attributes[0].Name.ToLower() == "type"
                          && node.Attributes[0].Value.ToLower() == "array")
                        {
                            sbJSON.Append("[ ");
                        }
                        else if (isNumberFlag)
                            sbJSON.Append(SafeJSON(childname));
                        else
                            sbJSON.Append(" \"" + SafeJSON(childname) + "\": [ ");
                        foreach (object Child in alChild)
                        {
                            OutputNode1(childname, Child, sbJSON, false, ref isStringFlag, isNumberFlag);
                        }
                        sbJSON.Remove(sbJSON.Length - 2, 2);
                        sbJSON.Append(" ], ");
                    }
                }
                sbJSON.Remove(sbJSON.Length - 2, 2);
                if (isArrayFlag && node.ChildNodes.Count == 0)
                    sbJSON.Append(":[]");
                if (!isStringFlag && isArrayFlag == false)
                    sbJSON.Append(" }");
            }
        }


        private static void StoreChildNode(SortedList childNodeNames, string nodeName, object nodeValue)
        {
            if (nodeValue is XmlElement)
            {
                XmlNode cnode = (XmlNode)nodeValue;
                if (cnode.Attributes.Count == 0)
                {
                    XmlNodeList children = cnode.ChildNodes;
                    if (children.Count == 0)
                        nodeValue = null;
                    else if (children.Count == 1 && (children[0] is XmlText))
                        nodeValue = ((XmlText)(children[0])).InnerText;
                }
            }

            object oValuesAL = childNodeNames[nodeName];
            ArrayList ValuesAL;
            if (oValuesAL == null)
            {
                ValuesAL = new ArrayList();
                childNodeNames[nodeName] = ValuesAL;
            }
            else
                ValuesAL = (ArrayList)oValuesAL;
            ValuesAL.Add(nodeValue);
        }

        private static void OutputNode1(string childname, object alChild, StringBuilder sbJSON, bool showNodeName, ref bool isStrFlag, bool isNumberFlag)
        {
            if (isNumberFlag)
            {
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Remove(sbJSON.Length - 1, 1);
                sbJSON.Append(SafeJSON(sChild));
                isStrFlag = true;
            }
            else if (alChild == null)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                sbJSON.Append("null");
                isStrFlag = false;
            }
            else if (alChild is string)
            {
                //if (showNodeName)
                //    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Remove(sbJSON.Length - 1, 1);
                sbJSON.Append("\"" + SafeJSON(sChild) + "\"");
                isStrFlag = true;
            }
            else
            {
                XmlToJSONnode(sbJSON, (XmlElement)alChild, showNodeName);
                isStrFlag = false;
            }
            sbJSON.Append(", ");
        }

        private static void OutputNode2(string childname, object alChild, StringBuilder sbJSON, bool showNodeName, ref bool isStrFlag, bool isNumberFlag)
        {
            if (isNumberFlag)
            {
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Remove(sbJSON.Length - 1, 1);
                sbJSON.Append(SafeJSON(sChild));
                isStrFlag = true;
            }
            else if (alChild == null)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                sbJSON.Append("null");
                isStrFlag = false;
            }
            else if (alChild is string)
            {
                //if (showNodeName)
                //    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Remove(sbJSON.Length - 1, 1);
                sbJSON.Append("\"" + SafeJSON(sChild) + "\"");
                isStrFlag = true;
            }
            else
            {
                XmlToJSONnode(sbJSON, (XmlElement)alChild, showNodeName);
                isStrFlag = false;
            }
        }

        // Make a string safe for JSON  
        private static string SafeJSON(string sIn)
        {
            StringBuilder sbOut = new StringBuilder(sIn.Length);
            foreach (char ch in sIn)
            {
                if (Char.IsControl(ch) || ch == '\'')
                {
                    int ich = (int)ch;
                    sbOut.Append(@"\u" + ich.ToString("x4"));
                    continue;
                }
                else if (ch == '\"' || ch == '\\' || ch == '/')
                {
                    sbOut.Append('\\');
                }
                sbOut.Append(ch);
            }
            return sbOut.ToString();
        }

        private static string unmountChildIENodeToSel(XmlElement xmlNode,string selector)
        {
            if(selector.StartsWith("<IENode>"))
            {
                string sel = JsonConvert.SerializeXmlNode(xmlNode, Newtonsoft.Json.Formatting.None, true);

                var jsonObj = JsonConvert.DeserializeObject(sel) as JObject;
                if (jsonObj["html"]["index"] != null)
                {
                    jsonObj["html"]["index"] = Convert.ToInt32(jsonObj["html"]["index"].ToString());
                }
                if (jsonObj.Property("frames") == null)
                {
                    jsonObj["html"]["frames"] = new JArray();
                }
                string output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.None);
                return output;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                JsonSerializerSettings setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    //setting.NullValueHandling = NullValueHandling.Ignore;
                    //setting.Formatting = Newtonsoft.Json.Formatting.Indented;
                    //setting.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
                    //setting.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
                    return setting;
                });

                doc.LoadXml(selector);
                return XmlToJSON(doc);
            }
        }

        private static bool findUiElement(UiElement uiElement, XmlElement _xmlElement, ref UiElement elementToFind)
        {
            var xmlElement = _xmlElement.CloneNode(true) as XmlElement;

            //System.Diagnostics.Trace.WriteLine("findUiElement####################" + xmlElement.InnerXml+ "####################");
            var children = uiElement.Children;

            if (isUiElementMatch(uiElement, xmlElement.FirstChild as XmlElement))
            {
                xmlElement.RemoveChild(xmlElement.FirstChild);

                if (!xmlElement.HasChildNodes)
                {
                    elementToFind = uiElement;
                    return true;
                }

                if ((xmlElement.FirstChild as XmlElement).HasAttribute("Idx"))
                {
                    var idxStr = (xmlElement.FirstChild as XmlElement).Attributes["Idx"].Value;
                    var idx = Convert.ToInt32(idxStr);

                    var element = uiElement.GetUiElementByIdx(idx);
                    if (element != null)
                    {
                        if (findUiElement(element, xmlElement, ref elementToFind))
                        {
                            return elementToFind != null;
                        }
                    }
                }

                if ((xmlElement.FirstChild as XmlElement).Name == "JavaNode")
                {
                    if ((xmlElement.FirstChild as XmlElement).HasAttribute("UserDefineId")
                        && uiElement.ControlType == "JavaNode")
                    {
                        var userDefineIdStr = (xmlElement.FirstChild as XmlElement).Attributes["UserDefineId"].Value;
                        var idx = Convert.ToInt32(userDefineIdStr);

                        UiElement element = null;
                        try
                        {
                            element = uiElement.GetUiElementByIdx(idx);
                        }
                        catch (Exception)
                        {
                            element = uiElement.GetUiElementByIdx(0);
                            (xmlElement.FirstChild as XmlElement).Attributes["UserDefineId"].Value = element.UserDefineId;
                        }
                        
                        if (element != null)
                        {
                            if (findUiElement(element, xmlElement, ref elementToFind))
                            {
                                return elementToFind != null;
                            }
                        }

                        return false;
                    }

                }

                foreach (var item in uiElement.Children)
                {
                    if (findUiElement(item, xmlElement, ref elementToFind))
                    {
                        break;
                    }
                }
            }

            return elementToFind != null;
        }

        private UiElement GetUiElementByIdx(int idx)
        {
            var item = uiNode.GetChildByIdx(idx);
            if (item != null)
            {
                return new UiElement(item, this);
            }
            else
            {
                return null;
            }
        }

        public Bitmap CaptureInformativeScreenshot()
        {
            currentDesktopScreenshot = CaptureDesktop();

            if (uiNode.BoundingRectangle.IsEmpty)
            {
                return null;
            }

            Bitmap target = drawInformativeOnUiNode(currentDesktopScreenshot);

            return target;
        }

        public Bitmap CurrentInformativeScreenshot()
        {
            return currentInformativeScreenshot;
        }


        public static Bitmap CaptureScreenshot(Rectangle rect)
        {
            var currentDesktopScreenshot = CaptureDesktop();

            Bitmap target = clipOnScreenshot(currentDesktopScreenshot, rect);

            return target;
        }

        private static Bitmap clipOnScreenshot(Bitmap currentDesktopScreenshot, Rectangle rect)
        {
            Rectangle cropRect = rect;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentDesktopScreenshot, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }


        private Bitmap clipOnUiNode(Bitmap currentDesktopScreenshot)
        {
            Rectangle cropRect = uiNode.BoundingRectangle;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentDesktopScreenshot, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }


        private Bitmap drawInformativeOnUiNode(Bitmap currentDesktopScreenshot)
        {
            var currentDesktopScreenshotClone = (Bitmap)currentDesktopScreenshot.Clone();
            using (Graphics g = Graphics.FromImage(currentDesktopScreenshotClone))
            {
                Pen pen = new Pen(Color.Red, 2);
                g.DrawRectangle(pen, uiNode.BoundingRectangle);
            }

            Rectangle cropRect = uiNode.BoundingRectangle;
            this.boundingRectangle = uiNode.BoundingRectangle;
            cropRect.Inflate(100, 50);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentDesktopScreenshotClone, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }


        public string CaptureInformativeScreenshotToFile(string filePath = null)
        {
            var ret = "";
            if (filePath == null)
            {
                var guid = Guid.NewGuid().ToString("N");

                var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(screenshotsPath))
                {
                    System.IO.Directory.CreateDirectory(screenshotsPath);
                }

                var fileName = guid + @".png";
                ret = fileName;
                filePath = screenshotsPath + @"\" + fileName;
            }

            if(currentInformativeScreenshot == null)
            {
                currentInformativeScreenshot = drawInformativeOnUiNode(currentDesktopScreenshot);
            }

            currentInformativeScreenshot.Save(filePath);

            return ret;
        }


        public static string BitmapToFile(Bitmap informativeScreenshot, string filePath = null)
        {
            var ret = "";
            if(informativeScreenshot != null)
            {
                if (filePath == null)
                {
                    var guid = Guid.NewGuid().ToString("N");

                    var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                    if (!System.IO.Directory.Exists(screenshotsPath))
                    {
                        System.IO.Directory.CreateDirectory(screenshotsPath);
                    }

                    var fileName = guid + @".png";
                    ret = fileName;
                    filePath = screenshotsPath + @"\" + fileName;
                }

                informativeScreenshot.Save(filePath);
            }

            return ret;
        }




        public string CaptureScreenshotToFile(string filePath = null)
        {
            var ret = "";
            if (filePath == null)
            {
                var guid = Guid.NewGuid().ToString("N");

                var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(screenshotsPath))
                {
                    System.IO.Directory.CreateDirectory(screenshotsPath);
                }

                var fileName = guid + @".png";
                ret = fileName;
                filePath = screenshotsPath + @"\" + fileName;
            }

            if (currentScreenshot == null)
            {
                currentScreenshot = clipOnUiNode(currentDesktopScreenshot);
            }

            currentScreenshot.Save(filePath);

            return ret;
        }


        public string CaptureScreenshotToBmpFile(string filePath = null)
        {
            var ret = "";
            if (filePath == null)
            {
                var guid = Guid.NewGuid().ToString("N");

                var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(screenshotsPath))
                {
                    System.IO.Directory.CreateDirectory(screenshotsPath);
                }

                var fileName = guid + @".bmp";
                ret = fileName;
                filePath = screenshotsPath + @"\" + fileName;
            }

            if (currentScreenshot == null)
            {
                currentScreenshot = clipOnUiNode(currentDesktopScreenshot);
            }

            currentScreenshot.Save(filePath, ImageFormat.Bmp);

            return ret;
        }



        public static string CaptureScreenshotToFile(Bitmap captureBitmap, string filePath = null)
        {
            var ret = "";
            if (filePath == null)
            {
                var guid = Guid.NewGuid().ToString("N");

                var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(screenshotsPath))
                {
                    System.IO.Directory.CreateDirectory(screenshotsPath);
                }

                var fileName = guid + @".png";
                ret = fileName;
                filePath = screenshotsPath + @"\" + fileName;
            }

            captureBitmap.Save(filePath);

            return ret;
        }

        public static string CaptureScreenshotToBmpFile(Bitmap captureBitmap, string filePath = null)
        {
            var ret = "";
            if (filePath == null)
            {
                var guid = Guid.NewGuid().ToString("N");

                var screenshotsPath = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(screenshotsPath))
                {
                    System.IO.Directory.CreateDirectory(screenshotsPath);
                }

                var fileName = guid + @".bmp";
                ret = fileName;
                filePath = screenshotsPath + @"\" + fileName;
            }

            captureBitmap.Save(filePath, ImageFormat.Bmp);

            return ret;
        }

        public static void StartElementHighlight()
        {
            StartHighlight(false);
        }

        public static void StartWindowHighlight()
        {
            StartHighlight(true);
        }

        private static void StartHighlight(bool isWindowHighlight)
        {
            if(!UiElement.IsRecordingWindowOpened)
            {
                SharedObject.Instance.ShowMainWindowMinimized();
            }
           
            overlayForm.IsWindowHighlight = isWindowHighlight;
            overlayForm.StartHighlight();
        }

        #region element operate

        public void DrawHighlight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
            var colorName = color ?? Color.Red;
            var rectangle = uiNode.BoundingRectangle;
            this.BoundingRectangle = uiNode.BoundingRectangle;
            if (!rectangle.IsEmpty)
            {
                var durationInMs = (int)(duration ?? TimeSpan.FromSeconds(2)).TotalMilliseconds;

                var overlayManager = new WinFormsOverlayManager();
                if (blocking)
                {
                    overlayManager.ShowBlocking(rectangle, colorName, durationInMs);
                }
                else
                {
                    overlayManager.Show(rectangle, colorName, durationInMs);
                }
            }
        }

        public void SetForeground()
        {
            var directWindow = DirectTopLevelWindow;
            if (directWindow != null)
            {
                UiCommon.ForceShow(directWindow.WindowHandle);
                DirectTopLevelWindow.uiNode.SetForeground();
            }

        }


        public UiElement FindRelativeElement(int position, int offsetX, int offsetY)
        {
            UiNode relativeNode = uiNode.FindRelativeNode(position, offsetX, offsetY);
            UiElement relativeElement = new UiElement(relativeNode);
            return relativeElement;
        }

        public List<UiElement> FindAllByFilter(FlaUI.Core.Definitions.TreeScope scope, ConditionBase condition, string filterStr)
        {
            List<UiElement> uiList = new List<UiElement>();
            List<UiElement> foundUiList = new List<UiElement>();

            filterStr = filterStr.Replace(Environment.NewLine, "");
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = filterStr;

            var filterElement = rootXmlElement.CloneNode(true) as XmlElement;
            foundUiList = FindAll(scope, condition);

            foreach(var element in foundUiList)
            {
                if (isUiElementMatch(element as UiElement, filterElement))
                {
                    uiList.Add(element);
                }
            }         
            return uiList;
        }


        public List<UiElement> FindAll(FlaUI.Core.Definitions.TreeScope scope, ConditionBase condition)
        {
            List<UiElement> uiList = new List<UiElement>();

            var list = new List<UiElement>();
            var children = uiNode.FindAll(scope, condition);
            foreach (var item in children)
            {
                list.Add(new UiElement(item, this));
            }
            return list;
        }


        public Point GetClickablePoint()
        {
            return uiNode.GetClickablePoint();
        }


        public void Focus()
        {
            uiNode.Focus();
        }


        public static void Wait(string selector, bool flag, int timeOut = 10000)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                DateTime now = DateTime.Now;
                UiElement element = null;
                string globalId = selector.Replace("\'", "\"");

                ScreenReaderOn();

                while (true)
                {
                    element = FromGlobalId(globalId);
                    if ((flag && element != null) || (!flag && element == null))
                    {
                        break;
                    }

                    if (DateTime.Now.Subtract(now).TotalMilliseconds >= (double)timeOut)
                    {
                        throw new Exception("等待元素超时");
                    }

                    Thread.Sleep(500);
                }
            }
        }

        #endregion


        public static void BrowserClick(string selector,int ClickType, int MouseButton)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement,selector);
                    try
                    {
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Chrome.Instance.ClickElement(hWnd, szID, false);
                    }
                    catch (Exception e)
                    {
                       
                    }

                }
                if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    try
                    {
                        //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                        string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Firefox.Instance.ClickElement(hWnd, szID, false);
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
                if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    try
                    {
                        string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        if (!IsInternetExplorerServerClass(hWnd))
                        {
                          UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                          {
                              StringBuilder sb = new StringBuilder(256);
                              UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                              if (sb.ToString() == "Internet Explorer_Server")
                              {
                                  hWnd = (IntPtr)hWndChild;
                                  return false;
                              }
                              return true;
                          }, 0);
                       }

                       IEServiceWrapper.Instance.ClickElement(hWnd, sel);
                    }
                    catch (Exception e)
                    {
                        //throw new Exception(e.Message);
                    }
                }
                if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    try
                    {
                        string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Sap.Instance.ClickElement(hWnd, szID, false);
                    }
                    catch (Exception e)
                    {
                        //throw new Exception(e.Message);
                    }
                }
            }
        }

        public static void BrowserTypeInto(string selector, string value)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");
                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    try
                    {
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Chrome.Instance.SetElementValue(hWnd, szID, value, false);
                    }
                    catch (Exception e)
                    {
                        
                    }

                }
                if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    try
                    {
                        //string sel = xmlDoc.FirstChild.FirstChild.Attributes["Sel"].Value;
                        string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Firefox.Instance.SetElementValue(hWnd, szID, value, false);
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
                if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    try
                    {
                        string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        if (!IsInternetExplorerServerClass(hWnd))
                        {
                            UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                            {
                                StringBuilder sb = new StringBuilder(256);
                                UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                                if (sb.ToString() == "Internet Explorer_Server")
                                {
                                    hWnd = (IntPtr)hWndChild;
                                    return false;
                                }
                                return true;
                            }, 0);
                        }

                        IEServiceWrapper.Instance.SetElementValue(hWnd, sel, value);
                    }
                    catch (Exception e)
                    {
                        //throw new Exception(e.Message);
                    }
                }
                if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {      
                    try
                    {
                        string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                        IntPtr hWnd = GetProcessHandle(sel);
                        string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                        Sap.Instance.SetElementValue(hWnd, szID, value, false);
                    }
                    catch (Exception e)
                    {
                        //throw new Exception(e.Message);
                    }
                }
            }
        }

        public static void SetElementSelect(string selector, string[] value)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Chrome.Instance.SetElementSelectedItems(hWnd, szID, value, 2);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Firefox.Instance.SetElementSelectedItems(hWnd, szID, value, 2);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }

                    IEServiceWrapper.Instance.SetElementSelectedItems(hWnd, sel, value);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Sap.Instance.SetElementSelectedItems(hWnd, szID, JArray.FromObject(value).ToString(), 2);
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    uiElement.SetElementSelect(value);
                }
            }
        }

        private void SetElementSelect(string[] value)
        {
            uiNode.SetElementSelect(value);
        }

        public static void SetElementChecked(string selector,bool bcheck)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Chrome.Instance.SetElementChecked(hWnd, szID,bcheck,false);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Firefox.Instance.SetElementChecked(hWnd, szID, bcheck, false);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }

                    IEServiceWrapper.Instance.SetElementChecked(hWnd, sel, bcheck);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Sap.Instance.SetElementChecked(hWnd, szID, bcheck, false);
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    uiElement.SetElementChecked(bcheck);
                }
            }
        }

        private void SetElementChecked(bool isChecked)
        {
            uiNode.SetElementChecked(isChecked);
        }

        public static void SetElementAttribute(string selector, string attrName, string attrValue)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Chrome.Instance.SetElementAttribute(hWnd, szID, attrName,attrValue);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Firefox.Instance.SetElementAttribute(hWnd, szID, attrName, attrValue);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }

                    IEServiceWrapper.Instance.SetElementAttribute(hWnd, sel, attrName, attrValue);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Sap.Instance.SetElementAttribute(hWnd, szID, attrName, attrValue);
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    uiElement.SetElementAttribute(attrName, attrValue);
                }
            }
        }

        private void SetElementAttribute(string attrName, string attrValue)
        {
            uiNode.SetElementAttribute(attrName, attrValue);
        }

        public static string GetElementAttribute(string selector, string attrName)
        {
            string attr = "";
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    attr = Chrome.Instance.GetElementAttribute(hWnd, szID, attrName);

                    JObject obj = JObject.Parse(attr);
                    int code = obj.Value<int>("retCode");
                    attr = obj.Value<string>("attr");
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    attr = Firefox.Instance.GetElementAttribute(hWnd, szID, attrName);

                    JObject obj = JObject.Parse(attr);
                    int code = obj.Value<int>("retCode");
                    attr = obj.Value<string>("attr");
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }

                    attr = IEServiceWrapper.Instance.GetElementAttribute(hWnd,sel,attrName);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    Sap.Instance.GetElementAttribute(hWnd, szID, attrName);
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    attr = uiElement.GetElementAttribute(attrName);
                }
            }
            return attr;
        }

        private string GetElementAttribute(string attrName)
        {
            return uiNode.GetElementAttribute(attrName);
        }

        public static string[] GetElementChildrens(string selector)
        {
            string childrens = "";
            List<string> listResult = new List<string>();
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    childrens = Chrome.Instance.GetElementChildren(hWnd, szID);
                    JObject obj = JObject.Parse(childrens);
                    JArray arr = obj.Value<JArray>("children");
                    if (arr != null)
                    {
                        listResult.Clear();
                        for (int i = 0; i< arr.Count;i++)
                        {
                            string result = Chrome.Instance.GetElementSelector(hWnd,arr[i].ToString());
                            XmlDocument xmlDocs = new XmlDocument();
                            var itemName = "ChromeNode";
                            var itemElement = xmlDocs.CreateElement(itemName);

                            if (!string.IsNullOrEmpty(result))
                            {
                                itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                            }

                            string buff = itemElement.OuterXml;
                            buff = UiElement.ReplaceXmlRootName(buff, itemName);
                            //buff = buff.Replace("\"", "\'");
                            listResult.Add(buff);
                        }
                    }
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    childrens = Firefox.Instance.GetElementChildren(hWnd, szID);
                    JObject obj = JObject.Parse(childrens);
                    JArray arr = obj.Value<JArray>("children");
                    if (arr != null)
                    {
                        listResult.Clear();
                        for (int i = 0; i < arr.Count; i++)
                        {
                            string result = Firefox.Instance.GetElementSelector(hWnd, arr[i].ToString());
                            XmlDocument xmlDocs = new XmlDocument();
                            var itemName = "FirefoxNode";
                            var itemElement = xmlDocs.CreateElement(itemName);

                            if (!string.IsNullOrEmpty(result))
                            {
                                itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                            }

                            string buff = itemElement.OuterXml;
                            buff = UiElement.ReplaceXmlRootName(buff, itemName);
                            //buff = buff.Replace("\"", "\'");
                            listResult.Add(buff);
                        }
                    }
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }
                        
                    listResult.Clear();
                    List<string> listChildren = IEServiceWrapper.Instance.GetElementChildren(hWnd, sel);
                    foreach(string item in listChildren)
                    {
                        XmlDocument xmlDocs = new XmlDocument();
                        var itemName = "IENode";
                        var itemElement = xmlDocs.CreateElement(itemName);

                        if (!string.IsNullOrEmpty(item))
                        {
                            XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(item), new XmlDictionaryReaderQuotas());
                            xmlDoc.Load(reader);
                            itemElement = xmlDoc.DocumentElement;
                            string buff = itemElement.OuterXml;
                            buff = UiElement.ReplaceXmlRootName(buff, itemName);
                            //buff = buff.Replace("\"", "\'");
                            listResult.Add(buff);
                        }
                    }
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    childrens = Sap.Instance.GetElementChildren(hWnd, szID);
                    JObject obj = JObject.Parse(childrens);
                    JArray arr = obj.Value<JArray>("children");
                    if (arr != null)
                    {
                        listResult.Clear();
                        for (int i = 0; i < arr.Count; i++)
                        {
                            string result = Sap.Instance.GetElementSelector(hWnd, arr[i].ToString());
                            XmlDocument xmlDocs = new XmlDocument();
                            var itemName = "SAPNode";
                            var itemElement = xmlDocs.CreateElement(itemName);

                            if (!string.IsNullOrEmpty(result))
                            {
                                itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                            }

                            string buff = itemElement.OuterXml;
                            buff = UiElement.ReplaceXmlRootName(buff, itemName);
                            //buff = buff.Replace("\"", "\'");
                            listResult.Add(buff);
                        }
                    }
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    foreach(var child in uiElement.Children)
                    {
                        listResult.Add(child.GlobalId);
                    }
                }
            }
            return listResult.ToArray();
        }

        public static string GetElementParents(string selector, Int32 level)
        {
            string result = "";
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);

                    while (level-- > 0)
                    {
                        string elementParentId = Chrome.Instance.GetElementParentId(hWnd, szID);
                        if (string.IsNullOrEmpty(elementParentId))
                        {
                            break;
                        }
                        szID = elementParentId;
                    }
                    result = Chrome.Instance.GetElementSelector(hWnd, szID);

                    XmlDocument xmlDocs = new XmlDocument();
                    var itemName = "ChromeNode";
                    var itemElement = xmlDocs.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(result))
                    {
                        itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                    }
                    string buff = itemElement.OuterXml;
                    buff = UiElement.ReplaceXmlRootName(buff, itemName);
                    //buff = buff.Replace("\"", "\'");
                    result = buff;
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);

                    while (level-- > 0)
                    {
                        string elementParentId = Firefox.Instance.GetElementParentId(hWnd, szID);
                        if (string.IsNullOrEmpty(elementParentId))
                        {
                            break;
                        }
                        szID = elementParentId;
                    }
                    result = Firefox.Instance.GetElementSelector(hWnd, szID);
                    XmlDocument xmlDocs = new XmlDocument();
                    var itemName = "FirefoxNode";
                    var itemElement = xmlDocs.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(result))
                    {
                        itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                    }
                    string buff = itemElement.OuterXml;
                    buff = UiElement.ReplaceXmlRootName(buff, itemName);
                    //buff = buff.Replace("\"", "\'");
                    result = buff;
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }
                       

                    result = IEServiceWrapper.Instance.GetElementParent(hWnd, sel, level);
                    XmlDocument xmlDocs = new XmlDocument();
                    var itemName = "IENode";
                    var itemElement = xmlDocs.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(result))
                    {
                        itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                        string buff = itemElement.OuterXml;
                        buff = UiElement.ReplaceXmlRootName(buff, itemName);
                        //buff = buff.Replace("\"", "\'");
                        result = buff;
                    }
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);

                    while (level-- > 0)
                    {
                        string elementParentId = Sap.Instance.GetElementParentId(hWnd, szID);
                        if (string.IsNullOrEmpty(elementParentId))
                        {
                            break;
                        }
                        szID = elementParentId;
                    }
                    result = Sap.Instance.GetElementSelector(hWnd, szID);

                    XmlDocument xmlDocs = new XmlDocument();
                    var itemName = "SAPNode";
                    var itemElement = xmlDocs.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(result))
                    {
                        itemElement = mountChildNodeBySel(xmlDocs, itemElement, result);
                    }
                    string buff = itemElement.OuterXml;
                    buff = UiElement.ReplaceXmlRootName(buff, itemName);
                    //buff = buff.Replace("\"", "\'");
                    result = buff;
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    result = uiElement.Parent.GlobalId;
                }
            }
            return result;
        }

        public static bool GetElementChecked(string selector)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Chrome.Instance.GetElementChecked(hWnd, szID);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Firefox.Instance.GetElementChecked(hWnd, szID);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }
                        
                    result = IEServiceWrapper.Instance.GetElementChecked(hWnd, sel);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Sap.Instance.GetElementChecked(hWnd, szID);
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    result = uiElement.GetElementChecked();
                }
            }
            return result;
        }

        public bool GetElementChecked()
        {
            return uiNode.GetElementChecked();
        }

        public static string[] GetElementSelect(string selector)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");

                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();

                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    string result = Chrome.Instance.GetElementSelectedItems(hWnd, szID, 0);
                    JObject obj = JObject.Parse(result);
                    JArray arr = obj.Value<JArray>("selectedItems");
                    return arr.ToObject<List<string>>().ToArray();
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    string result = Firefox.Instance.GetElementSelectedItems(hWnd, szID, 0);
                    JObject obj = JObject.Parse(result);
                    JArray arr = obj.Value<JArray>("selectedItems");
                    return arr.ToObject<List<string>>().ToArray();
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }
                        
                    return IEServiceWrapper.Instance.GetElementSelectedItems(hWnd, sel).ToArray();
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    string result = Sap.Instance.GetElementSelectedItems(hWnd, szID, 0);
                    JObject obj = JObject.Parse(result);
                    JArray arr = obj.Value<JArray>("selectedItems");
                    return arr.ToObject<List<string>>().ToArray();
                }
                else
                {
                    var uiElement = UiElement.FromSelector(selector);
                    return uiElement.GetElementSelectedItems();
                }
            }
            return null;
        }

        private string[] GetElementSelectedItems()
        {
            return uiNode.GetElementSelectedItems();
        }

        public static string GetElementRect(string selector)
        {
            string result = "";
            if (!string.IsNullOrEmpty(selector))
            {
                var globalId = selector.Replace("\'", "\"");
                globalId = globalId.Replace(Environment.NewLine, "");
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootXmlElement);
                rootXmlElement.InnerXml = globalId;
                if (xmlDoc.FirstChild.FirstChild.Name == "ChromeNode")
                {
                    string sel = unmountChildChromeNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Chrome.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Chrome.Instance.GetRectangleFromElem(hWnd, szID);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "FirefoxNode")
                {
                    string sel = unmountChildFirefoxNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Firefox.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Firefox.Instance.GetRectangleFromElem(hWnd, szID);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "IENode")
                {
                    string sel = unmountChildIENodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    if (!IsInternetExplorerServerClass(hWnd))
                    {
                        UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                        {
                            StringBuilder sb = new StringBuilder(256);
                            UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                            if (sb.ToString() == "Internet Explorer_Server")
                            {
                                hWnd = (IntPtr)hWndChild;
                                return false;
                            }
                            return true;
                        }, 0);
                    }
                        
                    result = IEServiceWrapper.Instance.GetElementInfo(hWnd, sel);
                }
                else if (xmlDoc.FirstChild.FirstChild.Name == "SAPNode")
                {
                    string sel = unmountChildSAPNodeToSel(xmlDoc.FirstChild.FirstChild as XmlElement, selector);
                    IntPtr hWnd = GetProcessHandle(sel);
                    string szID = Sap.Instance.ElementCacheIdFromSelector(hWnd, sel);
                    result = Sap.Instance.GetRectangleFromElem(hWnd, szID);
                }
                else 
                {
                    var uiElement = UiElement.FromSelector(selector);
                    Rectangle rect = uiElement.GetElementRectangle();
                    JObject jobject = new JObject();
                    jobject["x"] = rect.Left;
                    jobject["y"] = rect.Top;
                    jobject["width"] = rect.Width;
                    jobject["height"] = rect.Height;
                    return JsonConvert.SerializeObject(jobject); 
                }
            }
            return result;
        }

        private Rectangle GetElementRectangle()
        {
            return uiNode.GetElementRectangle();
        }

        #region keyboard operate event
        public static void KeyboardPress(VirtualKey virtualKey)
        {
            Keyboard.Press((VirtualKeyShort)virtualKey);
        }

        public static void KeyboardRelease(VirtualKey virtualKey)
        {
            Keyboard.Release((VirtualKeyShort)virtualKey);
        }

        public static void KeyboardType(string text)
        {
            Keyboard.Type(text);
        }
        #endregion

        #region keyboard operate event
        public void MouseClick(UiElementClickParams clickParams = null)
        {
            SetForeground();
            uiNode.MouseClick(clickParams);
        }

        public void MouseDoubleClick(UiElementClickParams clickParams = null)
        {
            SetForeground();
            uiNode.MouseDoubleClick(clickParams);
        }

        public void MouseRightClick(UiElementClickParams clickParams = null)
        {
            SetForeground();
            uiNode.MouseRightClick(clickParams);
        }

        public void MouseRightDoubleClick(UiElementClickParams clickParams = null)
        {
            SetForeground();
            uiNode.MouseRightDoubleClick(clickParams);
        }

        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            SetForeground();
            uiNode.MouseHover(hoverParams);
        }

        public static void MouseDrag(MouseButton mouseButton, Point startingPoint, Point endingPoint)
        {
            Mouse.Drag( startingPoint, endingPoint.X-startingPoint.X,endingPoint.Y-startingPoint.Y, (FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseUp(MouseButton mouseButton)
        {
            Mouse.Up((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseDown(MouseButton mouseButton)
        {
            Mouse.Down((FlaUI.Core.Input.MouseButton)mouseButton);
        }


        private static void UpdateMoveToSpeed()
        {
            Mouse.MovePixelsPerMillisecond = 3;
            Mouse.MovePixelsPerStep = 1;
        }

        public static void MouseMoveTo(Point newPosition)
        {
            UpdateMoveToSpeed();
            Mouse.MoveTo(newPosition);
        }

        public static void MouseMoveTo(int newX, int newY)
        {
            UpdateMoveToSpeed();
            Mouse.MoveTo(newX, newY);
        }

        public static void MouseSetPostion(Point newPosition)
        {
            Mouse.Position = newPosition;
        }
        public static void MouseSetPostion(int newX, int newY)
        {
            Mouse.Position = new Point(newX,newY);
        }


        public static void MouseAction(ClickType clickType, MouseButton mouseButton)
        {
            switch (clickType)
            {
                case ClickType.Single:
                    MouseClick(mouseButton);
                    break;
                case ClickType.Double: 
                    MouseDoubleClick(mouseButton);
                    break;
                case ClickType.Down:
                    MouseDown(mouseButton);
                    break;
                case ClickType.Up:
                    MouseUp(mouseButton);
                    break;
                default:
                    break;
            }
        }

        public static void MouseAction(ClickType clickType, MouseButton mouseButton, Point point)
        {
            switch (clickType)
            {
                case ClickType.Single:
                    MouseClick(point,mouseButton);
                    break;
                case ClickType.Double:
                    MouseDoubleClick(point,mouseButton);
                    break;
                case ClickType.Down:
                    MouseDown(mouseButton);
                    break;
                case ClickType.Up:
                    MouseUp(mouseButton);
                    break;
                default:
                    break;
            }
        }

        public static void MouseVerticalScroll(double lines)
        {
            Mouse.Scroll(lines);
        }

        public static void MouseHorizontalScroll(double lines)
        {
            Mouse.HorizontalScroll(lines);
        }

        public static void MouseClick(MouseButton mouseButton)
        {
            Mouse.Click((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseClick(Point point,MouseButton mouseButton)
        {
            Mouse.Click(point,(FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseDoubleClick(MouseButton mouseButton)
        {
            Mouse.DoubleClick((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseDoubleClick(Point point, MouseButton mouseButton)
        {
            Mouse.DoubleClick(point,(FlaUI.Core.Input.MouseButton)mouseButton);
        }

        public static void MouseLeftClick()
        {
            Mouse.LeftClick();
        }

        public static void MouseLeftClick(Point point)
        {
            Mouse.LeftClick(point);
        }

        public static void MouseRightClick()
        {
            Mouse.RightClick();
        }

        public static void MouseRightClick(Point point)
        {
            Mouse.RightClick(point);
        }

        public static void MouseLeftDoubleClick()
        {
            Mouse.LeftDoubleClick();
        }

        public static void MouseLeftDoubleClick(Point point)
        {
            Mouse.LeftDoubleClick(point);
        }

        public static void MouseRightDoubleClick()
        {
            Mouse.RightDoubleClick();
        }

        public static void MouseRightDoubleClick(Point point)
        {
            Mouse.RightDoubleClick(point);
        }

        #endregion

        public static UiElement FromScreenPoint(Point screenPoint)
        {
            FlaUI.Core.AutomationElements.AutomationElement element = null;

            for (int i = 0; i <= 10; i++)
            {
                try
                {
                    element = UIAUiNode.UIAAutomation.FromPoint(screenPoint);
                    element = overlayForm.InspectChildren(element, screenPoint, element.BoundingRectangle);

                    break;
                }
                catch (Exception err)
                {
                    Thread.Sleep(100);
                }
            }

            if (element == null)
            {
                IntPtr hWnd = UiCommon.WindowFromPoint(screenPoint);

                if (hWnd != IntPtr.Zero)
                {
                    element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                }
            }

            if (element == null)
            {
                return null;
            }


            UiNode currentHighlightElement = new UIAUiNode(element);

            if (overlayForm.inspectIEUiNode(screenPoint))
            {
                overlayForm.updateIESelector();
                currentHighlightElement = overlayForm.CurrentHighlightElement;
            }

            if (overlayForm.inspectJavaUiNode(element, screenPoint))
            {
                currentHighlightElement = overlayForm.CurrentHighlightElement;
            }

            if (overlayForm.inspectChromeUiNode(element, screenPoint))
            {
                overlayForm.updateChromeSelector();
                currentHighlightElement = overlayForm.CurrentHighlightElement;
            }

            if (overlayForm.inspectFireFoxUiNode(element, screenPoint))
            {
                overlayForm.updateFireFoxSelector();
                currentHighlightElement = overlayForm.CurrentHighlightElement;
            }

            var rect = currentHighlightElement.BoundingRectangle;

            var retUiElement = new UiElement(currentHighlightElement);

            var currentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);

            retUiElement.RelativeClickPos = currentHighlightElementRelativeClickPos;
            retUiElement.currentInformativeScreenshot = retUiElement.CaptureInformativeScreenshot();

            return retUiElement;
        }
    }
}