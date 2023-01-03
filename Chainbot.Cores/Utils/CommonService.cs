using Chainbot.Contracts.Log;
using Chainbot.Contracts.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;

namespace Chainbot.Cores.Utils
{
    public class CommonService : ICommonService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ImageSourceConverter _imageSourceConverter = new ImageSourceConverter();

        private ILogService _logService;

        public CommonService(ILogService logService)
        {
            _logService = logService;
        }

        public string GetProgramVersion()
        {
            FileVersionInfo myFileVersion = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.ExecutablePath);
            return myFileVersion.FileVersion;
        }

        public string GetValidDirectoryName(string path, string name, string suffix_format = " ({0})", int begin_index = 2)
        {
            var validName = name;
            if (Directory.Exists(path + @"\" + name))
            {
                for (int i = begin_index; ; i++)
                {
                    var format_i = string.Format(suffix_format, i);
                    if (!Directory.Exists(path + @"\" + name + format_i))
                    {
                        validName = name + format_i;
                        break;
                    }
                }
            }

            return validName;
        }


        public string MakeRelativePath(string baseDir, string filePath)
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

        public BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int size = 32, int dpi = 96)
        {
            var pixelFormat = PixelFormats.Pbgra32;
            RenderTargetBitmap rtb = new RenderTargetBitmap(size, size, dpi, dpi, pixelFormat);

            var drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(drawingBrush, null, new Rect(0, 0, size, size));
            }

            rtb.Render(drawingVisual);
            return rtb;
        }


        public ImageSource ToImageSource(string uri)
        {
            if(string.IsNullOrEmpty(uri))
            {
                return null;
            }

            return _imageSourceConverter.ConvertFromInvariantString(uri) as ImageSource;
        }

        public void LocateDirInExplorer(string dir)
        {
            Process.Start("explorer.exe", dir);
        }

        public bool DeleteDir(string path)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
                return false;
            }

            return true;
        }




        public bool DirectoryChildrenForEach(DirectoryInfo di, CheckDeleage checkFun, object param = null)
        {
            DirectoryInfo[] dis = di.GetDirectories();
            for (int j = 0; j < dis.Length; j++)
            {
                DirectoryInfo diItem = dis[j];
                if (checkFun != null && checkFun(diItem, param))
                {
                    return true;
                }

                if (DirectoryChildrenForEach(diItem, checkFun, param))
                {
                    return true;
                }
            }

            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                FileInfo fiItem = fis[i];

                if (checkFun != null && checkFun(fiItem, param))
                {
                    return true;
                }
            }


            return false;

        }


        public bool IsStringInFile(string fileName, string searchString)
        {
            return File.ReadAllText(fileName).Contains(searchString);
        }


        public bool DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
                return false;
            }

            return true;
        }

        public string GetValidFileName(string path, string name, string prefix_format = "", string suffix_format = " ({0})", int begin_index = 2)
        {
            var ext = Path.GetExtension(path + @"\" + name);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path + @"\" + name);

            var validName = name;
            if (File.Exists(path + @"\" + name))
            {
                if (File.Exists(path + @"\" + fileNameWithoutExt + prefix_format + ext))
                {
                    for (int i = begin_index; ; i++)
                    {
                        var format_i = string.Format(prefix_format + suffix_format, i);
                        if (!File.Exists(path + @"\" + fileNameWithoutExt + format_i + ext))
                        {
                            validName = fileNameWithoutExt + format_i + ext;
                            break;
                        }
                    }
                }
                else
                {
                    validName = fileNameWithoutExt + prefix_format + ext;
                }


            }

            return validName;
        }


        private string defaultSelectDirPath = "";
        public bool ShowSelectDirDialog(string desc, ref string select_dir)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = desc;
            if (defaultSelectDirPath != "")
            {
                dialog.SelectedPath = defaultSelectDirPath;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return false;
                }

                select_dir = dialog.SelectedPath;

                defaultSelectDirPath = dialog.SelectedPath;
                return true;
            }

            return false;
        }


        public string GetDirectoryNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)")
        {
            Match match = Regex.Match(name, pattern);
            if (match.Success)
            {
                if (match.Groups.Count == 2)
                {
                    name = match.Groups[1].Value;
                }
            }

            return name;
        }


        public string GetFileNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)")
        {
            var ext = Path.GetExtension(name);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);

            Match match = Regex.Match(fileNameWithoutExt, pattern);
            if (match.Success)
            {
                if (match.Groups.Count == 2)
                {
                    fileNameWithoutExt = match.Groups[1].Value;
                }
            }

            return fileNameWithoutExt + ext;
        }

        public string ShowSelectSingleFileDialog(string filter = null, string title = "")
        {
            string select_file = "";

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false; 
            fileDialog.Filter = filter;
            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileDialog.FileNames)
                {
                    select_file = file;
                }
            }

            return select_file;
        }

        public List<string> ShowSelectMultiFileDialog(string filter = null, string title = "")
        {
            List<string> fileList = new List<string>();

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = filter;
            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileDialog.FileNames)
                {
                    fileList.Add(file);
                }
            }

            return fileList;
        }


        public bool ShowSaveAsFileDialog(out string user_sel_path, string show_file, string filter_ext, string filter_desc, string title = "")
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = show_file;

            sfd.Filter = string.Format("{0}(*{1})|*{1}", filter_desc, filter_ext);

            sfd.FilterIndex = 1;

            sfd.RestoreDirectory = true;

            if (!string.IsNullOrEmpty(title))
            {
                sfd.Title = title;
            }

            //点了保存按钮进入 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                user_sel_path = sfd.FileName;
                return true;
            }

            user_sel_path = "";
            return false;
        }

        public string GetIp()
        {
            try
            {
                IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress item in IpEntry.AddressList)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return item.ToString();
                    }
                }
                return "";
            }
            catch { return ""; }
        }

        public string GetMachineId()
        {
            var machineId = "";

            var aes256 = new AesEverywhere.AES256();

            var aesKey = "{1C38D412-ECD0-4F81-871F-D8233477D416}";

            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("{6A1B908B-9DE1-4AAC-9636-87A307D636DE}", true);
            if (registryKey == null)
            {
                registryKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("{6A1B908B-9DE1-4AAC-9636-87A307D636DE}");
            }

            var content = (string)registryKey.GetValue("", "");
            if (string.IsNullOrEmpty(content))
            {
                JObject rootJsonObj = new JObject();
                rootJsonObj["machine_id"] = Guid.NewGuid().ToString("N");
                string json_str = Newtonsoft.Json.JsonConvert.SerializeObject(rootJsonObj);

                registryKey.SetValue("", aes256.Encrypt(json_str, aesKey));

                machineId = rootJsonObj["machine_id"].ToString();
            }
            else
            {
                var json_str = aes256.Decrypt(content, aesKey);
                JObject rootJsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json_str) as JObject;

                machineId = rootJsonObj["machine_id"].ToString();
            }

            return machineId;
        }

        public void LocateFileInExplorer(string file)
        {
            Process.Start("explorer.exe", "/select," + file);
        }

        public void DirectoryFilesCopy(string srcdir, string destdir, bool recursive = true)
        {
            DirectoryInfo dir;
            FileInfo[] files;
            DirectoryInfo[] dirs;
            string tmppath;

            if (!Directory.Exists(destdir))
            {
                Directory.CreateDirectory(destdir);
            }

            dir = new DirectoryInfo(srcdir);

            if (!dir.Exists)
            {
                throw new ArgumentException("source     dir     doesn't     exist     ->     " + srcdir);
            }

            files = dir.GetFiles();
              
            foreach (FileInfo file in files)
            {  
                tmppath = Path.Combine(destdir, file.Name);
                file.CopyTo(tmppath, false);
            }
  
            files = null;
    
            if (!recursive)
            {
                return;
            }

            dirs = dir.GetDirectories();
   
            foreach (DirectoryInfo subdir in dirs)
            {  
                tmppath = Path.Combine(destdir, subdir.Name);
                DirectoryFilesCopy(subdir.FullName, tmppath, recursive);
            }
     
            dirs = null;
            dir = null;
        }

        public string GetNupkgMd5(string packageName)
        {
            try
            {
                string result = "";

                string packagePath = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, "Packages", packageName);
                if (File.Exists(packagePath))
                {
                    result = Common.GetMD5HashFromFile(packagePath);
                }

                return result;
            }
            catch
            {
                return "";
            }
        }

    }
}
