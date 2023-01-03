using Plugins.Shared.Library.UiAutomation;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Plugins.Shared.Library.Librarys
{
    public class JavaAccessBridge
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);


        public static void Install(string javaHomePath)
        {
            var windowsHome = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var jabPath = SharedObject.Instance.ApplicationCurrentDirectory + @"\JAB";
            if (Environment.Is64BitOperatingSystem)
            {
                IntPtr intPtr = (IntPtr)0;
                Wow64DisableWow64FsRedirection(ref intPtr);

                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge-64.dll", windowsHome + @"\SYSTEM32");
                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge-32.dll", windowsHome + @"\SYSWOW64");

                string javaExe = javaHomePath +  @"\bin\java.exe";
                if (UiCommon.IsExe64Bit(javaExe))
                {
                    UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge-64.dll", javaHomePath + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge-64.dll", javaHomePath + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\access-bridge-64.jar", javaHomePath + @"\lib\ext");
                }
                else
                {
                    UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge-32.dll", javaHomePath + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge-32.dll", javaHomePath + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\access-bridge-32.jar", javaHomePath + @"\lib\ext");
                }

                UiCommon.CopyFileToDir(jabPath + @"\accessibility.properties", javaHomePath + @"\lib");
                UiCommon.CopyFileToDir(jabPath + @"\jaccess.jar", javaHomePath + @"\lib\ext");
            }
            else
            {
                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge.dll", windowsHome + @"\SYSTEM32");

                UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge.dll", javaHomePath + @"\bin");
                UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge.dll", javaHomePath + @"\bin");
                UiCommon.CopyFileToDir(jabPath + @"\access-bridge.jar", javaHomePath + @"\lib\ext");

                UiCommon.CopyFileToDir(jabPath + @"\accessibility.properties", javaHomePath + @"\lib");
                UiCommon.CopyFileToDir(jabPath + @"\jaccess.jar", javaHomePath + @"\lib\ext");
            }
        }
    }
}
