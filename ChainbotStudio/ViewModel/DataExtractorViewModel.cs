using Chainbot.ChromePlugin;
using Chainbot.FirefoxPlugin;
using ChainbotStudio.Views;
using Chainbot.Contracts.Activities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Windows;
using System.Xml;
using Chainbot.Contracts.UI;
using Chainbot.IEPlugin;
using System.Text;
using System.Runtime.Serialization.Json;
using Chainbot.Sap;

namespace ChainbotStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DataExtractorViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public DataExtractorViewModel(IDataExtractorServiceProxy dataExtractorServiceProxy,
            DocksViewModel docksViewModel,
            IMessageBoxService messageBoxService)
        {
            _dataExtractorServiceProxy = dataExtractorServiceProxy;
            _docksViewModel = docksViewModel;
            _messageBoxService = messageBoxService;
        }

        private DataExtractorWindow m_view;

        private RelayCommand<RoutedEventArgs> _loadedCommand;

        private IDataExtractorServiceProxy _dataExtractorServiceProxy;

        private DocksViewModel _docksViewModel;

        private IMessageBoxService _messageBoxService;

        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        ColumnsArr = new JArray();
                        m_view = (DataExtractorWindow)p.Source;
                    }));
            }
        }

        private RelayCommand _ExtractorHelpCommand;
        public RelayCommand ExtractorHelpCommand
        {
            get
            {
                return _ExtractorHelpCommand
                    ?? (_ExtractorHelpCommand = new RelayCommand(
                    () =>
                    {
                        
                    }));
            }
        }

        private RelayCommand _ExtractorReturnCommand;
        public RelayCommand ExtractorReturnCommand
        {
            get
            {
                return _ExtractorReturnCommand
                    ?? (_ExtractorReturnCommand = new RelayCommand(
                    () =>
                    {
                        if (currentPageName == "page1")
                        {
                            m_view.ClearDataTable();
                            m_view.SetReturnButtonStauts(false);
                            m_view.SetDataExtractorButtonStatus(false);
                        }

                        if (currentPageName == "page2")
                        {
                            m_view.ChangePageOne();
                            currentPageName = "page1";
                            m_view.ClearDataTable();
                            m_view.SetReturnButtonStauts(false);
                            m_view.SetDataExtractorButtonStatus(false);
                        }

                        if (currentPageName == "page3")
                        {
                            m_view.ChangePageTwo();
                            currentPageName = "page2";
                            m_view.ClearDataTable();
                            m_view.SetDataExtractorButtonStatus(false);
                        }

                        if (currentPageName == "page4")
                        {
                            m_view.ChangePageThree();
                            currentPageName = "page3";
                            m_view.ClearDataTable();
                            m_view.SetDataExtractorButtonStatus(false);
                        }
                    }));
            }
        }

        private RelayCommand _ExtractorMoreCommand;


        public RelayCommand ExtractorMoreCommand
        {
            get
            {
                return _ExtractorMoreCommand
                    ?? (_ExtractorMoreCommand = new RelayCommand(
                    () =>
                    {
                        currentPageName = "page1";
                        bShowUrl = false;
                        m_view.InitWindow();
                        m_view.SetDataExtractorButtonStatus(false);
                        ExcuceSelector();
                    }));

            }
        }

        private RelayCommand _ExtractorNextCommand;

        public RelayCommand ExtractorNextCommand
        {
            get
            {
                return _ExtractorNextCommand
                    ?? (_ExtractorNextCommand = new RelayCommand(
                    () =>
                    {
                        ExcuceSelector(); 
                    }));
            }
        }

        private void ExcuceSelector()
        {
            if (currentPageName == "page1" ||
                           currentPageName == "page2")
            {
                m_view.WindowState = WindowState.Minimized;
                UiElement.OnSelected = UiElement_OnSelected;
                UiElement.StartElementHighlight();
            }

            if (currentPageName == "page3")
            {
                if (pluginType == "ChromeNode")
                {
                    if (chromePlugin != null)
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(pattern);
                        string rootCustomId = jo.Value<string>("rootCustomId");
                        JArray sel = jo.Value<JArray>("itemSelector");
                        JObject jobject2 = new JObject();
                        jobject2["selecors"] = sel;
                        JArray arr = new JArray();
                        if (m_view.GetTitleCheckStatus())
                            arr.Add("text");
                        if (m_view.GetUrlCheckStatus())
                            arr.Add("url");
                        jobject2["props"] = arr;
                        JArray arr1 = new JArray();
                        arr1.Add(jobject2);
                        ColumnsArr.Add(jobject2);
                        string context = chromePlugin.OnExtractDataByPattern(hWnd, rootCustomId, false, arr1);
                        m_view.InitPageFourData(context, true);
                        m_view.SetDataExtractorButtonStatus(true);
                        m_view.btnNext.Content = Chainbot.Resources.Properties.Resources.DataExtractor_FinishButton;
                        m_view.ChangePageFour();
                        currentPageName = "page4";
                        return;
                    }
                }
                if (pluginType == "FirefoxNode")
                {
                    if (firefoxPlugin != null)
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(pattern);
                        string rootCustomId = jo.Value<string>("rootCustomId");
                        JArray sel = jo.Value<JArray>("itemSelector");
                        JObject jobject2 = new JObject();
                        jobject2["selecors"] = sel;
                        JArray arr = new JArray();
                        arr.Add("text");
                        if (m_view.GetUrlCheckStatus())
                            arr.Add("url");
                        jobject2["props"] = arr;
                        JArray arr1 = new JArray();
                        arr1.Add(jobject2);
                        ColumnsArr.Add(jobject2);
                        string context = firefoxPlugin.OnExtractDataByPattern(hWnd, rootCustomId, false, arr1);
                        m_view.InitPageFourData(context, true);
                        m_view.SetDataExtractorButtonStatus(true);
                        m_view.btnNext.Content = Chainbot.Resources.Properties.Resources.DataExtractor_FinishButton;
                        m_view.ChangePageFour();
                        currentPageName = "page4";
                        return;
                    }
                }
                if (pluginType == "IENode")
                {
                    JObject obj = JObject.Parse(pattern);
                    string rootCustomId = JsonConvert.SerializeObject((JObject)obj["Selector"]);
                    JArray sel = obj.Value<JArray>("itemSelector");
                    JObject jobject2 = new JObject();
                    jobject2["selecors"] = sel;
                    JArray arr = new JArray();
                    arr.Add("text");
                    if (m_view.GetUrlCheckStatus())
                        arr.Add("url");
                    jobject2["props"] = arr;
                    JArray arr1 = new JArray();
                    arr1.Add(jobject2);
                    ColumnsArr.Add(jobject2);
                    string context = IEServiceWrapper.Instance.OnExtractDataByPattern(hWnd, rootCustomId, 0, arr1);
                    JObject obj1 = JObject.Parse(context);
                    int code = obj1.Value<int>("retCode");
                    if (code == 1)
                    {
                        _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage3);
                    }
                    else if (code == 0)
                    {
                        m_view.InitPageFourData(context, true);
                        m_view.SetDataExtractorButtonStatus(true);
                        m_view.btnNext.Content = Chainbot.Resources.Properties.Resources.DataExtractor_FinishButton;
                        m_view.ChangePageFour();
                        currentPageName = "page4";
                    }
                    return;
                }
                //if (pluginType == "SAPNode")
                //{
                //    JObject jo = (JObject)JsonConvert.DeserializeObject(pattern);
                //    string rootCustomId = jo.Value<string>("rootCustomId");
                //    //JArray sel = jo.Value<JArray>("itemSelector");
                //    JArray sel = new JArray();
                //    JObject jobject2 = new JObject();
                //    jobject2["selecors"] = sel;
                //    JArray arr = new JArray();
                //    if (m_view.GetTitleCheckStatus())
                //        arr.Add("text");
                //    if (m_view.GetUrlCheckStatus())
                //        arr.Add("url");
                //    jobject2["props"] = arr;
                //    JArray arr1 = new JArray();
                //    arr1.Add(jobject2);
                //    ColumnsArr.Add(jobject2);
                //    string context = Sap.Instance.OnExtractDataByPattern(hWnd, rootCustomId, false, null);
                //    m_view.InitPageFourData(context, true);
                //    m_view.SetDataExtractorButtonStatus(true);
                //    m_view.btnNext.Content = "Complete";
                //    m_view.ChangePageFour();
                //    currentPageName = "page4";
                //    return;
                //}
            }

            if (currentPageName == "page4")
            {
                m_view.Hide();
                bool? ret = _messageBoxService.ShowQuestionYesNoCancel(Chainbot.Resources.Properties.Resources.DataExtractor_QuestionMessage2, true);
                if (ret == true)
                {
                    UiElement.OnSelected = UiElement_OnSelected;
                    UiElement.StartElementHighlight();
                }
                else
                {
                    DataExtractorFinish();
                }     
            }
        }
        private XmlElement mountChildNodeBySel(XmlDocument xmlDoc, XmlElement xmlNode, string sel)
        {
            XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(sel), new XmlDictionaryReaderQuotas());
            xmlDoc.Load(reader);
            return xmlDoc.DocumentElement;

            //var json = System.Net.WebUtility.HtmlDecode(sel);
            //XmlDocument doc_child = (XmlDocument)JsonConvert.DeserializeXmlNode(json, xmlNode.Name);

            //var tempNode = xmlDoc.ImportNode(doc_child.DocumentElement, true) as XmlElement;

            //foreach (XmlAttribute item in xmlNode.Attributes)
            //{
            //    if (item.Name != "Sel")
            //    {
            //        tempNode.SetAttribute(item.Name, item.Value);
            //    }
            //}

            //return tempNode;
        }
        private void DataExtractorFinish(string pageNextSelector = "")
        {
            XmlDocument xmlDoc = new XmlDocument();
            var itemName = pluginType;
            var itemElement = xmlDoc.CreateElement(itemName);
            string itemElementBuff = "";
            string metaDataElementBuff = "";

            {
                XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(selector), new XmlDictionaryReaderQuotas());
                xmlDoc.Load(reader);
                itemElement = xmlDoc.DocumentElement;
                itemElementBuff = itemElement.OuterXml;
                itemElementBuff = UiElement.ReplaceXmlRootName(itemElementBuff, itemName);
                //itemElementBuff = itemElementBuff.Replace("\"", "\'");
            }
            

            XmlDocument xmlMetadataDoc = new XmlDocument();
            var metaDataName = pluginType;
            var metaDataElement = xmlMetadataDoc.CreateElement(metaDataName);

            JObject obj = new JObject();
            String ss = ColumnsArr.ToString();
            ss = ss.Replace(",\r\n      null", "");
            ColumnsArr = JArray.Parse(ss);
            obj["Columns"] = ColumnsArr;

            string output = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);

            {
                XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(output), new XmlDictionaryReaderQuotas());
                xmlDoc.Load(reader);
                metaDataElement = xmlDoc.DocumentElement;
                metaDataElementBuff = metaDataElement.OuterXml;
                metaDataElementBuff = UiElement.ReplaceXmlRootName(metaDataElementBuff, itemName);
                //metaDataElementBuff = metaDataElementBuff.Replace("\"", "\'");
            }
            
            string pageElements = "";
            if (!string.IsNullOrEmpty(pageNextSelector))
            {
                XmlDocument xmlPageNextSelector = new XmlDocument();
                var pageNextSelectorElement = xmlPageNextSelector.CreateElement(pluginType);


                {
                    XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(pageNextSelector), new XmlDictionaryReaderQuotas());
                    xmlDoc.Load(reader);
                    pageNextSelectorElement = xmlDoc.DocumentElement;
                pageElements = pageNextSelectorElement.OuterXml;
                    pageElements = UiElement.ReplaceXmlRootName(pageElements, itemName);
                    //pageElements = metaDataElementBuff.Replace("\"", "\'");
                }
                
            }

            if (_docksViewModel.SelectedDocument is DesignerDocumentViewModel)
            {
                _dataExtractorServiceProxy.Save(_docksViewModel.SelectedDocument.Path, itemElementBuff, metaDataElementBuff, pageElements);
            }
        }
        private void UiElement_OnSelected(UiElement uiElement)
        {
            if(uiElement.ControlType == "IENode")
            {
                try
                {
                    pluginType = "IENode";
                    hWnd = uiElement.WindowHandle;
                    UiCommon.EnumChildWindows(hWnd.ToInt32(), delegate (int hWndChild, int lParamChild)
                    {
                        StringBuilder sb = new StringBuilder(256);
                        UiCommon.GetClassName((IntPtr)hWndChild, sb, sb.Capacity);
                        if (sb.ToString() == "Internet Explorer_Server")
                        {
                            hWnd = (IntPtr)hWndChild;
                            return false;
                        }
                        return true;
                    }, 0);

                    if (currentPageName == "page1")
                    {
                        bool code = IEServiceWrapper.Instance.GetHtmlTableInfo(hWnd, uiElement.Sel);
                        if (!code)
                        {
                            currentPageName = "page2";
                            pageOneSel = uiElement.Sel;
                            m_view.ChangePageTwo();
                        }
                        else
                        {
                            selector = uiElement.Sel;

                            bool? ret = _messageBoxService.ShowQuestionYesNoCancel(Chainbot.Resources.Properties.Resources.DataExtractor_QuestionMessage1, true);
                            if (ret == true)
                            {
                                string content = IEServiceWrapper.Instance.OnExtractDataByPattern(hWnd, uiElement.Sel,1, null);
                                JObject jo1 = (JObject)JsonConvert.DeserializeObject(content);
                                int code1 = jo1.Value<int>("retCode");
                                if (code1 == 1)
                                {
                                    _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage3);
                                }
                                else if(code1 == 0)
                                {
                                    m_view.InitPageFourData(content, false);
                                    m_view.ChangePageFour();
                                    currentPageName = "page4";
                                } 
                            }
                            else
                            {
                                currentPageName = "page2";
                                pageOneSel = uiElement.Sel;
                                m_view.ChangePageTwo();
                            } 
                        }
                        m_view.SetReturnButtonStauts(true);
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page2")
                    {
                        if(IEServiceWrapper.Instance.CompareHtmlElement(hWnd, uiElement.Sel, pageOneSel))
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage1);
                            return;
                        }

                        string nodeName = IEServiceWrapper.Instance.GetElementTagName(hWnd,pageOneSel);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(nodeName);
                        if (jo.Value<string>("name") == "A")
                        {
                            bShowUrl = true;
                        }

                        pattern = IEServiceWrapper.Instance.OnFindExtractPattern(hWnd,uiElement.Sel, pageOneSel);
                        jo = (JObject)JsonConvert.DeserializeObject(pattern);
                        int code = jo.Value<int>("retCode");
                        if (code == 1)
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage2);
                            
                        }
                        else if(code == 0)
                        {
                            JObject obj = JObject.Parse(pattern);
                            selector = JsonConvert.SerializeObject((JObject)obj["Selector"]);
                            currentPageName = "page3";
                            m_view.InitWindow();
                            m_view.SetUrlCheckStatus(bShowUrl);
                            m_view.SetDataExtractorButtonStatus(false);
                            m_view.ChangePageThree();
                        }
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page4")
                    {
                        DataExtractorFinish(uiElement.Sel);
                        m_view.WindowState = WindowState.Normal;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            if (uiElement.ControlType == "ChromeNode")
            {
                try
                {
                    if (chromePlugin == null)
                        chromePlugin = new Chrome();
                    pluginType = "ChromeNode";
                    hWnd = uiElement.WindowHandle;
                    if (currentPageName == "page1")
                    {
                        string cacheId = chromePlugin.ElementCacheIdFromSelector(hWnd, uiElement.Sel);
                        string tableInfo = chromePlugin.OnGetTableInfo(hWnd, cacheId);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(tableInfo);
                        int code = jo.Value<int>("retCode");
                        if (code == 1)
                        {
                            currentPageName = "page2";
                            pageOneSel = uiElement.Sel;
                            m_view.ChangePageTwo();
                        }
                        else if (code == 0)
                        {
                            rootId = jo.Value<string>("root");
                            selector = chromePlugin.GetElementSelector(hWnd, rootId);

                            bool? ret = _messageBoxService.ShowQuestionYesNoCancel(Chainbot.Resources.Properties.Resources.DataExtractor_QuestionMessage1, true);
                            if (ret == true)
                            {
                                cacheId = chromePlugin.ElementCacheIdFromSelector(hWnd, selector);
                                string content = chromePlugin.OnExtractDataByPattern(hWnd, cacheId, true, new JArray());
                                m_view.InitPageFourData(content, false);
                                m_view.ChangePageFour();
                                currentPageName = "page4";
                            }
                            else
                            {
                                currentPageName = "page2";
                                pageOneSel = uiElement.Sel;
                                m_view.ChangePageTwo();
                            }
                        }
                        m_view.SetReturnButtonStauts(true);
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page2")
                    {
                        string cacheId = chromePlugin.ElementCacheIdFromSelector(hWnd, uiElement.Sel);

                        string tableInfo = chromePlugin.OnGetTableInfo(hWnd, cacheId);

                        string cacheId2 = chromePlugin.ElementCacheIdFromSelector(hWnd, pageOneSel);

                        if (cacheId == cacheId2)
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage1);
                            return;
                        }

                        string nodeName = chromePlugin.GetElementNodeName(hWnd, cacheId);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(nodeName);
                        if (jo.Value<string>("name") == "A")
                        {
                            bShowUrl = true;
                        }
                        pattern = chromePlugin.OnFindExtractPattern(hWnd, cacheId2, cacheId);
                        if (pattern != null)
                        {
                            jo = (JObject)JsonConvert.DeserializeObject(pattern);
                            rootId = jo.Value<string>("rootCustomId");
                            selector = chromePlugin.GetElementSelector(hWnd, rootId);
                            currentPageName = "page3";

                            m_view.InitWindow();
                            m_view.SetUrlCheckStatus(bShowUrl);
                            m_view.SetDataExtractorButtonStatus(false);
                            m_view.ChangePageThree();
                        }
                        else
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage2);
                        }
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page4")
                    {
                        DataExtractorFinish(uiElement.Sel);
                        m_view.WindowState = WindowState.Normal;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            if (uiElement.ControlType == "FirefoxNode")
            {
                try
                {
                    if (firefoxPlugin == null)
                        firefoxPlugin = new Firefox();
                    pluginType = "FirefoxNode";
                    hWnd = uiElement.WindowHandle;
                    if (currentPageName == "page1")
                    {
                        string cacheId = firefoxPlugin.ElementCacheIdFromSelector(hWnd, uiElement.Sel);
                        string tableInfo = firefoxPlugin.OnGetTableInfo(hWnd, cacheId);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(tableInfo);
                        int code = jo.Value<int>("retCode");
                        if (code == 1)
                        {
                            currentPageName = "page2";
                            pageOneSel = uiElement.Sel;
                            m_view.ChangePageTwo();
                        }
                        else if (code == 0)
                        {
                            rootId = jo.Value<string>("root");
                            selector = firefoxPlugin.GetElementSelector(hWnd, rootId);

                            bool? ret = _messageBoxService.ShowQuestionYesNoCancel(Chainbot.Resources.Properties.Resources.DataExtractor_QuestionMessage1, true);
                            if (ret == true)
                            {
                                cacheId = firefoxPlugin.ElementCacheIdFromSelector(hWnd, selector);
                                string content = firefoxPlugin.OnExtractDataByPattern(hWnd, cacheId, true, new JArray());
                                if (content == null)
                                {
                                    _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage3);
                                    m_view.Close();
                                }

                                m_view.InitPageFourData(content, false);
                                m_view.ChangePageFour();
                                currentPageName = "page4";
                            }
                            else
                            {
                                currentPageName = "page2";
                                pageOneSel = uiElement.Sel;
                                m_view.ChangePageTwo();
                            }
                        }
                        m_view.SetReturnButtonStauts(true);
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page2")
                    {
                        string cacheId = firefoxPlugin.ElementCacheIdFromSelector(hWnd, uiElement.Sel);

                        string tableInfo = firefoxPlugin.OnGetTableInfo(hWnd, cacheId);

                        string cacheId2 = firefoxPlugin.ElementCacheIdFromSelector(hWnd, pageOneSel);

                        if (cacheId == cacheId2)
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage1);
                            return;
                        }

                        string nodeName = firefoxPlugin.GetElementNodeName(hWnd, cacheId);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(nodeName);
                        if (jo.Value<string>("name") == "A")
                        {
                            bShowUrl = true;
                        }
                        pattern = firefoxPlugin.OnFindExtractPattern(hWnd, cacheId2, cacheId);
                        if (pattern != null)
                        {
                            jo = (JObject)JsonConvert.DeserializeObject(pattern);
                            rootId = jo.Value<string>("rootCustomId");
                            selector = firefoxPlugin.GetElementSelector(hWnd, rootId);
                            currentPageName = "page3";

                            m_view.InitWindow();
                            m_view.SetUrlCheckStatus(bShowUrl);
                            m_view.SetDataExtractorButtonStatus(false);
                            m_view.ChangePageThree();
                        }
                        else
                        {
                            _messageBoxService.ShowError(Chainbot.Resources.Properties.Resources.DataExtractor_ErrMessage2);
                        }
                        m_view.WindowState = WindowState.Normal;
                        return;
                    }

                    if (currentPageName == "page4")
                    {
                        DataExtractorFinish(uiElement.Sel);
                        m_view.WindowState = WindowState.Normal;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            if (uiElement.ControlType == "SAPNode")
            {
                try
                {
                    pluginType = "SAPNode";
                    hWnd = uiElement.WindowHandle;
                    if (currentPageName == "page1")
                    {
                        string cacheId = Sap.Instance.ElementCacheIdFromSelector(hWnd, uiElement.Sel);
                        string tableInfo = Sap.Instance.OnGetTableInfo(hWnd, cacheId);
                        JObject jo = (JObject)JsonConvert.DeserializeObject(tableInfo);
                        int code = jo.Value<int>("retCode");
                        if (code == 0)
                        {
                            rootId = jo.Value<JObject>("root").ToString();
                            selector = Sap.Instance.GetElementSelector(hWnd, rootId);
                            cacheId = Sap.Instance.ElementCacheIdFromSelector(hWnd, selector);
                            string content = Sap.Instance.OnExtractDataByPattern(hWnd, cacheId, true, null);
                            m_view.InitPageFourData(content, false);
                            m_view.ChangePageFour();
                            currentPageName = "page4";
                        }

                        m_view.SetReturnButtonStauts(true);
                        m_view.WindowState = WindowState.Normal;
                        return;

                        if (currentPageName == "page4")
                        {
                            DataExtractorFinish(uiElement.Sel);
                            m_view.WindowState = WindowState.Normal;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        private Chrome chromePlugin = null;
        private Firefox firefoxPlugin = null;
        private string currentPageName = "page1";
        private IntPtr hWnd;
        private string pageOneSel = "";
        private string pattern = "";
        private JArray ColumnsArr;
       // JObject jSelector;
        private string selector = "";
        private string rootId = "";
        private string pluginType = "";
        bool bShowUrl = false;
    }
}