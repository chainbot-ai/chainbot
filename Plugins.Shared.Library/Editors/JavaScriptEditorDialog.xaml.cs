using Chainbot.Contracts.Classes;
using System.Activities.Presentation.Model;
using System.IO;
using System.Xml;

namespace Plugins.Shared.Library.Editors
{
    public partial class JavaScriptEditorDialog
    {
        public JavaScriptEditorDialog(ModelItem ownerActivity)
        {
            base.ModelItem = ownerActivity;
            base.Context = ownerActivity.GetEditingContext();
            InitializeComponent();
            textEditor.Text = base.ModelItem.Properties["Code"].ComputedValue as string;

            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                XmlTextReader xshd_reader = new XmlTextReader(new MemoryStream(Properties.Resources.JavaScript_Mode));
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                xshd_reader.Close();
            }
            else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                XmlTextReader xshd_reader = new XmlTextReader(new MemoryStream(Properties.Resources.JavaScript_Mode_Light));
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                xshd_reader.Close();
            }
            
        }

        protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
            if (dialogResult.HasValue && dialogResult.Value)
            {
                base.ModelItem.Properties["Code"].ComputedValue = textEditor.Text;
            }
        }

    }
}
