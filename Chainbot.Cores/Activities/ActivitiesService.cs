using Chainbot.Contracts.Activities;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using log4net;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Chainbot.Contracts.Utils;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Activities.Presentation.ViewState;
using Chainbot.Cores.Classes;
using System.Diagnostics;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace Chainbot.Cores.Activities
{
    public class ActivitiesService : MarshalByRefServiceBase, IActivitiesService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;

        private IConstantConfigService _constantConfigService;
        private IAssemblyResolveService _assemblyResolveService;
        private ICommonService _commonService;
        private IPathConfigService _pathConfigService;

        public List<string> Assemblies { get; private set; } = new List<string>();

        public List<string> CustomActivityConfigXmls { get; private set; } = new List<string>();

        public ConcurrentDictionary<string, string> ActivityConfCache = new ConcurrentDictionary<string, string>();

        public ActivitiesService(IConstantConfigService constantConfigService, 
            IAssemblyResolveService assemblyResolveService, 
            ILogService logService, 
            ICommonService commonService, 
            IPathConfigService pathConfigService)
        {
            _constantConfigService = constantConfigService;
            _assemblyResolveService = assemblyResolveService;
            _logService = logService;
            _commonService = commonService;
            _pathConfigService = pathConfigService;
        }

        public List<string> Init(List<string> assemblies)
        {
            Assemblies = assemblies;

            _assemblyResolveService.Init(Assemblies);

            CustomActivityConfigXmls.Clear();

            var list = Assemblies;//.Where(item => Regex.IsMatch(item, _constantConfigService.ProjectActivitiesAssemblyMatchRegex, RegexOptions.IgnoreCase)).ToList();

            foreach(var dll_file in list)
            {
                try
                {
                    if (Path.GetExtension(dll_file).ToLower() != ".dll")
                    {
                        continue;
                    }

                    var checkPath = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory,Path.GetFileName(dll_file));
                    if (System.IO.File.Exists(checkPath))
                    {
                        _logService.Debug($"The dll with the same name is found in the directory of the main program, and the load is ignored {dll_file}", _logger);
                        continue;
                    }

                    Assembly.LoadFrom(dll_file);

                    var dll_file_name_without_ext = Path.GetFileNameWithoutExtension(dll_file);
                    if (Regex.IsMatch(dll_file_name_without_ext, _constantConfigService.ProjectDefaultDependentPackagesMatchRegex, RegexOptions.IgnoreCase))
                    {
                        string packageName = dll_file.Replace(_pathConfigService.PackagesDir, "").Split('\\')[1];
                        string activityConfCachePath = Path.Combine(_pathConfigService.ActivitiesConfigCacheDir, packageName + "_" + GlobalConfig.CurrentLanguage.ToString() + ".json");

                        string activity_config_xml = GetActivityConfigCache(packageName, activityConfCachePath);

                        if (!string.IsNullOrEmpty(activity_config_xml))
                        {
                            CustomActivityConfigXmls.Add(activity_config_xml);
                        }
                        else
                        {
                            try
                            {
                                string configName = "activity.config.xml";
                                if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
                                {
                                    configName = "activity.config.zh-CN.xml";
                                }

                                var activity_config_info = Application.GetResourceStream(new Uri($"pack://application:,,,/{dll_file_name_without_ext};Component/{configName}", UriKind.Absolute));

                                using (StreamReader reader = new StreamReader(activity_config_info.Stream))
                                {
                                    activity_config_xml = reader.ReadToEnd();
                                }

                                CustomActivityConfigXmls.Add(activity_config_xml);

                                SaveActivityConfigCache(activity_config_xml, packageName, activityConfCachePath);
                            }
                            catch (Exception err)
                            {
                                SharedObject.Instance.Output(SharedObject.enOutputType.Error, string.Format(Chainbot.Resources.Properties.Resources.Message_ActivitiesMountError, dll_file), err);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return list;
        }


        private string GetActivityConfigCache(string packageName, string activityConfCachePath)
        {
            string activity_config_xml = null;

            try
            {
                if (ActivityConfCache.ContainsKey(packageName))
                {
                    activity_config_xml = ActivityConfCache[packageName];
                }
                else
                {
                    if (File.Exists(activityConfCachePath))
                    {
                        JObject jo;
                        using (StreamReader streamReader = new StreamReader(activityConfCachePath))
                        {
                            jo = JObject.Parse(streamReader.ReadToEnd());
                        }

                        string md5 = jo["md5"]?.ToString();
                        string config = jo["config"]?.ToString();

                        if (_commonService.GetNupkgMd5(packageName + ".nupkg") == md5 && !string.IsNullOrEmpty(config))
                        {
                            activity_config_xml = config;
                            ActivityConfCache[packageName] = config;
                        }
                    }
                }

                return activity_config_xml;
            }
            catch
            {
                return null;
            }
        }

        private void SaveActivityConfigCache(string activity_config_xml, string packageName, string activityConfCachePath)
        {
            try
            {
                ActivityConfCache[packageName] = activity_config_xml;

                JObject jo = new JObject();
                jo["md5"] = _commonService.GetNupkgMd5(packageName + ".nupkg");
                jo["config"] = activity_config_xml;

                using (StreamWriter streamWriter = new StreamWriter(new FileStream(activityConfCachePath, FileMode.Create, FileAccess.Write)))
                {
                    streamWriter.Write(jo.ToString(Formatting.Indented, new JsonConverter[0]));
                }
            }
            catch
            {
            }
        }

        public Stream GetIcon(string assemblyName,string relativeResPath)
        {
            try
            {
                var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{assemblyName};Component/{relativeResPath}", UriKind.Absolute)).Stream;

                return stream;
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, err);
                return null;
            }
        }

        public string GetAssemblyQualifiedName(string typeOf)
        {
            try
            {
                Type type = null;

                if (typeOf.Contains(","))
                {
                    type = Type.GetType(typeOf);

                    if (type == null)
                    {
                        string[] sArray = typeOf.Split(',');
                        if (sArray.Length > 1)
                        {
                            sArray[0] += "`1[[System.Object,mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089]]";
                        }
                        type = Type.GetType(string.Join(",", sArray));
                    }

                    if (type == null)
                    {
                        string[] sArray = typeOf.Split(',');
                        if (sArray.Length > 1)
                        {
                            sArray[0] += "`2[[System.Object,mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089]]";
                        }

                        type = Type.GetType(string.Join(",", sArray));
                    }
                }
                else
                {
                    if (typeOf == "FinalState")
                    {
                        type = typeof(System.Activities.Core.Presentation.FinalState);
                    }
                    else if (typeOf == "ForEach")
                    {
                        type = typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<object>);
                    }
                    else if (typeOf == "ParallelForEach")
                    {
                        type = typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<object>);
                    }
                    else if (typeOf == "Switch")
                    {
                        type = typeof(System.Activities.Statements.Switch<string>);
                    }
                    else
                    {
                        type = Type.GetType(string.Format("System.Activities.Statements.{0},System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeOf));
                        if (type == null)
                        {
                            type = Type.GetType(string.Format("System.Activities.Statements.{0}`1[[System.Object,mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089]],System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeOf));
                        }

                        if (type == null)
                        {
                            type = Type.GetType(string.Format("System.Activities.Statements.{0}`2[[System.Object,mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089]],System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeOf));
                        }
                    }
                }


                if (type != null)
                {
                    return type.AssemblyQualifiedName;
                }
                else
                {
                    _logService.Error(string.Format("{0} type not found！", typeOf), _logger);
                }

            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, err);
                
            }

            return "";
        }


        public void SetSharedObjectInstance(object instance)
        {
            SharedObject.SetCrossDomainInstance(instance as SharedObject);
        }

        public bool IsXamlValid(string xamlPath)
        {
            Activity workflow = ActivityXamlServices.Load(xamlPath);

            var result = ActivityValidationServices.Validate(workflow);
            if (result.Errors.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool IsXamlStringValid(string xamlString, string xamlPath)
        {
            try
            {
                Activity workflow = ActivityXamlServices.Load(new StringReader(xamlString));

                var result = ActivityValidationServices.Validate(workflow, new System.Activities.Validation.ValidationSettings
                {
                    SkipValidatingRootConfiguration = true
                });

                if (result.Errors.Count == 0)
                {
                    return true;
                }
                else
                {
                    foreach (var err in result.Errors)
                    {
                        try
                        {
                            var source = WorkflowViewState.GetIdRef(err.Source) != null ? err.Source : err.Source.GetParent();
                            var displayName = (source as Activity).DisplayName;
                            var idRef = WorkflowViewState.GetIdRef(source);
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, $"{displayName}：{err.Message}@{idRef}@{xamlPath}");
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError(ex.ToString());
                        }
                    }

                    return false;
                }
            }
            catch (Exception err)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, err.Message);
                return false;
            }
        }
    }
}
