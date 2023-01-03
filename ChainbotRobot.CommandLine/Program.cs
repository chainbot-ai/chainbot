using CommandLine;
using log4net;
using log4net.Appender;
using Chainbot.Contracts.Log;
using ChainbotRobot.CommandLine.Contracts;
using ChainbotRobot.CommandLine.Librarys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine
{
    //Sample: -f "C:\Project\Main.xaml" -i "{'inArg':'value','integer':3}"  -l "log.txt"

    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ILogService _logService;

        private static IRegisterService _registerService;

        private static IProjectService _projectService;

        private static IGlobalService _globalService;

        private static string[] Args;

        private static ServiceRegistry _serviceRegistry = new UserServiceRegistry();

        public static int Current_DispatcherUnhandledException { get; private set; }

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            Args = args;

            _serviceRegistry.RegisterServices();

            _logService = _serviceRegistry.ResolveType<ILogService>();
            _registerService = _serviceRegistry.ResolveType<IRegisterService>();
            _projectService = _serviceRegistry.ResolveType<IProjectService>();
            _globalService = _serviceRegistry.ResolveType<IGlobalService>();

            return Parser.Default.ParseArguments<Options>(args)
              .MapResult(
                options => RunAndReturnExitCode(options),
                _ => 1);
        }

       
        public static int Run(Options options)
        {
            var prevWorkDirrectory = System.IO.Directory.GetCurrentDirectory();
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            var ret = RunAndReturnExitCode(options,false);

            System.IO.Directory.SetCurrentDirectory(prevWorkDirrectory);

            return ret;
        }

        public static void Stop()
        {
            _projectService.Stop();
        }

        static void changeLogPath(string logPath)
        {
            _logService.Debug($"日志文件切换为：{logPath}", _logger);

            var repository = LogManager.GetRepository();
            var appenders = repository.GetAppenders();
            var targetApder = appenders.First(p => p.Name == "RollingLogFileAppender") as RollingFileAppender;
            targetApder.File = logPath;
            targetApder.RollingStyle = RollingFileAppender.RollingMode.Size;
            targetApder.ActivateOptions();
        }

        static int RunAndReturnExitCode(Options options,bool runInExe = true)
        {
            if(runInExe)
            {
                _logService.Debug($"+++++++++++++++++命令行方式运行 ChainbotRobot.CommandLine.exe+++++++++++++++++ 命令行参数为：{String.Join(" ", Args)}", _logger);
            }
            else
            {
                string optionsJson = Newtonsoft.Json.JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented);
                _logService.Debug($"+++++++++++++++++t程序集方式运行 ChainbotRobot.CommandLine+++++++++++++++++ 参数为：{optionsJson}", _logger);
            }

            _logService.Debug($"当前工作目录为：{System.IO.Directory.GetCurrentDirectory()}", _logger);

            if (!string.IsNullOrEmpty(options.Log))
            {
                changeLogPath(options.Log);
            }

            _globalService.Options = options;
            _projectService = _serviceRegistry.ResolveType<IProjectService>();

            var projectDir = Path.GetDirectoryName(options.File);
            System.IO.Directory.SetCurrentDirectory(projectDir);

            _projectService.Init(options.File);
            _projectService.Start();
            _globalService.AutoResetEvent.WaitOne();

            return 0;
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    _logService.Error("非UI线程全局异常", _logger);
                    _logService.Error(exception, _logger);
                }
            }
            catch (Exception ex)
            {
                _logService.Fatal("不可恢复的非UI线程全局异常", _logger);
                _logService.Fatal(ex, _logger);
            }
        }



    }
}
