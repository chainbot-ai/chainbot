using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.ViewModel;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Plugins.Shared.Library.Editors
{
    public class SelectorEditor : DialogPropertyValueEditor
    {
        public SelectorEditor()
        {
            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["SelectorEditorTemplate"];
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            try
            {
                ModelItem ownerActivity = new ModelPropertyEntryToOwnerActivityConverter().Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
                ShowDialog(ownerActivity, propertyValue.ParentProperty.PropertyName);
            }
            catch(Exception ex)
            {
            }
        }

        public void ShowDialog(ModelItem ownerActivity, string propertyName)
        {
            using (ModelEditingScope modelEditingScope = ownerActivity.BeginEdit())
            {
                var dlg = new SelectorEditorDialog(ownerActivity);
                var vm = dlg.rootGrid.DataContext as SelectorEditorViewModel;
                if(dlg.ModelItem.Properties[propertyName].ComputedValue != null)
                {
                    vm.PropertyName = propertyName;
                    vm.Selector = Common.GetStringLiteralFromArgument(dlg.ModelItem.Properties[propertyName].ComputedValue as InArgument<string>);
                    vm.MakeTreeView();

                    if (dlg.ShowOkCancel())
                    {
                        modelEditingScope.Complete();
                    }
                    else
                    {
                        modelEditingScope.Revert();
                    }
                }
            }
        }
    }
}
