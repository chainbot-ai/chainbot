using Microsoft.VisualBasic.Activities;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Plugins.Shared.Library.Librarys
{
    public class Common
    {
        static int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
        }

        public static void RunInUI(Action func)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                func();
            });
        }


        public static void RunInUIWithDispatcherPrioritySend(Action func)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                func();
            },System.Windows.Threading.DispatcherPriority.Send);
        }



        public static String getFileBase64(String fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open);
            byte[] arr = new byte[filestream.Length];
            filestream.Read(arr, 0, (int)filestream.Length);
            string baser64 = Convert.ToBase64String(arr);
            filestream.Close();
            return baser64;
        }

        public static String getFileBase64(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            byte[] arr = (byte[])converter.ConvertTo(img, typeof(byte[]));
            string baser64 = Convert.ToBase64String(arr);
            return baser64;
        }

        public static String getFileBase64(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            string baser64 = Convert.ToBase64String(arr);
            return baser64;
        }


        public static byte[] getImgBytes(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            byte[] imgBytesIn = br.ReadBytes((int)fs.Length);
            fs.Close();
            return imgBytesIn;
        }

        public static byte[] getImgBytes(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static void saveImage(string imageType, string path, string imageStr)
        {
            byte[] imageBytes = Convert.FromBase64String(imageStr);
            MemoryStream m = new MemoryStream(imageBytes);
            FileStream fileStream = null;
            string name = imageType + DateTime.Now.ToString("MMddHHmmss") + ".png";
            string pathName = path + @"\" + name;
            fileStream = new FileStream(pathName, FileMode.Create);

            m.WriteTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
            m.Close();
        }

        public static bool DeleteDir(string dir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                di.Delete(true);
            }
            catch (Exception )
            {
                return false;
            }

            return true;
        }


        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }


        public static string MakeRelativePath(string baseDir, string filePath)
        {
            if (string.IsNullOrEmpty(baseDir)) throw new ArgumentNullException("baseDir");
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

            if (!baseDir.EndsWith(@"\") && !baseDir.EndsWith(@"/"))
            {
                baseDir += @"\";
            }

            Uri fromUri = new Uri(baseDir);
            Uri toUri = new Uri(filePath);

            if (fromUri.Scheme != toUri.Scheme) { return filePath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        
        
        public static bool ShowSaveAsFileDialog(out string user_sel_path, string show_file, string filter_ext, string filter_desc)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = show_file;

            sfd.Filter = string.Format("{0}(*{1})|*{1}", filter_desc, filter_ext);

            sfd.FilterIndex = 1;

            sfd.RestoreDirectory = true;
 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                user_sel_path = sfd.FileName;
                return true;
            }

            user_sel_path = "";
            return false;
        }

        public static string GetMD5HashFromFile(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string MD5(string text, int length = 32)
        {
            string strEncryt = string.Empty;
            MD5CryptoServiceProvider hashmd5;
            hashmd5 = new MD5CryptoServiceProvider();
            strEncryt = BitConverter.ToString(hashmd5.ComputeHash(Encoding.Default.GetBytes(text.ToCharArray()))).Replace("-", "").ToLower();
            if (length == 16)
            {
                strEncryt = strEncryt.Substring(8, 16).ToLower();
            }

            return strEncryt;
        }


        [DllImport("shell32.dll")]
        public extern static IntPtr ShellExecute(IntPtr hwnd,
                                                 string lpOperation,
                                                 string lpFile,
                                                 string lpParameters,
                                                 string lpDirectory,
                                                 int nShowCmd
                                                );
        public enum ShowWindowCommands
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        public static void ShellExecute(string lpFile, string lpParameters = "")
        {
            ShellExecute(IntPtr.Zero, "open", lpFile, lpParameters, null, (int)Common.ShowWindowCommands.SW_SHOW);
        }


        public static string GetFileNameFromUrl(string url)
        {
            string[] strs = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var originFileName = "";
            if (strs.Length > 0)
            {
                originFileName = strs[strs.Length - 1];
            }

            return originFileName;
        }

        public static string GetStringLiteralFromArgument(InArgument<string> argument)
        {
            Literal<string> literal = argument.Expression as Literal<string>;
            if (literal != null)
            {
                return literal.Value;
            }
            VisualBasicValue<string> visualBasicValue = argument.Expression as VisualBasicValue<string>;

            if (visualBasicValue != null)
            {
                return visualBasicValue.ExpressionText;
            }
            return null;
        }


    }
}
