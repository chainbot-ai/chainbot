using log4net;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using Chainbot.Contracts.App;
using Chainbot.Contracts.Log;
using ChainbotRobot.CommandLine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine.Cores
{
    public class ProjectService : IProjectService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string ProjectDirectory { get; set; }

        public string ProjectJsonFile { get; set; }

        public string XamlFile { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        private IServiceLocator _serviceLocator;
        private IRunManagerService _runManagerService;
        private ILogService _logService;
        private ILoadDependenciesService _loadDependenciesService;

        public ProjectService(IServiceLocator serviceLocator, IRunManagerService runManagerService, ILogService logService
            , ILoadDependenciesService loadDependenciesService)
        {
            _serviceLocator = serviceLocator;

            _runManagerService = runManagerService;
            _logService = logService;
            _loadDependenciesService = loadDependenciesService;
        }

        public void Init(string filePath)
        {
            ProjectDirectory = System.IO.Path.GetDirectoryName(filePath);

            string fileName = System.IO.Path.GetFileName(filePath);
            if (fileName.ToLower() == "project.json")
            {
                ProjectJsonFile = filePath;

                if (System.IO.File.Exists(ProjectJsonFile))
                {
                    string json = System.IO.File.ReadAllText(ProjectJsonFile);
                    JObject jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                    var relativeMainXaml = jsonObj["main"].ToString();
                    var absoluteMainXaml = System.IO.Path.Combine(ProjectDirectory, relativeMainXaml);

                    XamlFile = absoluteMainXaml;

                    Name = jsonObj["name"].ToString();
                    Version = jsonObj["projectVersion"].ToString();
                }
            }
            else
            {
                ProjectJsonFile = ProjectDirectory + @"\project.json";

                XamlFile = filePath;

                if (System.IO.File.Exists(ProjectJsonFile))
                {
                    string json = System.IO.File.ReadAllText(ProjectJsonFile);
                    JObject jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);

                    Name = jsonObj["name"].ToString();
                    Version = jsonObj["projectVersion"].ToString();
                }
            }
        }


        private void RunWorkflow(List<string> activitiesDllLoadFrom, List<string> dependentAssemblies)
        {
            System.GC.Collect();

            SharedObject.Instance.ProjectPath = ProjectDirectory;
            SharedObject.Instance.ClearOutputEvent();
            SharedObject.Instance.OutputEvent += Instance_OutputEvent;

            _runManagerService.Init(Name, Version, XamlFile, activitiesDllLoadFrom, dependentAssemblies);
            _runManagerService.Run();
        }

        private void Instance_OutputEvent(SharedObject.enOutputType type, string msg, string msgDetails = "")
        {
            _logService.Debug(string.Format("活动日志：type={0},msg={1},msgDetails={2}", type, msg, msgDetails), _logger);
        }

        public void Start()
        {
            if (System.IO.File.Exists(ProjectJsonFile))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var serv = _serviceLocator.ResolveType<ILoadDependenciesService>();
                        serv.Init(ProjectJsonFile);
                        await serv.LoadDependencies();

                        RunWorkflow(serv.CurrentActivitiesDllLoadFrom, serv.CurrentDependentAssemblies);
                    }
                    catch (Exception err)
                    {
                        _logService.Error(err, _logger);
                    }
                });
            }
        }


        public void Stop()
        {
            _runManagerService.Stop();
        }

    }
}
