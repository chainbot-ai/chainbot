using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Contracts.Classes
{
    public class ProjectJsonConfig
    {
        private static readonly string initial_schema_version = "2.0.0";
        private static readonly string initial_project_version = "2.0.0";

        [JsonProperty(Order = 1)]
        public string schemaVersion { get; set; }

        [JsonProperty(Required = Required.Always, Order = 2)]
        public string studioVersion { get; set; }

        [JsonProperty(Required = Required.Always, Order = 3)]
        public string projectType { get; set; }

        [JsonProperty(Order = 4)]
        public string projectVersion { get; set; }

        [JsonProperty(Required = Required.Always, Order = 5)]
        public string name { get; set; }

        [JsonProperty(Order = 6)]
        public string description { get; set; }

        [JsonProperty(Required = Required.Always, Order = 7)]
        public string main { get; set; }

        [JsonProperty(Order = 8)]
        public JObject dependencies = new JObject();


        public bool Upgrade()
        {
            var schemaVersionTmp = schemaVersion;

            Upgrade(ref schemaVersionTmp, initial_schema_version);

            if (schemaVersion == schemaVersionTmp)
            {
                return false;
            }
            else
            {
                schemaVersion = schemaVersionTmp;
                return true;
            }
        }

        private void Upgrade(ref string schemaVersion, string newSchemaVersion)
        {
            if (schemaVersion == newSchemaVersion)
            {
                return;
            }

            Version currentVersion = new Version(schemaVersion);
            Version latestVersion = new Version(newSchemaVersion);

            if (currentVersion < new Version("2.0.0.0"))
            {
                var err = "当前程序不再支持V2.0版本以下的旧版本项目！";
                MessageBox.Show(err);
                throw new Exception(err);
            }

            Upgrade(ref schemaVersion, initial_schema_version);
        }

        public void Init()
        {
            schemaVersion = initial_schema_version;
            projectVersion = initial_project_version;
        }
    }
}
