using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Config
{
    public class ConstantConfigService : MarshalByRefServiceBase, IConstantConfigService
    {
        public string StudioName
        {
            get
            {
                return "ChainbotStudio";
            }
        }

        public string ProjectCreateName
        {
            get
            {
                //System.Reflection.Assembly dll = System.Reflection.Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "ChainbotStudio.exe");
                //ResourceManager resourceManager = new ResourceManager(dll.GetName().Name + ".Properties.Resources", dll);
                //object obj = resourceManager.GetObject("NewProject_NameContent");
                //return obj.ToString();

                return Chainbot.Resources.Properties.Resources.ProjectCreateName;
            }
        }

        public string ProjectConfigFileName
        {
            get
            {
                return "project"+ GlobalConstant.ProjectConfigFileExtension;
            }
        }

        public string ProjectConfigFileExtension
        {
            get
            {
                return GlobalConstant.ProjectConfigFileExtension;
            }
        }

        public string ProjectDefaultDependentPackagesMatchRegex
        {
            get
            {
                return @".*\.Activities$";
            }
        }

        public string MainXamlFileName
        {
            get
            {
                return "Main"+ XamlFileExtension;
            }
        }

        public string XamlFileExtension
        {
            get
            {
                return GlobalConstant.XamlFileExtension;
            }
        }

        public int RecentProjectsMaxRecordCount
        {
            get
            {
                return 100;
            }
        }


        public string ProjectActivitiesAssemblyMatchRegex
        {
            get
            {
                return @".*\.Activities.dll$";
            }
        }

        public int ExportDirHistoryMaxRecordCount
        {
            get
            {
                return 10;
            }

        }

        public string ProjectLocalDirectoryName
        {
            get
            {
                return ".local";
            }
        }

        public string ProjectSettingsFileName
        {
            get
            {
                //return "ProjectSettings.json";
                return "ProjectBreakpoints.json";
            }
        }

        public int ActivitiesRecentGroupMaxRecordCount
        {
            get
            {
                return 30;
            }
        }

        public string ScreenshotsPath
        {
            get
            {
                return ".screenshots";
            }
        }
    }




}
