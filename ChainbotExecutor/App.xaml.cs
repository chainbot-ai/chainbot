using log4net;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Executor;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using ChainbotExecutor.Librarys;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using Plugins.Shared.Library.UiAutomation;
using Chainbot.Contracts.Classes;
using static Chainbot.Contracts.Classes.GlobalConfig;
using System.Xml;

namespace ChainbotExecutor
{
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DispatcherTimer quitTimer { get; set; } = new DispatcherTimer();

        protected string[] Arguments { get; } = Environment.GetCommandLineArgs().Skip(1).ToArray<string>();

        private List<string> assemblyResolveDllList;

        private ChainbotExecutorAgent agent = new ChainbotExecutorAgent();
        private string guid;
        private string rpaFlag;
        private string m_controllerIpcName;

        IWorkflowManager manager;


        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();


        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            AllocConsole();
#else
            Console.SetOut(new LogToOutputWindowTextWriter());
#endif

            Logger.Debug("Application Start", logger);

            if (Arguments.Length < 3)
            {
                Logger.Debug("The number of parameters is insufficient, the program exits.", logger);
                Environment.Exit(-1);
            }

            guid = Arguments[0];
            rpaFlag = Arguments[1];
            m_controllerIpcName = Arguments[2];

            Logger.Debug($"Startup parameters: {guid} {rpaFlag} {m_controllerIpcName}", logger);

            LoadBinReferencedAssemblies();

            UiElement.Init();

            SetLanguage();

            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            agent.Init(rpaFlag, m_controllerIpcName);
            agent.StartEvent += Agent_StartEvent;
            agent.StopEvent += Agent_StopEvent;
            agent.ContinueEvent += Agent_ContinueEvent;
            agent.ExitEvent += Agent_ExitEvent;
            agent.UpdateAgentConfigEvent += Agent_UpdateAgentConfigEvent;
            agent.S2CNotificationEvent += Agent_S2CNotificationEvent;

