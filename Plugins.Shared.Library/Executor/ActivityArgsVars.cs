using log4net;
using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.Activities.Tracking;
using System.Collections.Generic;

namespace Plugins.Shared.Library.Executor
{
    public class ActivityArgsVars
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string PythonHome;

        private string TranslateValue(object value)
        {
            var str = "";
            try
            {
                if (value is PyObject)
                {
                    str = (value as PyObject).ToString();
                }
                else if (value is IEnumerable<PyObject>)
                {
                    var vals = value as IEnumerable<PyObject>;
                    var list = new List<string>();
                    foreach (var val in vals)
                    {
                        var item = (val as PyObject).ToString();
                        list.Add(item);
                    }
                    str = "[" + string.Join(",", list) + "]";
                }
                else if (value == null)
                {
                    str = "<null>";
                }
                else
                {
                    str = JsonConvert.SerializeObject(value);
                }

            }
            catch (Exception)
            {
                str = "无法解析";
            }

            return str;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        private void InitPython()
        {
            if (!PythonEngine.IsInitialized)
            {
                //setx /M 
                var rpaPythonPath = (Environment.GetEnvironmentVariable("CHAINBOT_PYTHON_PATH", EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable("CHAINBOT_PYTHON_PATH", EnvironmentVariableTarget.Machine));

                if (System.IO.Directory.Exists(rpaPythonPath))
                {
                    PythonHome = rpaPythonPath;
                }
                else
                {
                    PythonHome = System.IO.Path.Combine(SharedObject.Instance.ApplicationCurrentDirectory, @"Python");
                }

                //SharedObject.Instance.Output(SharedObject.enOutputType.Trace, $"PythonHome=“{PythonHome}”");

                Environment.SetEnvironmentVariable("PATH", PythonHome + ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine), EnvironmentVariableTarget.Process);

                try
                {
                    PythonEngine.PythonHome = PythonHome;
                    PythonEngine.Initialize();
                }
                catch (Exception err)
                {
                    PythonEngine.Shutdown();
                    _logger.Debug($"初始化Python失败({err})");
                    throw new Exception("Initialization failure");
                }
            }
        }

        public ActivityArgsVars(ActivityStateRecord activityStateRecord)
        {
            IntPtr ts = IntPtr.Zero;

            try
            {
                InitPython();

                ts = PythonEngine.BeginAllowThreads();
                using (Py.GIL())
                {
                    using (var ps = Py.CreateScope())
                    {
                        foreach (var item in activityStateRecord.Variables)
                        {
                            Variables[item.Key] = TranslateValue(item.Value);
                        }

                        foreach (var item in activityStateRecord.Arguments)
                        {
                            Arguments[item.Key] = TranslateValue(item.Value);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                if (err.Message == "Initialization failure")
                {
                    foreach (var item in activityStateRecord.Variables)
                    {
                        Variables[item.Key] = TranslateValue(item.Value);
                    }

                    foreach (var item in activityStateRecord.Arguments)
                    {
                        Arguments[item.Key] = TranslateValue(item.Value);
                    }
                }
            }
            finally
            {
                if (ts != IntPtr.Zero)
                {
                    PythonEngine.EndAllowThreads(ts);
                }
            }
        }

        public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}