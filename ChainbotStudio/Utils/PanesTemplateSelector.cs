using ChainbotStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace ChainbotStudio.Utils
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }

        public DataTemplate StartDocumentViewTemplate
        {
            get;
            set;
        }

        public DataTemplate DesignerDocumentViewTemplate
        {
            get;
            set;
        }

        public DataTemplate SourceCodeDocumentViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is DesignerDocumentViewModel)
                return DesignerDocumentViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
