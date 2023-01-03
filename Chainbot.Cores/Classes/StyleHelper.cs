using ActiproSoftware.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Chainbot.Cores.Classes
{
    public static class StyleHelper
    {
        public static Style BaseStyleOn(Style targetStyle, Style basedOn, Type targetType)
        {
            if (basedOn == null)
            {
                return targetStyle;
            }
            Style style = new Style(targetType, basedOn);
            if (targetStyle == null)
            {
                return style;
            }
            if (targetStyle.Setters != null)
            {
                foreach (SetterBase item in targetStyle.Setters)
                {
                    style.Setters.Add(item);
                }
            }
            if (targetStyle.Triggers != null)
            {
                foreach (TriggerBase item2 in targetStyle.Triggers)
                {
                    style.Triggers.Add(item2);
                }
            }
            return style;
        }

        public static void ApplyThemedStyle(this DataGrid dataGrid)
        {
            dataGrid.CellStyle = StyleHelper.BaseStyleOn(dataGrid.CellStyle, Application.Current.TryFindResource("VariablesDataGridCellCustomStyle") as Style, typeof(DataGridCell));
            dataGrid.RowStyle = StyleHelper.BaseStyleOn(dataGrid.RowStyle, Application.Current.TryFindResource(DataGridResourceKeys.DataGridRowStyleKey) as Style, typeof(DataGridRow));
            dataGrid.ColumnHeaderStyle = StyleHelper.BaseStyleOn(dataGrid.ColumnHeaderStyle, Application.Current.TryFindResource(DataGridResourceKeys.DataGridColumnHeaderStyleKey) as Style, typeof(DataGridColumnHeader));
            dataGrid.RowHeaderStyle = StyleHelper.BaseStyleOn(dataGrid.RowHeaderStyle, Application.Current.TryFindResource(DataGridResourceKeys.DataGridRowHeaderStyleKey) as Style, typeof(DataGridRowHeader));
            dataGrid.Style = StyleHelper.BaseStyleOn(dataGrid.Style, Application.Current.TryFindResource(DataGridResourceKeys.DataGridStyleKey) as Style, typeof(DataGrid));
            dataGrid.AlternatingRowBackground = null;
        }
    }
}
