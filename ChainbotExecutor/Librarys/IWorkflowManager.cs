using Plugins.Shared.Library;
using System;

namespace ChainbotExecutor.Librarys
{
    public interface IWorkflowManager
    {
        void Run();
        void Stop();

        void OnUnhandledException(string title, Exception err);

        
        string GetConfig(string key);
        void SetConfig(string key, string val);

        void RedirectLogToOutputWindow(SharedObject.enOutputType type, string msg, string msgDetails);
        void RedirectNotification(string notification, string notificationDetails);
    }
}