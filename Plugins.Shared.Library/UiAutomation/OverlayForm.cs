using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using Gma.UserActivityMonitor;
using System.Linq;
using WindowsAccessBridgeInterop;
using FlaUI.Core.AutomationElements;
using Chainbot.ChromePlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chainbot.FirefoxPlugin;
using System.Threading;
using System.Runtime.InteropServices;
using Chainbot.IEPlugin;
using System.Text;
using System.Diagnostics;
using Plugins.Shared.Library.Librarys;
using Chainbot.Sap;
using Plugins.Shared.Library.Window;

namespace Plugins.Shared.Library.UiAutomation
{
    public partial class OverlayForm : Form
    {
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        const int WM_NCHITTEST = 0x0084;
        const int HTTRANSPARENT = -1;

        private const uint WS_EX_LAYERED = 0x80000;
        private const uint WS_EX_TRANSPARENT = 0x20;
        private const int GWL_STYLE = (-16);
        private const int GWL_EXSTYLE = (-20);
        private const int LWA_ALPHA = 0;

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(
        IntPtr hwnd,
        int nIndex,
        uint dwNewLong
        );

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(
        IntPtr hwnd,
        int nIndex
        );

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        public bool IsWindowHighlight { get; internal set; }

        private bool enablePassThrough { get; set; }

        private static DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private static DispatcherTimer dealySelectDispatcherTimer = new DispatcherTimer();
        private static DispatcherTimer keyStateDispatcherTimer = new DispatcherTimer();

        private static DispatcherTimer cancelDealySelectDispatcherTimer = new DispatcherTimer();

        private static DispatcherTimer HintRefreshTimer = new DispatcherTimer();

        private ExtendedPanel panelBorderLeft, panelBorderTop, panelBorderRight, panelBorderBottom;

        private ExtendedPanel panelInside;

        internal UiNode CurrentHighlightElement;

        internal Point CurrentHighlightElementScreenPoint;


        private bool isLeftMouseButtonDown = false;
        private bool isLeftMouseButtonUp = false;
        private bool isDragging = false;

        private Point draggingBeginPoint { get; set; }
        private Point draggingCurrentPoint { get; set; }

        private Rectangle currentHighlightRect { get; set; }

        public enum SelectorMode
        {
            Default,
            UIA2,
        }

        private SelectorMode selectorMode = SelectorMode.Default;

        private ConutDownWindow conutDownWindow;
        private HintWindow hintWindow;

        public OverlayForm()
        {
            InitializeComponent();

            this.panelBorderLeft = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderTop = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderRight = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderBottom = createPanel(Color.FromArgb(232, 193, 116));

            this.panelInside = createPanel(Color.FromArgb(123, 159, 212));

            this.Cursor = UiCommon.GetCursor(Properties.Resources.cursor);
        }

        public void HideInformative()
        {
            this.panelBorderLeft.Hide();
            this.panelBorderTop.Hide();
            this.panelBorderRight.Hide();
            this.panelBorderBottom.Hide();
            this.panelInside.Hide();
        }

        public void ShowInformative()
        {
            this.panelBorderLeft.Show();
            this.panelBorderTop.Show();
            this.panelBorderRight.Show();
            this.panelBorderBottom.Show();
            this.panelInside.Show();
        }

        private void keyStateDispatcherTimer_Tick(object sender, EventArgs e)
        {
            const int MBUTTON = 0x04;
            if ((UiCommon.GetAsyncKeyState(MBUTTON) & 0x01) != 0)
            {
                doDelaySelect();
            }
        }

        private void HintRefreshTimer_Tick(object sender, EventArgs e)
        {
            hintWindow.ShowInfo();
        }

