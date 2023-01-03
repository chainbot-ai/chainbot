using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chainbot.Resources.Librarys
{
    public static class TreeViewHelperExt
    {
        public static bool GetCanMouseRightButtonSelect(TreeView treeView)
        {
            return (bool)treeView.GetValue(TreeViewHelperExt.CanMouseRightButtonSelectProperty);
        }

        public static void SetCanMouseRightButtonSelect(TreeView treeView, bool value)
        {
            treeView.SetValue(TreeViewHelperExt.CanMouseRightButtonSelectProperty, value);
        }

        private static void OnCanMouseRightButtonSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = d as TreeView;
            treeView.RemoveHandler(UIElement.PreviewMouseRightButtonDownEvent, new RoutedEventHandler(TreeViewHelperExt.OnTreeViewItemPreviewMouseRightButtonDown));
            if ((bool)e.NewValue)
            {
                treeView.AddHandler(UIElement.PreviewMouseRightButtonDownEvent, new RoutedEventHandler(TreeViewHelperExt.OnTreeViewItemPreviewMouseRightButtonDown));
            }
        }


        private static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        private static void OnTreeViewItemPreviewMouseRightButtonDown(object sender, RoutedEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }
        
        public static readonly DependencyProperty CanMouseRightButtonSelectProperty = DependencyProperty.RegisterAttached("CanMouseRightButtonSelect", typeof(bool), typeof(TreeViewHelperExt), new PropertyMetadata(new PropertyChangedCallback(TreeViewHelperExt.OnCanMouseRightButtonSelectChanged)));
    }
}
