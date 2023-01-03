using Chainbot.Contracts.App;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Project
{
    public class ProjectDependenciesService : IProjectDependenciesService
    {
        private IServiceLocator _serviceLocator;
        private IConstantConfigService _constantConfigService;

        private string _projectPath;

        public ProjectDependenciesService(IServiceLocator serviceLocator,IConstantConfigService constantConfigService)
        {
            _serviceLocator = serviceLocator;
            _constantConfigService = constantConfigService;
        }


        public bool Init(string projectPath)
        {
            _projectPath = projectPath;

            return true;
        }

        public bool Install(string nupkgPath)
        {
            try
            {
                var packageImportService = _serviceLocator.ResolveType<IPackageImportService>();
                packageImportService.Init(nupkgPath);

                var id = packageImportService.GetId();
                var ver = packageImportService.GetVersion();

                var filePath = Path.Combine(_projectPath, _constantConfigService.ProjectConfigFileName);
                var json_cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectJsonConfig>(File.ReadAllText(filePath));

                json_cfg.dependencies[id] = $"[{ver}]";

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(json_cfg, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Install(List<string> nupkgPathList)
        {
            bool isSuccess = true;

            foreach(var item in nupkgPathList)
            {
                if(!Install(item))
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        public bool Uninstall(NupkgIdVer IdVer)
        {
            try
            {
                var id = IdVer.Id;
                var ver = IdVer.Version;

                var filePath = Path.Combine(_projectPath, _constantConfigService.ProjectConfigFileName);
                var json_cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectJsonConfig>(File.ReadAllText(filePath));

                if(json_cfg.dependencies[id].ToString() == $"[{ver}]")
                {
                    json_cfg.dependencies.Remove(id);
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(json_cfg, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Uninstall(List<NupkgIdVer> IdVerList)
        {
            bool isSuccess = true;

            foreach (var item in IdVerList)
            {
                if (!Uninstall(item))
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }
    }
}
