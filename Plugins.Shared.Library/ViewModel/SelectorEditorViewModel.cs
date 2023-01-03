using System;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Languages.Xml.Implementation;
using GalaSoft.MvvmLight;
using Plugins.Shared.Library.Editors;
using System.Xml;
using System.Text;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using Plugins.Shared.Library.UiAutomation;
using System.Collections.ObjectModel;
using Chainbot.Contracts.Classes;
using Plugins.Shared.Library.Extensions;

namespace Plugins.Shared.Library.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SelectorEditorViewModel : ViewModelBase
    {
        public bool  HasInitTreeView { get; set; }

        /// <summary>
        /// Initializes a new instance of the SelectorEditorViewModel class.
        /// </summary>
        public SelectorEditorViewModel()
        {
            MakeView();
        }


        public void MakeView()
        {
            EditorDocument.Language = new XmlSyntaxLanguage();
            EditorDocument.IsReadOnly = false;

            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                SyntaxEditorThemeHelper.UpdateDarkStyle();
            }
        }


        public void MakeTreeView()
        {
            var selectorXml = EditorDocument.Text;

            XmlDocument doc = new XmlDocument();

            byte[] encodedString = Encoding.UTF8.GetBytes(selectorXml);

            using (var ms = new MemoryStream(encodedString))
            {
                ms.Flush();
                ms.Position = 0;
                doc.Load(ms);
                ms.Close();
            }

            var rootNode = doc.DocumentElement;
            InitTreeView(rootNode);

            HasInitTreeView = true;
        }

        private void InitTreeView(XmlElement rootNode)
        {
            var rootElement = rootNode as XmlElement;
            var rootItem = new SelectorXmlItemViewModel(this, SelectorXmlItemViewModel.XmlType.XmlElement);
            rootItem.Name = rootElement.Name;
            Children.Add(rootItem);

            InitChildren(rootElement,rootItem);
        }

        private void InitChildren(XmlElement element, SelectorXmlItemViewModel vmItem)
        {
            foreach (var childNode in element.ChildNodes)
            {
                if(childNode is XmlText)
                {
                    var childTextNode = childNode as XmlText;

                    var childItem = new SelectorXmlItemViewModel(this, SelectorXmlItemViewModel.XmlType.XmlText);
                    childItem.Name = childTextNode.Name;
                    childItem.Value = childTextNode.Value;
                    childItem.Parent = vmItem;

                    vmItem.Children.Add(childItem);
                }
                else if (childNode is XmlElement)
                {
                    var childElement = childNode as XmlElement;

                    var childItem = new SelectorXmlItemViewModel(this, SelectorXmlItemViewModel.XmlType.XmlElement);
                    childItem.Name = childElement.Name;
                    childItem.Parent = vmItem;

                    vmItem.Children.Add(childItem);

                    InitChildren(childElement, childItem);
                }
            }

            foreach (XmlAttribute attr in element.Attributes)
            {
                var childItem = new SelectorXmlItemViewModel(this, SelectorXmlItemViewModel.XmlType.XmlAttribute);
                childItem.Name = attr.Name;
                childItem.Value = attr.Value;
                childItem.Parent = vmItem;

                vmItem.Children.Add(childItem);
            }
        }

        private RelayCommand _highlightElementCommand;

        /// <summary>
        /// Gets the HighlihtElementCommand.
        /// </summary>
        public RelayCommand HighlightElementCommand
        {
            get
            {
                return _highlightElementCommand
                    ?? (_highlightElementCommand = new RelayCommand(
                    () =>
                    {
                        //var element = UiElement.FromSelector(Selector, 1);

                        //if (element != null)
                        //{
                        //    SharedObject.Instance.ShowMainWindowMinimized();
                        //    element.SetForeground();
                        //    element.DrawHighlight(System.Drawing.Color.Red, TimeSpan.FromSeconds(3), true);
                        //    SharedObject.Instance.ShowMainWindowNormal();
                        //    SharedObject.Instance.ActivateMainWindow();
                        //}

                        try
                        {
                            SharedObject.Instance.ShowMainWindowMinimized();

                            var element = UiElement.FromSelector(Selector, 1);
                            element.SetForeground();
                            element.DrawHighlight(System.Drawing.Color.Red, TimeSpan.FromSeconds(3), true);
                        }
                        catch (Exception ex)
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, "未找到要高亮的元素，请检查！", ex);
                        }
                        finally
                        {
                            SharedObject.Instance.ShowMainWindowNormal();
                        }
                    }));
            }
        }

        /// <summary>
        /// The <see cref="EditorDocument" /> property's name.
        /// </summary>
        public const string EditorDocumentPropertyName = "EditorDocument";

        private EditorDocument _editorDocumentProperty = new EditorDocument();

        /// <summary>
        /// Sets and gets the EditorDocument property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public EditorDocument EditorDocument
        {
            get
            {
                return _editorDocumentProperty;
            }

            set
            {
                if (_editorDocumentProperty == value)
                {
                    return;
                }

                _editorDocumentProperty = value;
                RaisePropertyChanged(EditorDocumentPropertyName);
            }
        }


        public string PropertyName { get; set; }


        public string Selector
        {
            get
            {
                return UnwrapSelectorXml(EditorDocument.Text).WrapXmlWithDoubleQuotationMarks();
            }

            set
            {
                EditorDocument.SetText(WrapSelectorXml(value));
            }
        }

        private string UnwrapSelectorXml(string wrappedSelector)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                byte[] encodedString = Encoding.UTF8.GetBytes(wrappedSelector);

                using (var ms = new MemoryStream(encodedString))
                {
                    ms.Flush();
                    ms.Position = 0;
                    doc.Load(ms);
                    ms.Close();
                }

                var rootNode = doc.DocumentElement;
                return rootNode.InnerXml;
            }
            catch (Exception ex)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "XML格式非法，不允许保存值！", ex);
            }

            return "";
        }


        private string FormatXmlDoc(XmlDocument doc)
        {
            MemoryStream msWrite = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(msWrite, Encoding.UTF8);
            xmlWriter.Indentation = 4;
            xmlWriter.Formatting = Formatting.Indented;
            doc.WriteContentTo(xmlWriter);
            xmlWriter.Close();

            return Encoding.UTF8.GetString(msWrite.ToArray());
        }

        private string WrapSelectorXml(string selector)
        {
            var selectorXml = $"<Selector>{selector}</Selector>";

            try
            {
                XmlDocument doc = new XmlDocument();

                byte[] encodedString = Encoding.UTF8.GetBytes(selectorXml);

                using (var ms = new MemoryStream(encodedString))
                {
                    ms.Flush();
                    ms.Position = 0;
                    doc.Load(ms);
                    ms.Close();
                }

                return FormatXmlDoc(doc);
            }
            catch (Exception ex)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "XML格式非法，请检查！", ex);
            }

            return "";
        }



        /// <summary>
        /// The <see cref="Children" /> property's name.
        /// </summary>
        public const string ChildrenPropertyName = "Children";

        private ObservableCollection<SelectorXmlItemViewModel> _childrenProperty = new ObservableCollection<SelectorXmlItemViewModel>();

        /// <summary>
        /// Sets and gets the Children property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SelectorXmlItemViewModel> Children
        {
            get
            {
                return _childrenProperty;
            }

            set
            {
                if (_childrenProperty == value)
                {
                    return;
                }

                _childrenProperty = value;
                RaisePropertyChanged(ChildrenPropertyName);
            }
        }

        

        public void RefershXmlEditorDocument()
        {
            if(!HasInitTreeView)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();

            foreach (var item in Children)
            {
                if(item.IsChecked == false)
                {
                    continue;
                }

               if(item.Type == SelectorXmlItemViewModel.XmlType.XmlElement)
                {
                    var itemElement = doc.CreateElement(item.Name);
                    doc.AppendChild(itemElement);

                    foreach (var item_child in item.Children)
                    {
                        if (item_child.IsChecked == false)
                        {
                            continue;
                        }

                        if (item_child.Type == SelectorXmlItemViewModel.XmlType.XmlAttribute)
                        {
                            itemElement.SetAttribute(item_child.Name, item_child.Value);
                        }else if(item_child.Type == SelectorXmlItemViewModel.XmlType.XmlText)
                        {
                            itemElement.InnerText = item_child.Value;
                        }else if(item_child.Type == SelectorXmlItemViewModel.XmlType.XmlElement)
                        {
                            BuildChildrenXml(doc, itemElement,item_child);
                        }
                    }
                }
            }

            var formatXmlStr = FormatXmlDoc(doc);
            EditorDocument.SetText(formatXmlStr);
        }

        private void BuildChildrenXml(XmlDocument doc, XmlElement itemParentElement,SelectorXmlItemViewModel item)
        {
            var itemElement = doc.CreateElement(item.Name);
            itemParentElement.AppendChild(itemElement);

            foreach (var item_child in item.Children)
            {
                if (item_child.IsChecked == false)
                {
                    continue;
                }

                if (item_child.Type == SelectorXmlItemViewModel.XmlType.XmlAttribute)
                {
                    itemElement.SetAttribute(item_child.Name, item_child.Value);
                }
                else if (item_child.Type == SelectorXmlItemViewModel.XmlType.XmlText)
                {
                    itemElement.InnerText = item_child.Value;
                }
                else if (item_child.Type == SelectorXmlItemViewModel.XmlType.XmlElement)
                {
                    BuildChildrenXml(doc, itemElement, item_child);
                }
            }
        }
    }
}