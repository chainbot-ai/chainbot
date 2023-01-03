using Plugins.Shared.Library;
using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Chainbot.Contracts.Classes.GlobalConfig;

namespace Chainbot.Cores.Config
{
    public class AppSettingsConfigService : MarshalByRefServiceBase, IAppSettingsConfigService
    {
        private IPathConfigService _pathConfigService;
        private IConstantConfigService _constantConfigService;
        private enTheme? _currentTheme;
        private enLanguage? _currentLanguage;

        public AppSettingsConfigService(IPathConfigService pathConfigService, IConstantConfigService constantConfigService)
        {
            _pathConfigService = pathConfigService;
            _constantConfigService = constantConfigService;
        }

        public string GetLastExportDir()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                var path = _pathConfigService.AppSettingsXml;
                doc.Load(path);
                var rootNode = doc.DocumentElement;
                var exportNupkgElement = rootNode.SelectSingleNode("ExportNupkg") as XmlElement;
                var lastExportDirElement = exportNupkgElement.SelectSingleNode("LastExportDir") as XmlElement;
                return lastExportDirElement.InnerText;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public bool SetLastExportDir(string dir)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                var path = _pathConfigService.AppSettingsXml;
                doc.Load(path);
                var rootNode = doc.DocumentElement;
                var exportNupkgElement = rootNode.SelectSingleNode("ExportNupkg") as XmlElement;
                var lastExportDirElement = exportNupkgElement.SelectSingleNode("LastExportDir") as XmlElement;
                lastExportDirElement.InnerText = dir;
                doc.Save(path);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool AddToExportDirHistoryList(string dir)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                var path = _pathConfigService.AppSettingsXml;
                doc.Load(path);
                var rootNode = doc.DocumentElement;

                var exportNupkgElement = rootNode.SelectSingleNode("ExportNupkg") as XmlElement;
                var exportDirHistoryList = exportNupkgElement.SelectSingleNode("ExportDirHistoryList") as XmlElement;
                var items = exportDirHistoryList.SelectNodes("Item");

                if (items.Count > _constantConfigService.ExportDirHistoryMaxRecordCount)
                {
                    exportDirHistoryList.RemoveChild(exportDirHistoryList.LastChild);
                }

                foreach (XmlElement item in items)
                {
                    if (dir.ToLower() == item.InnerText.ToLower())
                    {
                        exportDirHistoryList.RemoveChild(item);
                        break;
                    }
                }

                XmlElement itemElement = doc.CreateElement("Item");
                itemElement.InnerText = dir;

                exportDirHistoryList.PrependChild(itemElement);

                doc.Save(path);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        public List<string> GetExportDirHistoryList()
        {
            try
            {
                var ret = new List<string>();

                XmlDocument doc = new XmlDocument();
                var path = _pathConfigService.AppSettingsXml;
                doc.Load(path);
                var rootNode = doc.DocumentElement;
                var exportNupkgElement = rootNode.SelectSingleNode("ExportNupkg") as XmlElement;

                var exportDirHistoryList = exportNupkgElement.SelectSingleNode("ExportDirHistoryList") as XmlElement;
                var items = exportDirHistoryList.SelectNodes("Item");
                foreach (XmlElement item in items)
                {
                    ret.Add(item.InnerText);
                }

                return ret;
            }
            catch (Exception)
            {
                return null;
            }          
        }


        public enTheme? CurrentTheme
        {
            get
            {
                if(_currentTheme == null)
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        var path = _pathConfigService.AppSettingsXml;
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;

                        var themeElement = rootNode.SelectSingleNode("Theme") as XmlElement;
                        if(themeElement == null)
                        {
                            return null;
                        }

                        var currentThemeElement = themeElement.SelectSingleNode("CurrentTheme") as XmlElement;
                        _currentTheme = (enTheme)Enum.Parse(typeof(enTheme), currentThemeElement.InnerText, true);

                        return _currentTheme;
                    }
                    catch (Exception err)
                    {
                        SharedObject.Instance.Output(SharedObject.enOutputType.Error, "主题样式获取失败！", err);
                        return null;
                    }
                }
                else
                {
                    return _currentTheme;
                }
            }

            set
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    var path = _pathConfigService.AppSettingsXml;
                    doc.Load(path);
                    var rootNode = doc.DocumentElement;

                    var themeElement = rootNode.SelectSingleNode("Theme") as XmlElement;
                    XmlNode currentThemeElement;
                    if (themeElement != null)
                    {
                        currentThemeElement = themeElement.SelectSingleNode("CurrentTheme") as XmlElement;
                        currentThemeElement.InnerText = value.ToString();
                    }
                    else
                    {
                        themeElement = doc.CreateElement("Theme");
                        currentThemeElement = doc.CreateElement("CurrentTheme");
                        currentThemeElement.InnerText = value.ToString();
                        themeElement.AppendChild(currentThemeElement);
                        rootNode.AppendChild(themeElement);
                    }

                    doc.Save(path);
                    _currentTheme = value;
                }
                catch (Exception err)
                {
                    SharedObject.Instance.Output(SharedObject.enOutputType.Error, "主题样式设定失败！", err);
                }
            }
        }

        public enLanguage? CurrentLanguage
        {
            get
            {
                if (_currentLanguage == null)
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        var path = _pathConfigService.AppSettingsXml;
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;

                        var languageElement = rootNode.SelectSingleNode("Language") as XmlElement;
                        if (languageElement == null)
                        {
                            return null;
                        }

                        var currentLanguageElement = languageElement.SelectSingleNode("CurrentLanguage") as XmlElement;
                        _currentLanguage = (enLanguage)Enum.Parse(typeof(enLanguage), currentLanguageElement.InnerText, true);

                        return _currentLanguage;
                    }
                    catch (Exception err)
                    {
                        SharedObject.Instance.Output(SharedObject.enOutputType.Error, "默认语言获取失败！", err);
                        return null;
                    }
                }
                else
                {
                    return _currentLanguage;
                }
            }

            set
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    var path = _pathConfigService.AppSettingsXml;
                    doc.Load(path);
                    var rootNode = doc.DocumentElement;

                    var languageElement = rootNode.SelectSingleNode("Language") as XmlElement;
                    XmlNode currentLanguageElement;
                    if (languageElement != null)
                    {
                        currentLanguageElement = languageElement.SelectSingleNode("CurrentLanguage") as XmlElement;
                        currentLanguageElement.InnerText = value.ToString();
                    }
                    else
                    {
                        languageElement = doc.CreateElement("Language");
                        currentLanguageElement = doc.CreateElement("CurrentLanguage");
                        currentLanguageElement.InnerText = value.ToString();
                        languageElement.AppendChild(currentLanguageElement);
                        rootNode.AppendChild(languageElement);
                    }

                    doc.Save(path);
                    _currentLanguage = value;
                }
                catch (Exception err)
                {
                    SharedObject.Instance.Output(SharedObject.enOutputType.Error, "默认语言设定失败！", err);
                }
            }
        }

    }
}
