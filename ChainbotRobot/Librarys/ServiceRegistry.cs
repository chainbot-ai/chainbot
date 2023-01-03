using Chainbot.Contracts.App;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Log;
using Chainbot.Cores.App;
using Chainbot.Cores.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Librarys
{
    public abstract class ServiceRegistry
    {
        protected IServiceLocator _serviceLocator { get; }

        public ServiceRegistry()
        {
            _serviceLocator = ServiceLocator.New();
        }

        public void RegisterServices()
        {
            _serviceLocator.RegisterTypeSingleton<IConsoleManager, ConsoleManager>();

            _serviceLocator.RegisterTypeSingleton<Log4NetService>();
            _serviceLocator.RegisterTypeSingleton<ConsolePrintService>();
            _serviceLocator.RegisterTypeSingleton<DiagnosticsDebugPrintService>();

#if DEBUG
            _serviceLocator.RegisterTypeSingleton<ILogService, DebugLogService>();
#else
            _serviceLocator.RegisterTypeSingleton<ILogService, Log4NetService>();
#endif
            OnRegisterServices();
            OnRegisterViews();
            OnRegisterViewModels();
        }
        public abstract void OnRegisterServices();

        public abstract void OnRegisterViewModels();

        public abstract void OnRegisterViews();

        public T ResolveType<T>()
        {
            return _serviceLocator.ResolveType<T>();
        }

        public T TryResolveType<T>()
        {
            return _serviceLocator.TryResolveType<T>();
        }
    }

}