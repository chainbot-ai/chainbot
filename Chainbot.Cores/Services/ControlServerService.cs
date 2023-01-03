using Chainbot.Contracts.Config;
using Chainbot.Contracts.Log;
using Chainbot.Contracts.Services;
using Flurl.Http;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Plugins.Shared.Library;
using Flurl;
using Chainbot.Contracts.Utils;
using Plugins.Shared.Library.Librarys;
using System.Activities.XamlIntegration;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Annotations;

namespace Chainbot.Cores.Services
{
    public class ControlServerService : IControlServerService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;
        private IServerSettingsService _serverSettingsService;

        public ControlServerService(ILogService logService, IServerSettingsService serverSettingsService)
        {
            _logService = logService;
            _serverSettingsService = serverSettingsService;
        }

        private string NewGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<bool> Publish(string projectName, string publishVersion, string publishDescription, string nupkgFilePath)
        {
            try
            {
                var controlServerUrl = _serverSettingsService.ControlServerUrl;

                string add_process_info = controlServerUrl + "/rest/dict/savedict";
                string upload_affix = controlServerUrl + "/rest/affix/dict/save";

                var guid = NewGuid();
                JObject jObj = new JObject();
                jObj["DCT_ID"] = "PROCESS_INFO";
                var jArr = new JArray();

                JObject jItem = new JObject();
                jItem["F_GUID"] = guid;
                jItem["PROCESSNAME"] = projectName;
                jItem["PROCESSVERSION"] = publishVersion;
                jItem["PROCESSDESCRIBE"] = publishDescription;
                jArr.Add(jItem);

                jObj["InsertPool"] = jArr;

                var result = await add_process_info.PostJsonAsync(jObj).ReceiveJson<JObject>();
                if ((int)result["code"] != 1)
                {
                    _logService.Debug("增加流程信息失败！" + result.ToString(), _logger);
                    return false;
                }

                //上传该流程对应的附件
                var fileName = System.IO.Path.GetFileName(nupkgFilePath);

                jObj = new JObject();
                jObj["F_GUID"] = guid;
                jObj["F_CCLX"] = _serverSettingsService.CCLX;
                jObj["MDLID"] = "PROCESS_ALLOCATION";
                jObj["YWLX"] = "";
                jObj["F_NAME"] = fileName;
                jObj["F_PATH"] = "";
                jObj["EXT_STR01"] = "";
                jObj["EXT_STR02"] = "";
                jObj["EXT_STR03"] = "";
                jObj["EXT_STR04"] = "";

                result = await upload_affix
                .WithTimeout(300)
                .PostMultipartAsync(mp => mp
                .AddFile("FILE", nupkgFilePath)
                .AddJson("AFFIXMSG", jObj)).ReceiveJson<JObject>();

                if ((int)result["code"] != 0)
                {
                    _logService.Debug("AFFIXMSG=" + jObj.ToString(), _logger);
                    _logService.Debug("上传文件出错！" + result.ToString(), _logger);
                    return false;
                }

                return true;
            }
            catch (Exception err)
            {
                _logService.Debug(err, _logger);
                return false;
            }
        }

       
    }
}
