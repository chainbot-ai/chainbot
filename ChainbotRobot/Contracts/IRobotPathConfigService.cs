using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IRobotPathConfigService
    {
        string AppDataDir { get; }

        string LogsDir { get; }

        string ConfigDir { get; }

        string PackagesDir { get; }

        string ScreenRecorderDir { get; }

        string AppProgramDataDir { get; }

        string ProgramDataPackagesDir { get; }

        string ProgramDataInstalledPackagesDir { get; }

        void InitDirs();
    }
}
