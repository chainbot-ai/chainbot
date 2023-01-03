using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Classes
{
    public static class LanguageUtil
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern ushort SetThreadUILanguage(ushort LangId);

        public static bool ApplyLanguage(string language)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo(language);
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.CurrentUICulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                return true;
            }
            catch (Exception ex)
            {
                
            }

            return false;
        }

        public static bool ApplyLanguageToNativeThread(ushort managedLangId)
        {
            try
            {
                if (managedLangId == 0)
                {
                }
                else
                {
                    ushort num = SetThreadUILanguage(managedLangId);
                    if (num != managedLangId)
                    {

                    }
                    else
                    {
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        //public static void SetZhCN()
        //{
        //    if(ApplyLanguage("zh-CN"))
        //    {
        //        ApplyLanguageToNativeThread((ushort)(Thread.CurrentThread.CurrentUICulture.LCID & 0xffff));
        //    }
        //}

        public static void SetLanguage(string language)
        {
            if (ApplyLanguage(language))
            {
                ApplyLanguageToNativeThread((ushort)(Thread.CurrentThread.CurrentUICulture.LCID & 0xffff));
            }
        }
    }
}