        public void SetPenetrate(bool flag = true)
        {
            try
            {
                uint style = GetWindowLong(this.Handle, GWL_EXSTYLE);

                if (flag)
                    SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
                else
                    SetWindowLong(this.Handle, GWL_EXSTYLE, style & ~(WS_EX_TRANSPARENT));
            }
            catch (Exception)
            {

            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        public Point CurrentHighlightElementRelativeClickPos { get; private set; }
        public bool hasDoSelect { get; private set; }
        
        private ExtendedPanel createPanel(Color color)
        {
            var panel = new ExtendedPanel();
            panel.Size = new Size(0, 0);
            panel.Parent = this;
            panel.BackColor = color;
            panel.Show();
            return panel;
        }

        protected override void WndProc(ref Message m)
        {
            if (enablePassThrough)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void doCancel()
        {
            changeToDefaultSelectorMode();
            System.Threading.Tasks.Task.Run(() => {
                this.Invoke(new Action(() => {
                    UiElement.OnCanceled?.Invoke();
                }));
            });

            StopHighlight();
        }

        private void doDelaySelect()
        {
            StopHighlight(false);

            cancelDealySelectDispatcherTimer.Tick -= new EventHandler(cancelDealySelectDispatcherTimer_Tick);
            cancelDealySelectDispatcherTimer.Tick += new EventHandler(cancelDealySelectDispatcherTimer_Tick);

            cancelDealySelectDispatcherTimer.Interval = TimeSpan.FromMilliseconds(30);
            cancelDealySelectDispatcherTimer.Stop();
            cancelDealySelectDispatcherTimer.Start();

            dealySelectDispatcherTimer.Tick -= new EventHandler(dealySelectDispatcherTimer_Tick);
            dealySelectDispatcherTimer.Tick += new EventHandler(dealySelectDispatcherTimer_Tick);

            dealySelectDispatcherTimer.Interval = TimeSpan.FromSeconds(5);
            dealySelectDispatcherTimer.Stop();
            dealySelectDispatcherTimer.Start();

            conutDownWindow = new ConutDownWindow(5);
            conutDownWindow.Show();
        }

        private void doSelect()
        {
            lock(this)
            {
                try
                {
                    if (UiElement.IsEnableDragToSelectImage && isDragging)
                    {
                        try
                        {
                            this.HideInformative();
                            var captureBitmap = UiElement.CaptureScreenshot(currentHighlightRect);
                            this.Hide();

                            System.Threading.Tasks.Task.Run(() => {
                                this.Invoke(new Action(() => {
                                    UiElement.OnScreenCaptureSelected?.Invoke(captureBitmap, currentHighlightRect);
                                }));
                            });

                            StopHighlight();
                        }
                        catch (Exception)
                        {
                            StopHighlight();
                            System.Windows.MessageBox.Show("请重新进行拖拽选取！", "警告", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        if (hasDoSelect)
                        {
                            return;
                        }

                        hasDoSelect = true;

                        if (CurrentHighlightElement != null)
                        {
                            if (selectorMode == SelectorMode.Default)
                            {
                                if (enableJavaUiNode(CurrentHighlightElement))
                                {
                                    return;
                                }

                                enableChromeUiNode();

                                enableFireFoxUiNode();
                            }

                            Thread.Sleep(1000);

                            if (selectorMode == SelectorMode.Default)
                            {
                                enableIEUiNode();

                                enableSAPUiNode();
                            }

                            var element = new UiElement(CurrentHighlightElement);
                            element.RelativeClickPos = CurrentHighlightElementRelativeClickPos;

                            this.HideInformative();
                            hintWindow.Hide();
                            element.currentInformativeScreenshot = element.CaptureInformativeScreenshot();
                            this.Hide();

                            var sel = element.Selector;
                                                       //System.Diagnostics.Debug.WriteLine(string.Format("CurrentHighlightElementRelativeClickPos = ({0},{1})", CurrentHighlightElementRelativeClickPos.X, CurrentHighlightElementRelativeClickPos.Y));
                            System.Threading.Tasks.Task.Run(() => {
                                this.Invoke(new Action(() => {
                                    UiElement.OnSelected?.Invoke(element);
                                }));
                            });

                            StopHighlight();
                        }
                        else
                        {
                            this.Hide();

                            StopHighlight();
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);

                    this.Hide();

                    StopHighlight();
                }
                finally
                {
                    changeToDefaultSelectorMode();
                }
            }
        }

        private void dealySelectDispatcherTimer_Tick(object sender, EventArgs e)
        {
            cancelDealySelectDispatcherTimer.Tick -= new EventHandler(cancelDealySelectDispatcherTimer_Tick);
            cancelDealySelectDispatcherTimer.Stop();

            dealySelectDispatcherTimer.Tick -= new EventHandler(dealySelectDispatcherTimer_Tick);
            dealySelectDispatcherTimer.Stop();

            if (conutDownWindow != null)
            {
                conutDownWindow.Close();
                conutDownWindow = null;
            }

            StartHighlight();
        }

        private void cancelDealySelectDispatcherTimer_Tick(object sender, EventArgs e)
        {
            const int SPACE = 0x20;
            if (UiCommon.GetAsyncKeyState(SPACE) != 0)
            {
                cancelDealySelectDispatcherTimer.Tick -= new EventHandler(cancelDealySelectDispatcherTimer_Tick);
                cancelDealySelectDispatcherTimer.Stop();

                dealySelectDispatcherTimer.Tick -= new EventHandler(dealySelectDispatcherTimer_Tick);
                dealySelectDispatcherTimer.Stop();

                if (conutDownWindow != null)
                {
                    conutDownWindow.Close();
                    conutDownWindow = null;
                }

                StartHighlight();
            }      
        }

        public void MoveRect(Rectangle rect)
        {
            int margin = 5;

            panelBorderLeft.Location = new Point(rect.Left, rect.Top);
            panelBorderLeft.Size = new Size(margin, rect.Height);

            panelBorderTop.Location = new Point(rect.Left, rect.Top);
            panelBorderTop.Size = new Size(rect.Width, margin);

            panelBorderRight.Location = new Point((rect.Left + rect.Width - margin), rect.Top);
            panelBorderRight.Size = new Size(margin, rect.Height);

            panelBorderBottom.Location = new Point(rect.Left, (rect.Top + rect.Height - margin));
            panelBorderBottom.Size = new Size(rect.Width, margin);

            panelInside.Location = new Point(rect.Left + 5, rect.Top + 5);
            panelInside.Size = new Size(rect.Width - 10, rect.Height - 10);

        }

        internal void installHook()
        {
            uninstallHook();

            HookManager.MouseClickExt += HookManager_MouseClickExt;
            HookManager.MouseDown += HookManager_MouseDown;
            HookManager.MouseMove += HookManager_MouseMove;
            HookManager.MouseUp += HookManager_MouseUp;

            HookManager.KeyDown += HookManager_KeyDown;
        }

        

        internal void uninstallHook()
        {
            HookManager.MouseClickExt -= HookManager_MouseClickExt;
            HookManager.MouseDown -= HookManager_MouseDown;
            HookManager.MouseMove -= HookManager_MouseMove;
            HookManager.MouseUp -= HookManager_MouseUp;

            HookManager.KeyDown -= HookManager_KeyDown;
        }


        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    doCancel();
                    resetFlags();
                    break;
                case Keys.F2:
                    doDelaySelect();
                    break;
                case Keys.F4:
                    changeToUIASelectorMode();
                    e.SuppressKeyPress = true;
                    break;
                default:
                    break;
            }
        }

        private void changeToUIASelectorMode()
        {
            selectorMode = SelectorMode.UIA2;
            Trace.WriteLine("selectorMode = SelectorMode.UIA2");
        }

        private void changeToDefaultSelectorMode()
        {
            selectorMode = SelectorMode.Default;
            Trace.WriteLine("selectorMode = SelectorMode.Default");
        }

        private void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                isLeftMouseButtonDown = true;
                draggingBeginPoint = System.Windows.Forms.Cursor.Position;
            }
            
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (isLeftMouseButtonDown && !isLeftMouseButtonUp)
            {
                if(UiElement.IsEnableDragToSelectImage)
                {
                    isDragging = true;
                    draggingCurrentPoint = System.Windows.Forms.Cursor.Position;
                }
            }
        }

        private void HookManager_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    isLeftMouseButtonUp = true;
                    doSelect();
                    resetFlags();
                    break;
                case MouseButtons.Right:
                    doCancel();
                    resetFlags();
                    break;
                default:
                    break;
            }


            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseButtonDown = false;
                isLeftMouseButtonUp = false;
                isDragging = false;
            }
        }