            Logger.Debug($"Execute register command: guid={guid},rpaFlag={rpaFlag}", logger);
            agent.Register(guid);
        }

        private void SetLanguage()
        {
            enLanguage currentLanguage = GlobalConfig.DefaultLanguage;
            try
            {
                string localApplicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appSettingsPath = Path.Combine(localApplicationDataDir, @"ChainbotStudio\Config\AppSettings.xml");
                XmlDocument doc = new XmlDocument();
                doc.Load(appSettingsPath);
                var rootNode = doc.DocumentElement;

                var languageElement = rootNode.SelectSingleNode("Language") as XmlElement;
                if (languageElement != null)
                {
                    var currentLanguageElement = languageElement.SelectSingleNode("CurrentLanguage") as XmlElement;
                    currentLanguage = (enLanguage)Enum.Parse(typeof(enLanguage), currentLanguageElement.InnerText, true);
                }
            
            }
            catch (Exception)
            {
            }

            GlobalConfig.CurrentLanguage = currentLanguage;

            if (GlobalConfig.CurrentLanguage == enLanguage.English)
            {
                LanguageUtil.SetLanguage("en-US");
            }
            else if (GlobalConfig.CurrentLanguage == enLanguage.Chinese)
            {
                LanguageUtil.SetLanguage("zh-CN");
            }
        }

        private List<string> GetBinAssemblies()
        {
            List<string> list = new List<string>();
            try
            {
                foreach (string text in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
                {
                    try
                    {
                        list.Add(text);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }
            }
            catch (Exception ex2)
            {
                Trace.TraceError(ex2.ToString());
            }
            return list;
        }

        private void LoadBinReferencedAssemblies()
        {
            var list = GetBinAssemblies();
            foreach(var assemblyFile in list)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(assemblyFile);
                    if(IgnoreAssemblyName(fileName))
                    {
                        continue;
                    }

                    var asms = Assembly.LoadFrom(assemblyFile).GetReferencedAssemblies();
                    foreach(var item in asms)
                    {
                        try
                        {
                            if (IgnoreAssemblyName(item.Name))
                            {
                                continue;
                            }

                            Assembly.Load(item);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                catch (Exception)
                {

                }
                
            }
        }

        private bool IgnoreAssemblyName(string name)
        {
            var names = new string[] { "NPinyinPro" };

            if (names.Contains(name))
            {
                return true;
            }

            return false;
        }

        private void Agent_ContinueEvent(string id, string next_operate)
        {
            if(manager is WorkflowDebuggerManager)
            {
                var m = manager as WorkflowDebuggerManager;
                m.Continue();
            }
        }

        private void Agent_UpdateAgentConfigEvent(string id, string key, string val)
        {
            Logger.Debug($"update_agent_config,id={id},key={key},val={val}", logger);
            if (manager != null)
            {
                manager.SetConfig(key,val);
            }
        }

        private void Agent_S2CNotificationEvent(string id, string notification, string notification_details)
        {
            Logger.Debug($"s2c_notification,id={id},notification={notification},notification_details={notification_details}", logger);

            SharedObject.Instance.S2CNotify(notification, notification_details);
        }


        private void Agent_StartEvent(bool is_in_debugging_state, JObject debug_json_cfg, JObject command_line_json_cfg, string name, string version, string mainXamlPath
            , List<string> loadAssemblyFromList, List<string> assemblyResolveDllList, string projectPath, JObject json_params)
        {
            var desc = is_in_debugging_state ? "Debugging" : "Running";
            Logger.Debug($"Receive start command, Start {desc} the workflow: " + mainXamlPath, logger);

            this.assemblyResolveDllList = assemblyResolveDllList;

            SharedObject.Instance.ProjectPath = projectPath;
            SharedObject.Instance.ClearOutputEvent();
            SharedObject.Instance.OutputEvent += Instance_OutputEvent;

            SharedObject.Instance.ClearNotifyEvent();
            SharedObject.Instance.NotifyEvent += Instance_NotifyEvent;

            SharedObject.Instance.JsonParams = json_params;

            foreach (var dll_file in loadAssemblyFromList)
            {
                try
                {
                    var checkPath = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, Path.GetFileName(dll_file));
                    if (System.IO.File.Exists(checkPath))
                    {
                        continue;
                    }

                    Assembly.LoadFrom(dll_file);
                }
                catch (Exception err)
                {
                    
                }
                
            }

            if(is_in_debugging_state)
            {
                Logger.Debug("Initialize the debug manager", logger);
                manager = new WorkflowDebuggerManager(agent, name, version, mainXamlPath);
                initDebugJsonConfig(debug_json_cfg);
            }
            else
            {
                manager = new WorkflowRunManager(agent, name, version, mainXamlPath);
            }

            if(rpaFlag == "ChainbotRobot.CommandLine")
            {
                initCommandLineJsonConfig(command_line_json_cfg);
            }

            Directory.SetCurrentDirectory(SharedObject.Instance.ProjectPath);
            manager.Run();
        }

        private void Instance_NotifyEvent(string notification, string notificationDetails)
        {
            manager.RedirectNotification(notification, notificationDetails);
        }

        private void Instance_OutputEvent(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            RedirectLogToOutputWindow(type,msg,msgDetails);
        }

        private void initDebugJsonConfig(JObject json_cfg)
        {
            manager.SetConfig("next_operate", json_cfg["next_operate"].ToString());
            manager.SetConfig("slow_step_speed_ms", json_cfg["slow_step_speed_ms"].ToString());
            manager.SetConfig("is_log_activities", json_cfg["is_log_activities"].ToString());
            manager.SetConfig("activity_id_json_array", json_cfg["activity_id_json_array"].ToString());
            manager.SetConfig("breakpoint_id_json_array", json_cfg["breakpoint_id_json_array"].ToString());
            manager.SetConfig("tracker_vars", json_cfg["tracker_vars"].ToString());
        }

        private void initCommandLineJsonConfig(JObject json_cfg)
        {
            manager.SetConfig("input_args", json_cfg["input"]?.ToString());
        }

        private void RedirectLogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails)
        {
            manager.RedirectLogToOutputWindow(type,msg,msgDetails);
        }

        private void Agent_StopEvent()
        {
            Logger.Debug("The stop command is received, and the program exits……", logger);

            quitTimer.Interval = TimeSpan.FromMilliseconds(300);
            quitTimer.Tick += QuitTimer_Tick;
            quitTimer.Start();
        }

        private void Agent_ExitEvent()
        {
            Logger.Debug("The exit command is received, and the program exits……", logger);

            quitTimer.Interval = TimeSpan.FromMilliseconds(300);
            quitTimer.Tick += QuitTimer_Tick;
            quitTimer.Start();
        }


        private void QuitTimer_Tick(object sender, EventArgs e)
        {
            quitTimer.Tick -= QuitTimer_Tick;

            UiElement.ScreenReaderOff();

            Logger.Debug("Application End", logger);
            Environment.Exit(0);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UiElement.ScreenReaderOff();

#if Chainbot_EXECUTOR_CONSOLE_DEBUG
            FreeConsole();
#endif
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Common.RunInUI(() =>
            {
                try
                {
                    manager.OnUnhandledException(ChainbotExecutor.Properties.Resources.Message2, e.Exception);

                    Logger.Error("UI thread global exception", logger);
                    Logger.Error(e.Exception, logger);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Unrecoverable UI thread global exception", logger);
                    Logger.Fatal(ex, logger);
                }
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Common.RunInUI(() =>
            {
                try
                {
                    var exception = e.ExceptionObject as Exception;
                    if (exception != null)
                    {
                        manager.OnUnhandledException(ChainbotExecutor.Properties.Resources.Message3, exception);

                        Logger.Error("Non-ui thread global exception", logger);
                        Logger.Error(exception, logger);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Unrecoverable non-UI thread global exception", logger);
                    Logger.Fatal(ex, logger);
                }
            });
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Split(',')[0];

            var path = assemblyResolveDllList.Where(item => System.IO.Path.GetFileNameWithoutExtension(item).Equals(name)).FirstOrDefault();

            if (System.IO.File.Exists(path))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    return assembly;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                path = Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, name + ".dll");
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(path);
                        return assembly;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    
                }

                Trace.WriteLine(string.Format("********************{0} assembly could not be found********************", args.Name));
            }

            return null;
        }









    }
}
