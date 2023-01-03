using System;
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
    public class CodeEditor : DialogPropertyValueEditor
    {
        public CodeEditor()
        {
            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["CodeEditorTemplate"];
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            try
            {
                ModelItem ownerActivity = new ModelPropertyEntryToOwnerActivityConverter().Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
                this.ShowDialog(ownerActivity);
            }
            catch
            {
            }
        }

        public void ShowDialog(ModelItem ownerActivity)
        {
            using (ModelEditingScope modelEditingScope = ownerActivity.BeginEdit())
            {
                if (new CodeEditorDialog(ownerActivity).ShowOkCancel())
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
