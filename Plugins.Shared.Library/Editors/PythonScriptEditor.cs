using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;

namespace Plugins.Shared.Library.Editors
{

    public class PythonScriptEditor : DialogPropertyValueEditor
    {
        private string Title = "";
        private string ContentTitle = "";

        public PythonScriptEditor(string title, string contentTitle)
        {
            this.Title = title;
            this.ContentTitle = contentTitle;

            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["PythonScriptEditorTemplate"];
        }

        public PythonScriptEditor()
        {
            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["PythonScriptEditorTemplate"];
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            try
            {
                ModelItem ownerActivity = new ModelPropertyEntryToOwnerActivityConverter().Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
                ShowDialog(ownerActivity);
            }
            catch
            {
            }
        }

        public void ShowDialog(ModelItem ownerActivity)
        {
            using (ModelEditingScope modelEditingScope = ownerActivity.BeginEdit())
            {
                var dlg = new PythonScriptEditorDialog(ownerActivity);

                if (!string.IsNullOrEmpty(Title))
                {
                    dlg.Title = Title;
                }

                if (!string.IsNullOrEmpty(ContentTitle))
                {
                    dlg.lblContentTitle.Content = ContentTitle;
                }

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
