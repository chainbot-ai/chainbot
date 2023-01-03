using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;

namespace Plugins.Shared.Library.Editors
{
    public class VBAScriptEditor : DialogPropertyValueEditor
    {
        private string Title = "";
        private string ContentTitle = "";
        public VBAScriptEditor(string title, string contentTitle)
        {
            this.Title = title;
            this.ContentTitle = contentTitle;

            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["VBAScriptEditorTemplate"];
        }

        public VBAScriptEditor()
        {
            base.InlineEditorTemplate = (DataTemplate)new EditorTemplates()["VBAScriptEditorTemplate"];
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
                var dlg = new VBAScriptEditorDialog(ownerActivity);

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
