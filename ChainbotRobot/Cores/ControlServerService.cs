using ChainbotRobot.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Flurl.Http;
using AesEverywhere;
using log4net;
using ChainbotRobot.ViewModel;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.App;

namespace ChainbotRobot.Cores
{
    public class ControlServerService : IControlServerService
    {
        private IServiceLocator _serviceLocator;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAboutInfoService _aboutInfoService;

        private ILogService _logService;

        private AES256 aes256 = new AES256();

        private UserPreferencesViewModel _userPreferencesViewModel;

        public ControlServerService(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _aboutInfoService = _serviceLocator.ResolveType<IAboutInfoService>();

            _userPreferencesViewModel = _serviceLocator.ResolveType<UserPreferencesViewModel>();

            _logService = _serviceLocator.ResolveType<ILogService>();

            FlurlHttp.Configure(settings =>
            {
                settings.Timeout = TimeSpan.FromSeconds(60);
            });
        }

        private string NewGuid()
        {
            return Guid.NewGuid().ToString();
        }


        public async void Register()
        {
            if (!_userPreferencesViewModel.IsEnableControlServer)
            {
                return;
            }

            try
            {
                string find_dict = _userPreferencesViewModel.ControlServerUri + "/rest/dict/finddict";
                string add_info = _userPreferencesViewModel.ControlServerUri + "/rest/dict/savedict";
                string update_info = _userPreferencesViewModel.ControlServerUri + "/rest/dict/savedict";

                JObject jObj = new JObject();
                jObj["DCT_ID"] = "MACHINE_INFO";
                jObj["MACHINENAME"] = _aboutInfoService.GetMachineId();

                _logService.Debug("Register() The request content for querying the machine message is:" + jObj.ToString(), _logger);

                var result = await find_dict.PostJsonAsync(jObj).ReceiveJson<JObject>();
                if ((int)result["code"] != 1)
                {
                    _logService.Debug("Failed to query machine information!" + jObj.ToString(), _logger);

                    return;
                }

                _logService.Debug("Register() The request result of querying machine information is:" + result.ToString(), _logger);

                if (result["data"]["rowSetArray"] == null)
                {
                    var guid = NewGuid();
                    jObj = new JObject();
                    jObj["DCT_ID"] = "MACHINE_INFO";
                    var jArr = new JArray();

                    JObject jItem = new JObject();
                    jItem["F_GUID"] = guid;
                    jItem["MACHINENAME"] = _aboutInfoService.GetMachineId();
                    jItem["ROBOTVERSION"] = _aboutInfoService.GetVersion();
                    jItem["MACHINEIP"] = _aboutInfoService.GetIp();
                    jArr.Add(jItem);

                    jObj["InsertPool"] = jArr;

                    _logService.Debug("Register() The request content for adding machine information is:" + jObj.ToString(), _logger);

                    result = await add_info.PostJsonAsync(jObj).ReceiveJson<JObject>();
                    if ((int)result["code"] != 1)
                    {
                        _logService.Debug("Failed to add machine information!" + jObj.ToString(), _logger);
                        return;
                    }
                    _logService.Debug("Register() The result of the request to add machine information is:" + result.ToString(), _logger);
                }
                else
                {
                    if (result["data"]["rowSetArray"].Count() == 1)
                    {
                        jObj = new JObject();
                        jObj["DCT_ID"] = "MACHINE_INFO";
                        var jArr = new JArray();

                        JObject jItem = result["data"]["rowSetArray"][0]["dataMap"] as JObject;
                        jItem.Remove("F_CHDATE");
                        jItem["MACHINENAME"] = _aboutInfoService.GetMachineId();
                        jItem["ROBOTVERSION"] = _aboutInfoService.GetVersion();
                        jItem["MACHINEIP"] = _aboutInfoService.GetIp();
                        jArr.Add(jItem);

                        jObj["UpdatePool"] = jArr;

                        _logService.Debug("Register() The request content for updating machine information is:" + jObj.ToString(), _logger);

                        result = await update_info.PostJsonAsync(jObj).ReceiveJson<JObject>();
                        if ((int)result["code"] != 1)
                        {
                            _logService.Debug("Failed to update machine information!" + jObj.ToString(), _logger);
                            return;
                        }

                        _logService.Debug("Register() The result of the request to update machine information is:" + result.ToString(), _logger);
                    }
                    else
                    {
                        _logService.Debug("The number of machine information queried exceeds 1!" + result.ToString(), _logger);
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }

        }

        public async Task<JArray> GetProcesses()
        {
            if (!_userPreferencesViewModel.IsEnableControlServer)
            {
                return null;
            }

            try
            {
                string find_dict = _userPreferencesViewModel.ControlServerUri + "/rest/dict/finddict";
                JObject jObj = new JObject();
                jObj["DCT_ID"] = "PROCESS_ALLOCATION";
                jObj["MACHINENAME"] = _aboutInfoService.GetMachineId();

                _logService.Debug("GetProcesses() The request content for querying process information is:" + jObj.ToString(), _logger);

                var result = await find_dict.PostJsonAsync(jObj).ReceiveJson<JObject>();
                if ((int)result["code"] != 1)
                {
                    _logService.Debug("Failed to query process information!" + jObj.ToString(), _logger);

                    return null;
                }

                _logService.Debug("GetProcesses() The request result of querying process information is:" + result.ToString(), _logger);

                var jRetArray = new JArray();
                if (result["data"]["rowSetArray"] != null)
                {
                    JArray jArr = result["data"]["rowSetArray"] as JArray;
                    for (int i = 0; i < jArr.Count; i++)
                    {
                        jObj = new JObject();
                        var dataMap = jArr[i]["dataMap"];
                        jObj["PROCESSNAME"] = dataMap["PROCESSNAME"];
                        jObj["PROCESSVERSION"] = dataMap["PROCESSVERSION"];

                        string fileName = jObj["PROCESSNAME"].ToString() + "." + jObj["PROCESSVERSION"].ToString() + ".nupkg";
                        jObj["NUPKGFILENAME"] = fileName;

                        string nupkgUrl = $"{_userPreferencesViewModel.ControlServerUri}/rest/affix/dict/download{dataMap["AFFIXURL"].ToString()}";
                        jObj["NUPKGURL"] = nupkgUrl;

                        jRetArray.Add(jObj);
                    }
                }

                return jRetArray;
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }

            return null;
        }

        public async Task<JObject> GetRunProcess()
        {
            if (!_userPreferencesViewModel.IsEnableControlServer)
            {
                return null;
            }

            try
            {
                string find_dict_status = _userPreferencesViewModel.ControlServerUri + "/rest/dict/finddictstatus";
                JObject jObj = new JObject();
                jObj["DCT_ID"] = "PROCESS_ALLOCATION";
                jObj["MACHINENAME"] = _aboutInfoService.GetMachineId();

                _logService.Debug("GetRunProcess() The request content for querying the information when the process status is started is:" + jObj.ToString(), _logger);

                var result = await find_dict_status.PostJsonAsync(jObj).ReceiveJson<JObject>();
                if ((int)result["code"] != 1)
                {
                    _logService.Debug("Failed to query the information when the process status is started!" + jObj.ToString(), _logger);

                    return null;
                }

                _logService.Debug("GetRunProcess() The request result of querying the information when the process status is started is:" + result.ToString(), _logger);

                if (result["data"]["rowSetArray"] != null)
                {
                    JArray jArr = result["data"]["rowSetArray"] as JArray;
                    if (jArr.Count() == 1)
                    {
                        return jArr[0]["dataMap"] as JObject;
                    }
                    else
                    {
                        _logService.Debug("The number of information found when the query process status is started exceeds 1!" + result.ToString(), _logger);
                        return null;
                    }
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }

            return null;
        }


        public async Task UpdateRunStatus(string projectName, string projectVersion, enProcessStatus status)
        {
            if (!_userPreferencesViewModel.IsEnableControlServer)
            {
                return;
            }

            try
            {
                string find_dict = _userPreferencesViewModel.ControlServerUri + "/rest/dict/finddict";
                string update_info = _userPreferencesViewModel.ControlServerUri + "/rest/dict/savedict";

                JObject jObj = new JObject();
                jObj["DCT_ID"] = "PROCESS_ALLOCATION";
                jObj["MACHINENAME"] = _aboutInfoService.GetMachineId();

                _logService.Debug("UpdateRunStatus() The request content for querying all process allocation information corresponding to the local machine is:" + jObj.ToString(), _logger);

                var result = await find_dict.PostJsonAsync(jObj).ReceiveJson<JObject>();
                if ((int)result["code"] != 1)
                {
                    _logService.Debug("Failed to query all process allocation information corresponding to this machine!" + jObj.ToString(), _logger);
                    return;
                }

                _logService.Debug("UpdateRunStatus() The request result of querying all process allocation information corresponding to the local machine is:" + result.ToString(), _logger);

                JObject machedObject = null;
                if (result["data"]["rowSetArray"] != null)
                {
                    var jArr = result["data"]["rowSetArray"] as JArray;
                    if (jArr.Count() > 0)
                    {
                        for (int i = 0; i < jArr.Count(); i++)
                        {
                            var dataMap = jArr[i]["dataMap"] as JObject;

                            if (dataMap["PROCESSNAME"].ToString() == projectName && dataMap["PROCESSVERSION"].ToString() == projectVersion)
                            {
                                machedObject = dataMap;
                            }
                        }
                    }

                }

                if (machedObject != null)
                {
                    jObj = new JObject();
                    jObj["DCT_ID"] = "PROCESS_ALLOCATION";
                    var jArr = new JArray();

                    JObject jItem = machedObject;
                    jItem.Remove("F_CHDATE");
                    jItem["PROCESSSTATUS"] = ((int)status).ToString();
                    jArr.Add(jItem);

                    jObj["UpdatePool"] = jArr;

                    _logService.Debug("UpdateRunStatus() The request content for updating process status information is:" + jObj.ToString(), _logger);

                    result = await update_info.PostJsonAsync(jObj).ReceiveJson<JObject>();
                    if ((int)result["code"] != 1)
                    {
                        _logService.Debug("Failed to update process status information!" + jObj.ToString(), _logger);
                        return;
                    }

                    _logService.Debug("UpdateRunStatus() The request result of updating process status information is:" + result.ToString(), _logger);
                }
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
            }
        }


        public async Task Log(string projectName, string projectVersion, string level, string msg)
        {
            if (!_userPreferencesViewModel.IsEnableControlServer)
            {
                return;
            }

            string add_info = _userPreferencesViewModel.ControlServerUri + "/rest/dict/savedict";

            JObject jObj = new JObject();
            jObj["DCT_ID"] = "LOG_INFO";
            var jArr = new JArray();

            JObject jItem = new JObject();
            jItem["F_GUID"] = NewGuid();
            jItem["MACHINENAME"] = _aboutInfoService.GetMachineId();

            jItem["PROCESSNAME"] = projectName;
            jItem["PROCESSVERSION"] = projectVersion;

            jItem["LOGLEVEL"] = level;
            jItem["LOGCONTENT"] = aes256.Encrypt(msg, "ABCDE");

            jArr.Add(jItem);

            jObj["InsertPool"] = jArr;

            _logService.Debug("Log() The content of the request for adding log information is:" + jObj.ToString(), _logger);

            var result = await add_info.PostJsonAsync(jObj).ReceiveJson<JObject>();
            if ((int)result["code"] != 1)
            {
                _logService.Debug("Failed to add log information!" + jObj.ToString(), _logger);
                return;
            }

            _logService.Debug("Log() The request result for adding log information is:" + result.ToString(), _logger);
        }


    }
}
