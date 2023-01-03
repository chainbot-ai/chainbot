using System.Activities.Presentation.Model;

namespace Plugins.Shared.Library.Editors
{
    public partial class TextEditorDialog
    {
        public TextEditorDialog(ModelItem ownerActivity)
        {
            base.ModelItem = ownerActivity;
            base.Context = ownerActivity.GetEditingContext();
            InitializeComponent();
        }
    }
}
