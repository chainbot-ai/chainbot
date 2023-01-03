using log4net;
using Plugins.Shared.Library.Librarys;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using ChainbotRobot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Cores
{
    public class PackageService:IPackageService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;

        private MainViewModel _mainViewModel;

        public PackageService(MainViewModel mainViewModel, ILogService logService)
        {
            _mainViewModel = mainViewModel;
            _logService = logService;
        }


        public void Run(string name)
        {
            Common.RunInUI(async () =>
            {
                string runningName, runningVersion;
                bool existRun = GetRunningPackage(out runningName, out runningVersion);

                var version = GetPackageVersionByName(name);

                if (existRun)
                {
                    if (runningName == name && runningVersion == version)
                    {
                        return;
                    }
                    else
                    {
                        await StopCurrentRun(runningName, runningVersion);
                    }
                }

                if (!string.IsNullOrEmpty(version))
                {
                    RealRun(name, version);
                }
            });
        }

        public void Run(string name, string version)
        {
            Common.RunInUI(async () =>
            {
                string runningName, runningVersion;
                bool existRun = GetRunningPackage(out runningName, out runningVersion);

                if (existRun)
                {
                    if (runningName == name && runningVersion == version)
                    {
                        return;
                    }
                    else
                    {
                        await StopCurrentRun(runningName, runningVersion);
                    }
                    
                }


                RealRun(name, version);
            });

        }


        private void RealRun(string name, string version)
        {
            bool hasFound = false;
            foreach (var item in _mainViewModel.PackageItems)
            {
                if (item.Name == name && item.Version == version)
                {
                    if (item.IsNeedUpdate)
                    {
                        item.UpdateCommand.Execute(null);
                    }

                    item.StartCommand.Execute(null);

                    Common.RunInUI(() =>
                    {
                        _mainViewModel.RefreshAllPackages();
                    });

                    hasFound = true;
                    break;
                }
            }

            if(!hasFound)
            {
                _logService.Error($"The process you want to run is not found. Name: {name} Version: {version}",_logger);
            }
        }


        private bool GetRunningPackage(out string name, out string version)
        {
            foreach (var item in _mainViewModel.PackageItems)
            {
                if (item.IsRunning)
                {
                    name = item.Name;
                    version = item.Version;

                    return true;
                }
            }

            name = "";
            version = "";
            return false;
        }


        private async Task StopCurrentRun(string runningName, string runningVersion)
        {
            _logService.Error($"Forcibly stop the currently running process, Name: {runningName} Version: {runningVersion}", _logger);
            _mainViewModel.StopCommand.Execute(null);

            await Task.Delay(5000);
        }


        private string GetPackageVersionByName(string name)
        {
            foreach (var item in _mainViewModel.PackageItems)
            {
                if (item.Name == name)
                {
                    return item.Version;
                }
            }

            return null;
        }

        public bool IsPackageNameExist(string name)
        {
            foreach (var item in _mainViewModel.PackageItems)
            {
                if(item.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
