using Chainbot.Contracts.Classes;
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Plugins.Shared.Library.Editors
{
    public partial class CodeEditorDialog : WorkflowElementDialog
    {
        private string lang;

        public CodeEditorDialog()
        {
            InitializeComponent();
        }

        public CodeEditorDialog(ModelItem ownerActivity)
        {
            base.ModelItem = ownerActivity;
            base.Context = ownerActivity.GetEditingContext();
            InitializeComponent();

            this.lang = base.ModelItem.Properties["Language"].ComputedValue.ToString();

            if(lang == "VBNet")
            {
                textEditor_vb.Text = base.ModelItem.Properties["Code"].ComputedValue as string;
                textEditor_vb.Visibility = Visibility.Visible;
                lblContentTitle.Content = Properties.Resources.CodeEditorVBLabel;

                XmlTextReader xshd_reader = new XmlTextReader(new MemoryStream(Properties.Resources.VBNET_Mode));
                textEditor_vb.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                xshd_reader.Close();
            }
            else
            {
                textEditor_csharp.Text = base.ModelItem.Properties["Code"].ComputedValue as string;
                textEditor_csharp.Visibility = Visibility.Visible;
                lblContentTitle.Content = Properties.Resources.CodeEditorCSharpLabel;

                if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
                {
                    XmlTextReader xshd_reader = new XmlTextReader(new MemoryStream(Properties.Resources.CSharp_Mode));
                    textEditor_csharp.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    xshd_reader.Close();
                }
                else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
                {
                    XmlTextReader xshd_reader = new XmlTextReader(new MemoryStream(Properties.Resources.CSharp_Mode_Light));
                    textEditor_csharp.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    xshd_reader.Close();
                }
                
            }
            
            
        }

        protected override void OnWorkflowElementDialogClosed(bool? dialogResult)
        {
            if (dialogResult != null && dialogResult.Value)
            {
                string text = "";
                if (lang == "VBNet")
                    text = this.textEditor_vb.Text;
                else
                    text = this.textEditor_csharp.Text;
                base.ModelItem.Properties["Code"].ComputedValue = text;
            }
        }

    }
}
