using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using Accessibility;
using mshtml;

namespace Plugins.Shared.Library.UiAutomation
{
    internal class SystemAccessibleObject : IDisposable
    {
        public struct POINT
        {
            public int x;

            public int y;

            public POINT(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public override string ToString()
            {
                return "X:" + x + ", Y:" + y;
            }
        }


        public enum OLECMDF
        {
            OLECMDF_DEFHIDEONCTXTMENU = 0x20,
            OLECMDF_ENABLED = 2,
            OLECMDF_INVISIBLE = 0x10,
            OLECMDF_LATCHED = 4,
            OLECMDF_NINCHED = 8,
            OLECMDF_SUPPORTED = 1
        }

        public enum OLECMDID
        {
            OLECMDID_PAGESETUP = 8,
            OLECMDID_PRINT = 6,
            OLECMDID_PRINTPREVIEW = 7,
            OLECMDID_PROPERTIES = 10,
            OLECMDID_SAVEAS = 4
        }

        public enum OLECMDEXECOPT
        {
            OLECMDEXECOPT_DODEFAULT,
            OLECMDEXECOPT_PROMPTUSER,
            OLECMDEXECOPT_DONTPROMPTUSER,
            OLECMDEXECOPT_SHOWHELP
        }

        [ComImport]
        [Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E")]
        [TypeLibType(TypeLibTypeFlags.FHidden | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FOleAutomation)]
        public interface IWebBrowser2
        {
            [DispId(200)]
            object Application
            {
                [return: MarshalAs(UnmanagedType.IDispatch)]
                get;
            }

            [DispId(201)]
            object Parent
            {
                [return: MarshalAs(UnmanagedType.IDispatch)]
                get;
            }

            [DispId(202)]
            object Container
            {
                [return: MarshalAs(UnmanagedType.IDispatch)]
                get;
            }

            [DispId(203)]
            object Document
            {
                [return: MarshalAs(UnmanagedType.IDispatch)]
                get;
            }

            [DispId(204)]
            bool TopLevelContainer { get; }

            [DispId(205)]
            string Type { get; }

            [DispId(206)]
            int Left { get; set; }

            [DispId(207)]
            int Top { get; set; }

            [DispId(208)]
            int Width { get; set; }

            [DispId(209)]
            int Height { get; set; }

            [DispId(210)]
            string LocationName { get; }

            [DispId(211)]
            string LocationURL { get; }

            [DispId(212)]
            bool Busy { get; }

            [DispId(0)]
            string Name { get; }

            [DispId(-515)]
            int HWND { get; }

            [DispId(400)]
            string FullName { get; }

            [DispId(401)]
            string Path { get; }

            [DispId(402)]
            bool Visible { get; set; }

            [DispId(403)]
            bool StatusBar { get; set; }

            [DispId(404)]
            string StatusText { get; set; }

            [DispId(405)]
            int ToolBar { get; set; }

            [DispId(406)]
            bool MenuBar { get; set; }

            [DispId(407)]
            bool FullScreen { get; set; }

            [DispId(-525)]
            WebBrowserReadyState ReadyState { get; }

            [DispId(550)]
            bool Offline { get; set; }

            [DispId(551)]
            bool Silent { get; set; }

            [DispId(552)]
            bool RegisterAsBrowser { get; set; }

            [DispId(553)]
            bool RegisterAsDropTarget { get; set; }

            [DispId(554)]
            bool TheaterMode { get; set; }

            [DispId(555)]
            bool AddressBar { get; set; }

            [DispId(556)]
            bool Resizable { get; set; }

            [DispId(100)]
            void GoBack();

            [DispId(101)]
            void GoForward();

            [DispId(102)]
            void GoHome();

            [DispId(103)]
            void GoSearch();

            [DispId(104)]
            void Navigate([In] string Url, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);

            [DispId(-550)]
            void Refresh();

            [DispId(105)]
            void Refresh2([In] ref object level);

            [DispId(106)]
            void Stop();

            [DispId(300)]
            void Quit();

            [DispId(301)]
            void ClientToWindow(out int pcx, out int pcy);

            [DispId(302)]
            void PutProperty([In] string property, [In] object vtValue);

            [DispId(303)]
            object GetProperty([In] string property);

            [DispId(500)]
            void Navigate2([In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);

            [DispId(501)]
            OLECMDF QueryStatusWB([In] OLECMDID cmdID);

            [DispId(502)]
            void ExecWB([In] OLECMDID cmdID, [In] OLECMDEXECOPT cmdexecopt, ref object pvaIn, IntPtr pvaOut);

            [DispId(503)]
            void ShowBrowserBar([In] ref object pvaClsid, [In] ref object pvarShow, [In] ref object pvarSize);
        }

        private IAccessible iacc;

        private int childID;

        private bool disposed;

        private static Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");

        private static Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E");

        private static Guid IID_IAccessible = new Guid("618736E0-3C3D-11CF-810C-00AA00389B71");

        public IAccessible IAccessible => iacc;

        public int ChildID => childID;

        public static SystemAccessibleObject MouseCursor => FromWindow(IntPtr.Zero, AccessibleObjectID.OBJID_CURSOR);

        public static SystemAccessibleObject Caret
        {
            get
            {
                try
                {
                    return FromWindow(IntPtr.Zero, AccessibleObjectID.OBJID_CARET);
                }
                catch (COMException)
                {
                    return null;
                }
            }
        }

        public string Description => iacc.get_accDescription((object)childID);

        public string Name => iacc.get_accName((object)childID);

        public object Role => iacc.get_accRole((object)childID);

        public int RoleIndex
        {
            get
            {
                object role = Role;
                if (role is int)
                {
                    return (int)role;
                }
                return -1;
            }
        }

        public string RoleString
        {
            get
            {
                object role = Role;
                if (role is int)
                {
                    return RoleToString((int)role);
                }
                if (role is string)
                {
                    return (string)role;
                }
                return role.ToString();
            }
        }

        public Rectangle Location
        {
            get
            {
                int pxLeft, pyTop, pcxWidth, pcyHeight;

                iacc.accLocation(out pxLeft, out pyTop, out pcxWidth, out pcyHeight, childID);
                return new Rectangle(pxLeft, pyTop, pcxWidth, pcyHeight);
            }
        }

        public string Value => iacc.get_accValue((object)childID);

        public int State => (int)iacc.get_accState((object)childID);

        public string StateString => StateToString(State);

        public bool Visible => (State & 0x8000) == 0;

        public SystemAccessibleObject Parent
        {
            get
            {
                if (childID != 0)
                {
                    return new SystemAccessibleObject(iacc, 0);
                }
                IAccessible accessible = (IAccessible)iacc.accParent;
                if (accessible == null)
                {
                    return null;
                }
                return new SystemAccessibleObject(accessible, 0);
            }
        }

        public string KeyboardShortcut
        {
            get
            {
                try
                {
                    return iacc.get_accKeyboardShortcut((object)childID);
                }
                catch (ArgumentException)
                {
                    return "";
                }
                catch (NotImplementedException)
                {
                    return "";
                }
                catch (COMException)
                {
                    return null;
                }
            }
        }

        public string DefaultAction
        {
            get
            {
                try
                {
                    return iacc.get_accDefaultAction((object)childID);
                }
                catch (COMException)
                {
                    return null;
                }
            }
        }

        public SystemAccessibleObject[] SelectedObjects
        {
            get
            {
                if (childID != 0)
                {
                    return new SystemAccessibleObject[0];
                }
                object accSelection;
                try
                {
                    accSelection = iacc.accSelection;
                }
                catch (NotImplementedException)
                {
                    return new SystemAccessibleObject[0];
                }
                catch (COMException)
                {
                    return new SystemAccessibleObject[0];
                }
                if (accSelection == null)
                {
                    return new SystemAccessibleObject[0];
                }
                if (accSelection is IEnumVARIANT)
                {
                    IEnumVARIANT enumVARIANT = (IEnumVARIANT)accSelection;
                    enumVARIANT.Reset();
                    List<SystemAccessibleObject> list = new List<SystemAccessibleObject>();
                    object[] array = new object[1];
                    while (enumVARIANT.Next(1, array, IntPtr.Zero) == 0 && (!(array[0] is int) || (int)array[0] >= 0))
                    {
                        list.Add(ObjectToSAO(array[0]));
                    }
                    return list.ToArray();
                }
                if (!(accSelection is int) || (int)accSelection >= 0)
                {
                    return new SystemAccessibleObject[1] { ObjectToSAO(accSelection) };
                }
                return new SystemAccessibleObject[0];
            }
        }

        public IntPtr Window
        {
            get
            {
                IntPtr phwnd;
                WindowFromAccessibleObject(iacc, out phwnd);
                return phwnd;
            }
        }

        public SystemAccessibleObject[] Children
        {
            get
            {
                if (childID != 0)
                {
                    return new SystemAccessibleObject[0];
                }
                int accChildCount = iacc.accChildCount;
                object[] array = new object[accChildCount * 2];
                int pcObtained;
                uint num = AccessibleChildren(iacc, 0, accChildCount * 2, array, out pcObtained);
                if (num != 0 && num != 1)
                {
                    return new SystemAccessibleObject[0];
                }
                if (pcObtained == 1 && array[0] is int && (int)array[0] < 0)
                {
                    return new SystemAccessibleObject[0];
                }
                SystemAccessibleObject[] array2 = new SystemAccessibleObject[pcObtained];
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i] = ObjectToSAO(array[i]);
                }
                return array2;
            }
        }

