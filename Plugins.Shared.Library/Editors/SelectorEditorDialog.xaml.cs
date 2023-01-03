using Plugins.Shared.Library.ViewModel;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Plugins.Shared.Library.Editors
{
    public partial class SelectorEditorDialog
    {
        public SelectorEditorDialog(ModelItem ownerActivity)
        {
            base.ModelItem = ownerActivity;
            base.Context = ownerActivity.GetEditingContext();

            InitializeComponent();
        }

        protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
            if (dialogResult.HasValue && dialogResult.Value)
            {
                var vm = this.rootGrid.DataContext as SelectorEditorViewModel;
                if(!string.IsNullOrEmpty(vm.Selector))
                {
                    base.ModelItem.Properties[vm.PropertyName].SetValue(new InArgument<string>(vm.Selector));
                }
            }
        }

        private void TreeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        private void WorkflowElementDialog_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
    }
}
