using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IConstantConfigService
    {
        string StudioName { get;}

        string ProjectCreateName { get; }

        string ProjectConfigFileName { get; }

        string ProjectConfigFileExtension { get; }


        string ProjectDefaultDependentPackagesMatchRegex { get; }

        string ProjectActivitiesAssemblyMatchRegex { get; }

        string MainXamlFileName { get; }


        string ProjectLocalDirectoryName { get; }


        string ProjectSettingsFileName { get; }

        
        string XamlFileExtension { get; }

        int RecentProjectsMaxRecordCount { get;}

        int ExportDirHistoryMaxRecordCount { get;}

        int ActivitiesRecentGroupMaxRecordCount { get;}

        string ScreenshotsPath { get; }
    }
}