        public SystemAccessibleObject(IAccessible iacc, int childID)
        {
            if (iacc == null)
            {
                throw new ArgumentNullException();
            }
            if (childID != 0)
            {
                try
                {
                    object obj = null;
                    try
                    {
                        obj = iacc.get_accChild((object)childID);
                    }
                    catch (Exception)
                    {
                    }
                    if (obj != null)
                    {
                        iacc = (IAccessible)obj;
                        childID = 0;
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            this.iacc = iacc;
            this.childID = childID;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                iacc = null;
            }
            GC.SuppressFinalize(this);
        }

        public static SystemAccessibleObject FromPoint(int x, int y)
        {
            POINT pt = default(POINT);
            pt.x = x;
            pt.y = y;
            IAccessible accObj;
            object ChildID;
            IntPtr intPtr = AccessibleObjectFromPoint(pt, out accObj, out ChildID);
            if (intPtr != IntPtr.Zero)
            {
                throw new Exception("AccessibleObjectFromPoint returned " + intPtr.ToInt32());
            }
            return new SystemAccessibleObject(accObj, (int)(ChildID ?? ((object)0)));
        }

        public static SystemAccessibleObject FromWindow(IntPtr window, AccessibleObjectID objectID)
        {
            return new SystemAccessibleObject((IAccessible)AccessibleObjectFromWindow(window, (uint)objectID, new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}")), 0);
        }

        public static SystemAccessibleObject FromHtmlElement(object pHtmlElement)
        {
            return new SystemAccessibleObject(IHTMLElementToMSAA(pHtmlElement), 0);
        }

        public static string RoleToString(int roleNumber)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            if (GetRoleText((uint)roleNumber, stringBuilder, 1024u) == 0)
            {
                throw new Exception("Invalid role number");
            }
            return stringBuilder.ToString();
        }

        public static string StateToString(int stateNumber)
        {
            if (stateNumber == 0)
            {
                return "None";
            }
            int num = stateNumber & -stateNumber;
            int num2 = stateNumber - num;
            string text = StateBitToString(num);
            if (num2 == 0)
            {
                return text;
            }
            return StateToString(num2) + ", " + text;
        }

        public static string StateBitToString(int stateBit)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            if (GetStateText((uint)stateBit, stringBuilder, 1024u) == 0)
            {
                throw new Exception("Invalid role number");
            }
            return stringBuilder.ToString();
        }

        public void DoDefaultAction()
        {
            iacc.accDoDefaultAction(ChildID);
        }

        private SystemAccessibleObject ObjectToSAO(object obj)
        {
            if (obj is int)
            {
                return new SystemAccessibleObject(iacc, (int)obj);
            }
            return new SystemAccessibleObject((IAccessible)obj, 0);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SystemAccessibleObject sao = obj as SystemAccessibleObject;
            return Equals(sao);
        }

        public bool Equals1(SystemAccessibleObject sao)
        {
            if ((object)sao == null)
            {
                return false;
            }
            if (childID == sao.childID)
            {
                return Location == sao.Location;
            }
            return false;
        }

        public bool Equals(SystemAccessibleObject sao)
        {
            if ((object)sao == null)
            {
                return false;
            }
            if (childID == sao.childID)
            {
                return DeepEquals(iacc, sao.iacc);
            }
            return false;
        }

        private static bool DeepEquals(IAccessible ia1, IAccessible ia2)
        {
            if (ia1.Equals(ia2))
            {
                return true;
            }
            if (Marshal.GetIUnknownForObject(ia1) == Marshal.GetIUnknownForObject(ia2))
            {
                return true;
            }
            if (ia1.accChildCount != ia2.accChildCount)
            {
                return false;
            }
            SystemAccessibleObject systemAccessibleObject = new SystemAccessibleObject(ia1, 0);
            SystemAccessibleObject systemAccessibleObject2 = new SystemAccessibleObject(ia2, 0);
            if (systemAccessibleObject.Window != systemAccessibleObject2.Window)
            {
                return false;
            }
            if (systemAccessibleObject.Location != systemAccessibleObject2.Location)
            {
                return false;
            }
            if (systemAccessibleObject.DefaultAction != systemAccessibleObject2.DefaultAction)
            {
                return false;
            }
            if (systemAccessibleObject.Description != systemAccessibleObject2.Description)
            {
                return false;
            }
            if (systemAccessibleObject.KeyboardShortcut != systemAccessibleObject2.KeyboardShortcut)
            {
                return false;
            }
            if (systemAccessibleObject.Name != systemAccessibleObject2.Name)
            {
                return false;
            }
            if (!systemAccessibleObject.Role.Equals(systemAccessibleObject2.Role))
            {
                return false;
            }
            if (systemAccessibleObject.State != systemAccessibleObject2.State)
            {
                return false;
            }
            if (systemAccessibleObject.Value != systemAccessibleObject2.Value)
            {
                return false;
            }
            if (systemAccessibleObject.Visible != systemAccessibleObject2.Visible)
            {
                return false;
            }
            if (ia1.accParent == null && ia2.accParent == null)
            {
                return true;
            }
            if (ia1.accParent == null || ia2.accParent == null)
            {
                return false;
            }
            return DeepEquals((IAccessible)ia1.accParent, (IAccessible)ia2.accParent);
        }

        public override int GetHashCode()
        {
            return childID ^ iacc.GetHashCode();
        }

        public static bool operator ==(SystemAccessibleObject a, SystemAccessibleObject b)
        {
            if ((object)a == b)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (a.iacc == b.iacc)
            {
                return a.childID == b.childID;
            }
            return false;
        }

        public static bool operator !=(SystemAccessibleObject a, SystemAccessibleObject b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            try
            {
                return Name + " [" + RoleString + "]";
            }
            catch
            {
                return "??";
            }
        }

        [DllImport("oleacc.dll")]
        private static extern IntPtr AccessibleObjectFromPoint(POINT pt, [MarshalAs(UnmanagedType.Interface)] out IAccessible accObj, out object ChildID);

        [DllImport("oleacc.dll")]
        private static extern uint GetRoleText(uint dwRole, [Out] StringBuilder lpszRole, uint cchRoleMax);

        [DllImport("oleacc.dll", ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        private static extern object AccessibleObjectFromWindow(IntPtr hwnd, uint dwObjectID, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [DllImport("oleacc.dll")]
        private static extern uint GetStateText(uint dwStateBit, [Out] StringBuilder lpszStateBit, uint cchStateBitMax);

        [DllImport("oleacc.dll")]
        private static extern uint WindowFromAccessibleObject(IAccessible pacc, out IntPtr phwnd);

        [DllImport("oleacc.dll")]
        private static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        public static IHTMLDocument2 GetDocumentFromWindow(IHTMLWindow2 htmlWindow)
        {
            if (htmlWindow == null)
            {
                return null;
            }
            try
            {
                return htmlWindow.document;
            }
            catch (COMException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception)
            {
                return null;
            }
            try
            {
                IServiceProvider obj = (IServiceProvider)htmlWindow;
                object ppvObject = null;
                obj.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out ppvObject);
                return (IHTMLDocument2)((IWebBrowser2)ppvObject).Document;
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
            return null;
        }

        public static IHTMLElement MSAAToIHTMLElement(IAccessible pacc)
        {
            if (pacc == null)
            {
                return null;
            }
            IServiceProvider serviceProvider = pacc as IServiceProvider;
            if (serviceProvider == null)
            {
                return null;
            }
            Guid guidService = typeof(IHTMLElement).GUID;
            object ppvObject;
            if (serviceProvider.QueryService(ref guidService, ref guidService, out ppvObject) != 0)
            {
                return null;
            }
            return ppvObject as IHTMLElement;
        }

        public static IAccessible IHTMLElementToMSAA(object pHtmlElement)
        {
            IServiceProvider serviceProvider = pHtmlElement as IServiceProvider;
            if (serviceProvider == null)
            {
                return null;
            }
            Guid guidService = IID_IAccessible;
            object ppvObject;
            if (serviceProvider.QueryService(ref guidService, ref guidService, out ppvObject) != 0)
            {
                return null;
            }
            return ppvObject as IAccessible;
        }
    }
}