        private void resetFlags()
        {
            UiElement.IsEnableDragToSelectImage = false;
        }


        private void HookManager_MouseClickExt(object sender, MouseEventExtArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    e.Handled = true;
                    break;
                case MouseButtons.Right:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }


        internal void StopHighlight(bool isNeedShowMainWindow = true)
        {

            uninstallHook();

            dispatcherTimer.Stop();
            dealySelectDispatcherTimer.Stop();
            keyStateDispatcherTimer.Stop();
            HintRefreshTimer.Stop();

            hintWindow.Hide();
            this.Hide();
            panelBorderLeft.Size = panelBorderTop.Size = panelBorderRight.Size = panelBorderBottom.Size = panelInside.Size = new Size(0, 0);

            UiElement.ScreenReaderOff();

            if (!UiElement.IsRecordingWindowOpened)
            {
                if (isNeedShowMainWindow)
                {
                    SharedObject.Instance.ShowMainWindowNormal();
                }
            }
        }

        internal void StartHighlight()
        {
            hasDoSelect = false;

            JavaUiNode.EnumJvms(true); 
            UiElement.ScreenReaderOn();

            installHook();

            this.ShowInformative();
            this.Show();

            if (this.hintWindow == null)
            {
                this.hintWindow = new HintWindow();
            }
            else
            {
                this.hintWindow.Rest();
            }   
            this.hintWindow.Show();

            dispatcherTimer.Tick -= new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            dispatcherTimer.Start();

            keyStateDispatcherTimer.Tick -= new EventHandler(keyStateDispatcherTimer_Tick);
            keyStateDispatcherTimer.Tick += new EventHandler(keyStateDispatcherTimer_Tick);

            keyStateDispatcherTimer.Interval = TimeSpan.FromMilliseconds(30);
            keyStateDispatcherTimer.Start();

            HintRefreshTimer.Tick -= new EventHandler(HintRefreshTimer_Tick);
            HintRefreshTimer.Tick += new EventHandler(HintRefreshTimer_Tick);

            HintRefreshTimer.Interval = TimeSpan.FromMilliseconds(30);
            HintRefreshTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var screenPoint = Cursor.Position;

                bool bReDo = true;
                if (isDragging)
                {
                    CurrentHighlightElement = null;
                    CurrentHighlightElementScreenPoint = screenPoint;
                    
                    var left = Math.Min(draggingBeginPoint.X, draggingCurrentPoint.X);
                    var top = Math.Min(draggingBeginPoint.Y, draggingCurrentPoint.Y);

                    var right = Math.Max(draggingBeginPoint.X, draggingCurrentPoint.X);
                    var bottom = Math.Max(draggingBeginPoint.Y, draggingCurrentPoint.Y);

                    var rect = Rectangle.FromLTRB(left, top, right, bottom);
                    currentHighlightRect = rect;

                    if(currentHighlightRect.Width == 0 || currentHighlightRect.Height == 0)
                    {
                        isDragging = false;
                        bReDo = true;
                    }
                    else
                    {
                        bReDo = false;
   
                    CurrentHighlightElementRelativeClickPos = new Point(0, 0);

                    this.MoveRect(rect);
                }
                }

                if(bReDo)
                {
                    enablePassThrough = true;

                    AutomationElement element = null;

                    if (IsWindowHighlight)
                    {
                        IntPtr hWnd = UiCommon.GetRootWindow(screenPoint);

                        if (hWnd != IntPtr.Zero)
                        {
                            element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                        }
                    }
                    else
                    {
                        if (selectorMode == SelectorMode.Default)
                        {
                            if (inspectIEUiNode(screenPoint))
                            {
                                return;
                            }
                        }
                           

                        try
                        {
                            element = UIAUiNode.UIAAutomation.FromPoint(screenPoint);
                            element = InspectChildren(element, screenPoint, element.BoundingRectangle);
                        }
                        catch (Exception)
                        {
                            if (element == null)
                            {
                                IntPtr hWnd = UiCommon.WindowFromPoint(screenPoint);

                                if (hWnd != IntPtr.Zero)
                                {
                                    element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                                }
                            }
                        }

                        if (selectorMode == SelectorMode.Default)
                        {
                            if (inspectJavaUiNode(element, screenPoint))
                            {
                                return;
                            }

                            if (inspectChromeUiNode(element, screenPoint))
                            {
                                return;
                            }

                            if (inspectFireFoxUiNode(element, screenPoint))
                            {
                                return;
                            }

                            if (inspectSAPUiNode(element, screenPoint))
                            {
                                return;
                            }
                        }
                    }

                    CurrentHighlightElement = new UIAUiNode(element);
                    CurrentHighlightElementScreenPoint = screenPoint;

                    if (element == null)
                    {
                        return;
                    }

                    var rect = CurrentHighlightElement.BoundingRectangle;
                    currentHighlightRect = rect;

                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);

                    this.MoveRect(rect);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                enablePassThrough = false;

                this.TopMost = true;
            }
        }

        public AutomationElement InspectChildren(AutomationElement element, Point point, Rectangle elementRectangle)
        {
            var children = element.FindAllChildren();

            var minRect = elementRectangle;
            var minElement = element;
            foreach (var child in children)
            {
                if (child.BoundingRectangle.Contains(point))
                {
                    if (child.BoundingRectangle.Width <= minRect.Width && child.BoundingRectangle.Height <= minRect.Height)
                    {
                        minRect = child.BoundingRectangle;
                        minElement = child;
                    }
                }
            }

            if (minElement == element)
            {
                return minElement;
            }
            else
            {
                return InspectChildren(minElement, point, minRect);
            }
        }

        private bool enableJavaUiNode(UiNode node)
        {
            if (node.WindowHandle != IntPtr.Zero
                && (node.ClassName == "SunAwtFrame" || node.ProcessName.ToLower() == "javaw.exe" || node.ProcessName.ToLower() == "java.exe")
                && !JavaUiNode.accessBridge.Functions.IsJavaWindow(node.WindowHandle))
            {
                StopHighlight();

                var nodeProcessFullPath = node.ProcessFullPath;
                var ret = System.Windows.MessageBox.Show("是否启用Java Access Bridge？", "询问", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.Yes);
                if (ret == System.Windows.MessageBoxResult.Yes)
                {

                    var jabswitchExe = System.IO.Path.GetDirectoryName(nodeProcessFullPath) + @"\jabswitch.exe";
                    if (System.IO.File.Exists(jabswitchExe))
                    {
                        UiCommon.RunProcess(jabswitchExe, "-enable", true);
                    }
                    else
                    {
                        string javaHome = System.IO.Directory.GetParent(System.IO.Path.GetDirectoryName(nodeProcessFullPath)).FullName;
                        JavaAccessBridge.Install(javaHome);
                    }

                    System.Windows.MessageBox.Show("操作完成，请重新运行Java程序以便操作生效", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }

                return true;
            }

            return false;
        }


        public bool inspectJavaUiNode(AutomationElement element,Point screenPoint)
        {
            if (element != null)
            {
                var node = new UIAUiNode(element);
                if (JavaUiNode.accessBridge.Functions.IsJavaWindow(node.WindowHandle))
                {
                    Path<AccessibleNode> javaNodePath = JavaUiNode.EnumJvms().Select(javaNode => javaNode.GetNodePathAt(screenPoint)).Where(x => x != null).FirstOrDefault(); 
                    var currentJavaNode = javaNodePath == null ? null : javaNodePath.Leaf;
                    if (currentJavaNode == null)
                    {
                        return false;
                    }

                    CurrentHighlightElement = new JavaUiNode(currentJavaNode);

                    if(CurrentHighlightElement.Role == "table")
                    {
                        var javaElement = CurrentHighlightElement as JavaUiNode;

                        UiNode cellUiNode;
                        if (javaElement.TableCellContains(screenPoint, out cellUiNode))
                        {
                            CurrentHighlightElement = cellUiNode;
                        }
                    }

                    CurrentHighlightElementScreenPoint = screenPoint;

                    var rect = CurrentHighlightElement.BoundingRectangle;

                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - (int)rect.Left, screenPoint.Y - (int)rect.Top);

                    this.MoveRect(rect);
                    return true;
                }
            }
            //Console.WriteLine("return false ");
            return false;
        }

        private bool isInChromeDocument(UIAUiNode node)
        {
            if (node.ProcessName.ToLower() != "chrome.exe" )
            {
                return false;
            }

            if (node.ClassName == "Chrome_RenderWidgetHostHWND")
            {
                return true;
            }
            else
            {
                UiNode parentNode = node;
                while((parentNode = parentNode.Parent)!=null)
                {
                    if(parentNode is UIAUiNode)
                    {
                        var pnode = parentNode as UIAUiNode;
                        if(pnode.ClassName == "Chrome_RenderWidgetHostHWND")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }

        public bool inspectChromeUiNode(AutomationElement element, Point screenPoint)
        {
            if (element != null)
            {
                var node = new UIAUiNode(element);
                if(isInChromeDocument(node)) //node.ClassName == "Chrome_RenderWidgetHostHWND" || node.ProcessName.Contains("chrome") || node.ProcessFullPath.Contains("chrome"))
                {
                    try
                    {
                        IntPtr hWnd = node.WindowHandle;
                        if (IntPtr.Equals(node.WindowHandle,IntPtr.Zero))
                        {
                            Point point = new Point(screenPoint.X, screenPoint.Y);
                            hWnd = UiCommon.WindowFromPoint(point);
                        }   
                        string result = Chrome.Instance.ElementRectFromPoint(hWnd, screenPoint.X, screenPoint.Y);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                        Rectangle rect = new Rectangle();
                        rect.X = jo.Value<int>("x");
                        rect.Y = jo.Value<int>("y");
                        rect.Width = jo.Value<int>("width");
                        rect.Height = jo.Value<int>("height");

                        CurrentHighlightElement = new ChromeUiNode(rect, hWnd);
                        CurrentHighlightElementScreenPoint = screenPoint;

                        this.MoveRect(rect);

                        return true;
                    }
                    catch (Exception e)
                    {
                        doCancel();
                        if (e.Message == "Can't connect to message host!")
                        {
                            System.Windows.MessageBox.Show("chrome扩展未安装或禁用，请检查！", "警告", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        }
                    }
                }
            }
            return false;
        }


        public bool inspectFireFoxUiNode(AutomationElement element, Point screenPoint)
        {
            if (element != null)
            {
                var node = new UIAUiNode(element);
                var nodeParent = node.Parent;
                while (nodeParent != null)
                {
                    if (node.ProcessName.ToLower() == "firefox.exe" || nodeParent.ClassName == "MozillaWindowClass")
                    {
                        try
                        {
                            IntPtr hWnd = node.WindowHandle;
                            if (IntPtr.Equals(node.WindowHandle, IntPtr.Zero))
                            {
                                Point point = new Point(screenPoint.X, screenPoint.Y);
                                hWnd = UiCommon.WindowFromPoint(point);
                            }
                            string result = Firefox.Instance.ElementRectFromPoint(hWnd, screenPoint.X, screenPoint.Y);
                            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                            Rectangle rect = new Rectangle();
                            rect.X = jo.Value<int>("x");
                            rect.Y = jo.Value<int>("y");
                            rect.Width = jo.Value<int>("width");
                            rect.Height = jo.Value<int>("height");
                            CurrentHighlightElement = new FireFoxUiNode(rect, hWnd);
                            CurrentHighlightElementScreenPoint = screenPoint;

                            this.MoveRect(rect);
                            return true;
                        }
                        catch (Exception e)
                        {
                            doCancel();
                            if (e.Message == "Can't connect to message host!")
                            {
                                System.Windows.MessageBox.Show("firefox扩展未安装或禁用，请检查！", "警告", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                                return false;
                            }
                        }
                    }
                    nodeParent = nodeParent.Parent;
                }
            }
            return false;
        }

        public bool isInspectingIE(Point screenPoint, ref UIAUiNode refNode)
        {
            IntPtr hWnd = UiCommon.WindowFromPoint(screenPoint);

            if (hWnd != IntPtr.Zero)
            {
                var element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                if (element != null && element.ClassName == "Internet Explorer_Server")
                {
                    refNode = new UIAUiNode(element);
                    return true;
                }
            }

            return false;
        }

        public bool inspectIEUiNode(Point screenPoint)
        {
            try
            {
                UIAUiNode node = null;
                if (isInspectingIE(screenPoint, ref node))
                {
                    IntPtr hWnd = node.WindowHandle;
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

                    //SetPenetrate(true);
                    string result = IEServiceWrapper.Instance.ElementRectFromPoint(hWnd, screenPoint.X, screenPoint.Y);
                    JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                    Rectangle rect = new Rectangle();
                    rect.X = jo.Value<int>("left");
                    rect.Y = jo.Value<int>("top");
                    rect.Width = jo.Value<int>("width");
                    rect.Height = jo.Value<int>("height");
                    CurrentHighlightElement = new IEUiNode(rect, node.WindowHandle);
                    CurrentHighlightElementScreenPoint = screenPoint;

                    this.MoveRect(rect);
                    //SetPenetrate(false);
                    return true;
                }
            }
            catch (Exception err)
            {
                Trace.WriteLine(err);
            }

            return false;
        }

        private bool isInspectingSAP(UIAUiNode node)
        {
            if (node.ClassName == "SAP_FRONTEND_SESSION" || node.ClassName == "SAP_FRONTEND_EMBEDDED")
            {
                return true;
            }
            else
            {
                UiNode parentNode = node;
                while ((parentNode = parentNode.Parent) != null)
                {
                    if (parentNode is UIAUiNode)
                    {
                        var pnode = parentNode as UIAUiNode;
                        if (pnode.ClassName == "SAP_FRONTEND_SESSION" || pnode.ClassName == "SAP_FRONTEND_EMBEDDED")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }

        private bool inspectSAPUiNode(AutomationElement element, Point screenPoint)
        {
            if (element != null)
            {
                var node = new UIAUiNode(element);
                if (isInspectingSAP(node))
                {
                    try
                    {
                        //Console.WriteLine(node.WindowHandle);
                        IntPtr hWnd = node.WindowHandle;
                        if (IntPtr.Equals(node.WindowHandle, IntPtr.Zero))
                        {
                            Point point = new Point(screenPoint.X, screenPoint.Y);
                            hWnd = UiCommon.WindowFromPoint(point);
                        }

                        string result = Sap.Instance.ElementRectFromPoint(hWnd, screenPoint.X, screenPoint.Y);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                        Rectangle rect = new Rectangle();
                        rect.X = jo.Value<int>("x");
                        rect.Y = jo.Value<int>("y");
                        rect.Width = jo.Value<int>("width");
                        rect.Height = jo.Value<int>("height");

                        CurrentHighlightElement = new SAPUiNode(rect, node.WindowHandle);
                        CurrentHighlightElementScreenPoint = screenPoint;

                        this.MoveRect(rect);
                        return true;
                    }
                    catch (Exception err)
                    {
                        Trace.WriteLine(err);
                    }
                }
            }
            return false;
        }

        public void updateIESelector()
        {
            var point = CurrentHighlightElementScreenPoint;
            SetPenetrate(true);
            string sel = IEServiceWrapper.Instance.GetElementSelector(CurrentHighlightElement.WindowHandle, point.X, point.Y);
            SetPenetrate(false);
            CurrentHighlightElement.Sel = sel;
        }

        public void updateChromeSelector()
        {
            var point = CurrentHighlightElementScreenPoint;
            string szID = Chrome.Instance.ElementCacheIdFromPoint(CurrentHighlightElement.WindowHandle, point.X, point.Y, 0, 0);
            Chrome.Instance.GetElementNodeName(CurrentHighlightElement.WindowHandle, szID);
            Chrome.Instance.GetRectangleFromElem(CurrentHighlightElement.WindowHandle, szID);
            string sel = Chrome.Instance.GetElementSelector(CurrentHighlightElement.WindowHandle, szID);
            //sel = sel.Replace("\"", "'");
            CurrentHighlightElement.Sel = sel;
        }

        public void updateFireFoxSelector()
        {
            var point = CurrentHighlightElementScreenPoint;
            string szID = Firefox.Instance.ElementCacheIdFromPoint(CurrentHighlightElement.WindowHandle, point.X, point.Y, 0, 0);
            Firefox.Instance.GetElementNodeName(CurrentHighlightElement.WindowHandle, szID);
            Firefox.Instance.GetRectangleFromElem(CurrentHighlightElement.WindowHandle, szID);
            string sel = Firefox.Instance.GetElementSelector(CurrentHighlightElement.WindowHandle, szID);
            //sel = sel.Replace("&quot;", "'");
            CurrentHighlightElement.Sel = sel;
        }

        public void updateSAPSelector()
        {
            var point = CurrentHighlightElementScreenPoint;
            string szID = Sap.Instance.ElementCacheIdFromPoint(CurrentHighlightElement.WindowHandle, point.X, point.Y, 0, 0);
            Sap.Instance.GetElementNodeName(CurrentHighlightElement.WindowHandle, szID);
            Sap.Instance.GetRectangleFromElem(CurrentHighlightElement.WindowHandle, szID);
            string sel = Sap.Instance.GetElementSelector(CurrentHighlightElement.WindowHandle, szID);
            //sel = sel.Replace("\"", "'");
            CurrentHighlightElement.Sel = sel;
        }

        private bool enableIEUiNode()
        {
            if (CurrentHighlightElement.ControlType == "IENode")
            {
                try
                {
                    updateIESelector();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }


        
        private bool enableChromeUiNode()
        {
            if (CurrentHighlightElement.ControlType == "ChromeNode")
            {
                try
                {
                    updateChromeSelector();
                    return true;
                }
                catch (Exception e)
                {
                    doCancel();
                    if (e.Message == "Can't connect to message host!")
                    {
                        System.Windows.MessageBox.Show("chrome扩展未安装或禁用，请检查！", "警告", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            return false;
        }

        private bool enableFireFoxUiNode()
        {
            if (CurrentHighlightElement.ControlType == "FirefoxNode")
            {
                try
                {
                    updateFireFoxSelector();
                    return true;
                }
                catch (Exception e)
                {
                    doCancel();
                    if (e.Message == "Can't connect to message host!")
                    {
                        System.Windows.MessageBox.Show("firefox扩展未安装或禁用，请检查！", "警告", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            return false;
        }

        private bool enableSAPUiNode()
        {
            if (CurrentHighlightElement.ControlType == "SAPNode")
            {
                try
                {
                    updateSAPSelector();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }
    }

    public class ExtendedPanel : Panel
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTTRANSPARENT = -1;

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == (int)WM_NCHITTEST)
                message.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref message);
        }
    }
}
