using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugins.Shared.Library.Librarys
{
    public class ReadWriteIniFile
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultVal, StringBuilder returnVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);



        public string FileName
        {
            set
            {
                fileName = string.IsNullOrEmpty(value) ? "my.ini" : value;
                FileInfo fileInfo = new FileInfo(fileName);
                if ((!fileInfo.Exists))
                {
                    StreamWriter sw = new StreamWriter(fileName, false, Encoding.Unicode);
                    try
                    {
                        sw.Close();
                    }
                    catch (Exception)
                    {
                        throw (new ApplicationException("Ini文件不存在"));
                    }
                }

                fileName = fileInfo.FullName;
            }
        }

        private ReadWriteIniFile()
        {
            fileName = string.Empty;
        }

        public static ReadWriteIniFile Instance
        {
            get
            {
                return instance;
            }
        }

        public void WriteString(string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, fileName);
            }
            catch (Exception)
            {
                throw (new Exception("写入Ini文件出错"));
            }
        }

        public string ReadString(string section, string key, string defaultVal)
        {
            StringBuilder temp = new StringBuilder(500);
            try
            {
                GetPrivateProfileString(section, key, null, temp, 500, fileName);
            }
            catch (Exception)
            {
                throw (new Exception("读取Ini文件出错"));
            }

            return temp.ToString();
        }



        public string[] INIGetAllSectionNames()
        {
            uint MAX_BUFFER = 32767;    

            string[] sections = new string[0];      


            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            uint bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, fileName);
            if (bytesReturned != 0)
            {

                string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();

                sections = local.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);
            return sections;
        }


        public string[] INIGetAllItems(string section)
        {
            uint MAX_BUFFER = 32767;    

            string[] items = new string[0];     

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, fileName);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {

                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            Marshal.FreeCoTaskMem(pReturnedString);     
            return items;
        }



        public void DeleteSection(string section)
        {
            try
            {
                WritePrivateProfileString(section, null, null, fileName);
            }
            catch (Exception)
            {
                throw (new Exception("无法清除Ini文件中的节"));
            }
        }

        public void DeleteKey(string section, string key)
        {
            WritePrivateProfileString(section, key, null, fileName);
        }


        public void UpdateFile()
        {
            WritePrivateProfileString(null, null, null, fileName);
        }

        ~ReadWriteIniFile()
        {
            UpdateFile();
        }

        private static readonly ReadWriteIniFile instance = new ReadWriteIniFile();

        private string fileName; 
    }
}
