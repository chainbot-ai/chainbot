using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Plugins.Shared.Library.UiAutomation
{
    public class UiCommon
    {
        public const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TRANSPARENT = 0x00000020;

        public static class ShowWindowTypes
        {
            public const int SW_HIDE = 0;
            public const int SW_SHOWNORMAL = 1;
            public const int SW_NORMAL = 1;
            public const int SW_SHOWMINIMIZED = 2;
            public const int SW_SHOWMAXIMIZED = 3;
            public const int SW_MAXIMIZE = 3;
            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_SHOW = 5;
            public const int SW_MINIMIZE = 6;
            public const int SW_SHOWMINNOACTIVE = 7;
            public const int SW_SHOWNA = 8;
            public const int SW_RESTORE = 9;
            public const int SW_SHOWDEFAULT = 10;
            public const int SW_FORCEMINIMIZE = 11;
            public const int SW_MAX = 11;
        }


        public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        public static string Decode(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            return Regex.Unescape(s.Replace("\\", "\\\\").Replace("%", "\\u"));
        }


        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetDlgCtrlID(IntPtr hWnd);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr WindowFromPoint(System.Drawing.Point Point);

        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("User32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadCursorFromFile(String str);


        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint nCmd);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int procId);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public delegate bool WNDENUMPROC(int hWnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(int hWndParent, WNDENUMPROC lpfn, int lParam);

        public static Cursor GetCursor(byte[] cursorResource)
        {
            try
            {
                var tmpPath = System.IO.Path.GetTempPath();
                var guid = Guid.NewGuid().ToString("N");

                if (tmpPath.Substring(tmpPath.Length - 1, 1) != @"\")
                {
                    tmpPath = tmpPath + @"\";
                }

                var tempFile = tmpPath + guid + @".cur";

                File.WriteAllBytes(tempFile, cursorResource);

                var cursor = new Cursor(LoadCursorFromFile(tempFile));
                File.Delete(tempFile);
                return cursor;
            }
            catch (Exception)
            {

            }

            return Cursors.Default;

        }

        private static bool IsNeedRestoreWindow(IntPtr hWnd)
        {
            var state = GetPlacement(hWnd).showCmd;
            if (state == ShowWindowCommands.Hide || state == ShowWindowCommands.Minimized)
            {
                return true;
            }

            return false;
        }

        public static void ForceShow(IntPtr hWnd)
        {
            if (IsNeedRestoreWindow(hWnd))
            {
                ShowWindow(hWnd, ShowWindowTypes.SW_RESTORE);
                if(JavaUiNode.accessBridge.Functions.IsJavaWindow(hWnd))
                {
                    InvalidateRect(hWnd, IntPtr.Zero, true);
                }
                
            }
        }

        public static string GetProcessNameWithoutSuffix(int pid)
        {
            Process ps = Process.GetProcessById(pid);
            return ps.ProcessName;
        }

        public static string GetProcessName(int pid)
        {
            return System.IO.Path.GetFileName(GetProcessFullPath(pid));
        }

        public static string GetProcessFullPath(int pid)
        {
            string result = "";

            try
            {
                var processHandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);

                if (processHandle == IntPtr.Zero)
                {
                    throw new Exception("打开指定PID的进程返回NULL");
                }

                const int lengthSb = 4000;

                var sb = new StringBuilder(lengthSb);

                if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0)
                {
                    result = sb.ToString();
                }

                CloseHandle(processHandle);

                if(string.IsNullOrEmpty(result))
                {
                    throw new Exception("获取进程路径为空");
                }
            }
            catch (Exception err)
            {
                try
                {
                    var latch = new CountdownEvent(1);
                    Thread td = new Thread(() =>
                    {
                        try
                        {
                            string Query = "SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = " + pid;

                            using (ManagementObjectSearcher mos = new ManagementObjectSearcher(Query))
                            {
                                using (ManagementObjectCollection moc = mos.Get())
                                {
                                    string ExecutablePath = (from mo in moc.Cast<ManagementObject>() select mo["ExecutablePath"]).First().ToString();

                                    result = ExecutablePath;
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }

                        latch.Signal();
                    });
                    td.TrySetApartmentState(ApartmentState.STA);
                    td.IsBackground = true;
                    td.Start();
                    latch.Wait();
                }
                catch
                {
                }
            }

            return result;
        }


        public static void EnablePassThrough(Form form)
        {
            int style = GetWindowLong(
                form.Handle, GWL_EXSTYLE);
            SetWindowLong(
             form.Handle, GWL_EXSTYLE,
                style | WS_EX_TRANSPARENT);
        }

        public static void DisablePassThrough(Form form)
        {
            int style = GetWindowLong(
                form.Handle, GWL_EXSTYLE);
            SetWindowLong(
            form.Handle, GWL_EXSTYLE,
               style & ~WS_EX_TRANSPARENT);
        }

        public static IntPtr GetRootWindow(Point point)
        {
            IntPtr hWnd = WindowFromPoint(point);

            IntPtr hParentWnd = GetParent(hWnd);

            while (hParentWnd != IntPtr.Zero)
            {
                hWnd = hParentWnd;
                hParentWnd = GetParent(hParentWnd);
            }

            return hWnd;
        }


        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        public enum BinaryType : uint
        {
            SCS_32BIT_BINARY = 0, // A 32-bit Windows-based application
            SCS_64BIT_BINARY = 6, // A 64-bit Windows-based application.
            SCS_DOS_BINARY = 1,   // An MS-DOS – based application
            SCS_OS216_BINARY = 5, // A 16-bit OS/2-based application
            SCS_PIF_BINARY = 3,   // A PIF file that executes an MS-DOS – based application
            SCS_POSIX_BINARY = 4, // A POSIX – based application
            SCS_WOW_BINARY = 2    // A 16-bit Windows-based application 
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetBinaryType(
            string lpApplicationName,
            out BinaryType dwBinType
            );


        public static bool IsExe64Bit(string exePath)
        {
            BinaryType exeType = BinaryType.SCS_32BIT_BINARY;
            if (GetBinaryType(exePath, out exeType))
            {
                if(exeType == BinaryType.SCS_64BIT_BINARY)
                {
                    return true;
                }
            }

            return false;
        }

        public static Process RunProcess(string fileName, string arguments,bool isHidden = false)
        {
            ProcessStartInfo psi = new ProcessStartInfo();

            if (isHidden)
            {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.FileName = fileName;
            psi.Arguments = arguments;

            var p = Process.Start(psi);

            return p;
        }

        public static void CopyFileToDir(string srcPath,string dstDir,bool isOverwrite = true)
        {
            string dstPath = "";
            try
            {
                if(dstDir.Substring(dstDir.Length - 1, 1)== @"\")
                {
                    dstPath = dstDir + System.IO.Path.GetFileName(srcPath);
                }
                else
                {
                    dstPath = dstDir + @"\" + System.IO.Path.GetFileName(srcPath);
                }

                if(isOverwrite)
                {
                    System.IO.File.Copy(srcPath, dstPath, isOverwrite);
                }
                else
                {
                    if(!File.Exists(dstPath))
                    {
                        System.IO.File.Copy(srcPath, dstPath, isOverwrite);
                    }
                }
                
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Trace, $"复制文件{srcPath}到{dstPath}出错", err);
            }
        }

        public static List<IntPtr> FindChildren(IntPtr hWnd, string className, string Name)
        {
            List<IntPtr> list = new List<IntPtr>();
            string pattern = "^" + Regex.Escape(className).Replace("\\*", ".*").Replace("\\?", ".");
            string text = (!string.IsNullOrEmpty(Name)) ? ("^" + Regex.Escape(Name).Replace("\\*", ".*").Replace("\\?", ".")) : null;
            IntPtr window = GetWindow(hWnd, 6u);
            if (window != IntPtr.Zero)
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                GetClassName(window, stringBuilder, 256);
                if (Regex.IsMatch(stringBuilder.ToString(), pattern))
                {
                    StringBuilder stringBuilder2 = new StringBuilder(256);
                    GetWindowText(window, stringBuilder2, 256);
                    if (string.IsNullOrEmpty(text) || Regex.IsMatch(stringBuilder2.ToString(), text) || Name == "*")
                    {
                        list.Add(window);
                    }
                }
            }
            if ((className + Name).IndexOfAny(new char[]
            {
                '*',
                '?'
            }) == -1)
            {
                string lpWindowName = (Name != null && Name.Length > 0) ? Name : null;
                IntPtr hwndChildAfter = IntPtr.Zero;
                for (;;)
                {
                    IntPtr intPtr = FindWindowEx(hWnd, hwndChildAfter, className, lpWindowName);
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
                    GetClassName(window2, stringBuilder3, 256);
                    if (Regex.IsMatch(stringBuilder3.ToString(), pattern))
                    {
                        StringBuilder stringBuilder4 = new StringBuilder(256);
                        GetWindowText(window2, stringBuilder4, 256);
                        if (string.IsNullOrEmpty(text) || Regex.IsMatch(stringBuilder4.ToString(), text) || Name == "*")
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
            while (queue.Any<IntPtr>())
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

        public static string GetProcessNameEx(int processId)
        {
            try
            {
                foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID = " + processId).Get())
                {
                    ManagementObject managementObject = (ManagementObject)managementBaseObject;
                    if (managementObject["Name"] != null)
                    {
                        try
                        {
                            string text = managementObject["Name"].ToString();
                            if (!string.IsNullOrEmpty(text))
                            {
                                return text.Substring(0, text.LastIndexOf('.'));
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return Process.GetProcessById(processId).ProcessName;
        }

        public static bool MatchDescendants(IntPtr hWnd, string className, string Name)
        {
            List<IntPtr> list = new List<IntPtr>();
            Queue<IntPtr> queue = new Queue<IntPtr>();
            list.AddRange(FindChildren(hWnd, className, Name));
            queue.Enqueue(hWnd);
            while (queue.Count != 0 && list.Count <= 0)
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
            return list.Count > 0;
        }
    }
}
