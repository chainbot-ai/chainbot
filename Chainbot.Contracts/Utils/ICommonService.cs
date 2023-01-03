using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chainbot.Contracts.Utils
{
    public delegate bool CheckDeleage(object item, object param);

    public interface ICommonService
    {
        string GetValidDirectoryName(string path, string name, string suffix_format = " ({0})", int begin_index = 2);

        string GetProgramVersion();

        string ShowSelectSingleFileDialog(string filter = null, string title = "");

        string MakeRelativePath(string baseDir, string filePath);

        BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int size = 32, int dpi = 96);
        
        ImageSource ToImageSource(string uri);

        void LocateDirInExplorer(string dir);

        bool DeleteDir(string path);

        bool DirectoryChildrenForEach(DirectoryInfo di, CheckDeleage checkFun, object param = null);

        bool IsStringInFile(string fileName, string searchString);

        bool DeleteFile(string file);

        string GetValidFileName(string path, string name, string prefix_format = "", string suffix_format = " ({0})", int begin_index = 2);

        bool ShowSelectDirDialog(string desc, ref string select_dir);

        string GetDirectoryNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)");

        string GetFileNameWithoutSuffixFormat(string name, string pattern = @"(.*) \([0-9]+\)");

        List<string> ShowSelectMultiFileDialog(string filter = null, string title = "");

        bool ShowSaveAsFileDialog(out string user_sel_path, string show_file, string filter_ext, string filter_desc, string title = "");

        string GetIp();

        string GetMachineId();

        void LocateFileInExplorer(string file);

        void DirectoryFilesCopy(string srcdir, string destdir, bool recursive = true);

        string GetNupkgMd5(string packageName);
    }
}
