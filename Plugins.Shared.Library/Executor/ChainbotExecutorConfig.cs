using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Executor
{
    public class ChainbotExecutorStartupConfig
    {
        public bool IsInDebuggingState { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string MainXamlPath { get; set; }

        public string PipeName { get; set; }

        public List<string> LoadAssemblyFromList { get; set; }

        public List<string> AssemblyResolveDllList { get; set; }
        

        public string ProjectPath { get; set; }


        public JObject InitialDebugJsonConfig { get; set; } = new JObject();


        public JObject InitialCommandLineJsonConfig { get; set; } = new JObject();

        public JObject JsonParams { get; set; } = new JObject();

    }
}
