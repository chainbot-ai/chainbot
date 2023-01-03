using ChainbotStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChainbotStudio.Utils
{
    class PanesStyleSelector : StyleSelector
    {
        public Style StartDocumentStyle
        {
            get;
            set;
        }

        public Style DesignerDocumentStyle
        {
            get;
            set;
        }

        public Style SourceCodeDocumentStyle
        {
            get;
            set;
        }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is DesignerDocumentViewModel)
                return DesignerDocumentStyle;

            return base.SelectStyle(item, container);
        }
    }
}
