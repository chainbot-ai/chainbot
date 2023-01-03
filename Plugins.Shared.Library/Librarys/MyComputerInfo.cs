using System;
using System.Management;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;

namespace Plugins.Shared.Library.Librarys
{
    public class MyComputerInfo
    {
        public string CpuID;
        public string MacAddress;
        public List<string> MacAddressList;
        public List<string> DiskIDList;
        public string DiskID;
        public string IpAddress;
        public string LoginUserName;
        public string ComputerName;
        public string SystemType;
        public string TotalPhysicalMemory; 

        public string OSVersion;
        public string OSFullName;

        private static MyComputerInfo _instance;
        public static MyComputerInfo Instance()
        {
            if (_instance == null)
                _instance = new MyComputerInfo();
            return _instance;
        }

        private MyComputerInfo()
        {
            CpuID = GetCpuID();
            MacAddressList = GetMacAddressList();
            MacAddress = GetMacAddress();
            DiskID = GetDiskID();
            DiskIDList = GetDiskIDList();
            IpAddress = GetIPAddress();
            LoginUserName = GetUserName();
            SystemType = GetSystemType();
            TotalPhysicalMemory = GetTotalPhysicalMemory();
            ComputerName = GetComputerName();

            GetOSVersionInfo();
        }

       

        private void GetOSVersionInfo()
        {
            try
            {
                var computer = new ComputerInfo();
                OSVersion = computer.OSVersion;//6.1.7601.65536
                OSFullName = computer.OSFullName;//Microsoft Windows 7 Ultimate
            }
            catch (Exception)
            {

            }
        }


        string GetCpuID()
        {
            try
            {
                string cpuInfo = "";
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }


        public bool IsExistMacAddress(string mac)
        {
            if(MacAddressList.Count == 0)
            {
                return true;
            }

            foreach(var item in MacAddressList)
            {
                if(item  == mac)
                {
                    return true;
                }
            }

            return false;
        }

        List<string> GetMacAddressList()
        {
            var macList = new List<string>();

            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    try
                    {
                        var mac = mo["MacAddress"]?.ToString();
                        if (!string.IsNullOrEmpty(mac))
                        {
                            macList.Add(mac);
                        }
                    }
                    catch (Exception err)
                    {
                    }
                }
                moc = null;
                mc = null;

                return macList;
            }
            catch (Exception err)
            {

            }

            return macList;
        }


        string GetMacAddress()
        {
            try
            { 
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        string GetIPAddress()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo["IpAddress"].ToString();   
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }


        private List<string> GetDiskIDList()
        {
            var ret = new List<string>();
            try
            { 
                String HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["SerialNumber"].Value.ToString();
                    ret.Add(HDid);
                }
                moc = null;
                mc = null;
            }
            catch
            {
               
            }
            finally
            {
            }

            return ret;
        }


        string GetDiskID()
        {
            try
            { 
                String HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    try
                    {
                        var interfaceType = mo.Properties["InterfaceType"].Value.ToString();
                        if(interfaceType.ToLower() == "USB".ToLower())
                        {
                            continue;
                        }
                    }
                    catch
                    {

                    }

                    
                    HDid = (string)mo.Properties["SerialNumber"].Value.ToString();
                    break;
                }
                moc = null;
                mc = null;
                return HDid;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        public bool IsExistDiskID(string diskId)
        {
            if (DiskIDList.Count == 0)
            {
                return true;
            }

            foreach (var item in DiskIDList)
            {
                if (item == diskId)
                {
                    return true;
                }
            }

            return false;
        }


  
        string GetUserName()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["UserName"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }


    
        string GetSystemType()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["SystemType"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        

        string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["TotalPhysicalMemory"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        string GetComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

    }
}
