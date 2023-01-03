using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Project;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Config
{
    public class ProjectConfigFileService : IProjectConfigFileService
    {
        private IConstantConfigService _constantConfigService;

        public ProjectJsonConfig ProjectJsonConfig { get; set; }

        public string CurrentProjectJsonFilePath { get; set; }

        public ProjectConfigFileService(IConstantConfigService constantConfigService)
        {
            _constantConfigService = constantConfigService;
        }

        public bool Load(string projectConfigFilePath)
        {
            CurrentProjectJsonFilePath = projectConfigFilePath;

            ProjectJsonConfig = ProcessProjectJsonConfig();
            return true;
        }

        public bool Save()
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(ProjectJsonConfig, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(CurrentProjectJsonFilePath, output);

            return true;
        }


        private ProjectJsonConfig ProcessProjectJsonConfig()
        {
            ProjectJsonConfig = null;

            var json_str = File.ReadAllText(CurrentProjectJsonFilePath);
            try
            {
                var json_cfg = JsonConvert.DeserializeObject<ProjectJsonConfig>(json_str);
                if (json_cfg.Upgrade())
                {
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(json_cfg, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(CurrentProjectJsonFilePath, output);

                    json_str = File.ReadAllText(CurrentProjectJsonFilePath);
                    json_cfg = JsonConvert.DeserializeObject<ProjectJsonConfig>(json_str);
                }

                return json_cfg;
            }
            catch (Exception)
            {
                throw;
            }


        }

    }
}
