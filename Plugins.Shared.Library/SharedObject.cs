using Chainbot.Contracts.Classes;
using System;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Plugins.Shared.Library
{
    public class SharedObject : MarshalByRefServiceBase
    {
        public enum enOutputType
        {
            Trace,
            Information,
            Warning,
            Error,
        }

        public enum enJobType
        {
            TRACK_IN, 
            TRACK_OUT,
            SERVICE, 
        }



        public delegate void NotifyDelegate(string notification, string notificationDetails = "");

        public event NotifyDelegate NotifyEvent;



        public void Notify(string notification, string notificationDetails = "")
        {
            NotifyEvent?.Invoke(notification, notificationDetails);
        }



        public delegate void S2CNotifyDelegate(string notification, string notificationDetails = "");

        public event S2CNotifyDelegate S2CNotifyEvent;



        public void S2CNotify(string notification, string notificationDetails = "")
        {
            S2CNotifyEvent?.Invoke(notification, notificationDetails);
        }

        public delegate void OutputDelegate(enOutputType type, string msg, string msgDetails = "");

        public event OutputDelegate OutputEvent;

        public void ClearOutputEvent()
        {
            OutputEvent = null;
        }
       
        public void ClearNotifyEvent()
        {
            NotifyEvent = null;
        }

        public void Output(enOutputType type, object msg, object msgDetails = null)
        {
            var msgStr = msg == null ? "" : msg.ToString();
            var msgDetailsStr = msgDetails == null ? msgStr : msgDetails.ToString();
            OutputEvent?.Invoke(type, msgStr, msgDetailsStr);
        }

        public void OutputDirect(enOutputType type, object msg, object msgDetails = null)
        {
            var msgStr = msg == null ? "" : msg.ToString();
            var msgDetailsStr = msgDetails == null ? msgStr : msgDetails.ToString();
            OutputEvent?.Invoke(type, msgStr, msgDetailsStr);
        }


        public string ProjectPath { get; set; }

        public bool IsBeginDebugChildWorkflow { get; set; }

        public string CurrentDebugWorkflowFilePath { get; set; }

        public string CurrentDebugLocationId { get; set; }

        public string ApplicationCurrentDirectory
        {
            get
            {
                var exeFullPath = this.GetType().Assembly.Location;//AppDomain.CurrentDomain.BaseDirectory;
                var exeDir = System.IO.Path.GetDirectoryName(exeFullPath);
                return exeDir;
            }
        }


        public void ShowMainWindowMinimized()
        {
            Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        public void ShowMainWindowNormal()
        {
            Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;

            Application.Current.MainWindow.Activate();
        }

        public void ActivateMainWindow()
        {
            Application.Current.MainWindow.Activate();
        }


        public bool isHighlightElements { get; set; }


        public JObject JsonParams { get; set; } = new JObject();


        private static SharedObject _instance = null;
        public static SharedObject Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new SharedObject();
                }

                return _instance;
            }

            private set
            {
                _instance = value;
            }
        }


        public static void SetCrossDomainInstance(SharedObject instance)
        {
            Instance = instance;
        }

        public object VisualTrackingParticipant { get; set; }
    }
}
