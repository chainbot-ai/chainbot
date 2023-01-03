using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Plugins.Shared.Library.Editors
{
    public static class SyntaxEditorThemeHelper
    {
        public static void UpdateDarkStyle()
        {
            using (Stream stream = Application.GetResourceStream(new Uri($"pack://application:,,,/Chainbot.Resources;Component/Themes/Dark.vssettings", UriKind.Absolute)).Stream)
            {
                if (stream != null)
                    AmbientHighlightingStyleRegistry.Instance.ImportHighlightingStyles(stream);
            }
        }
    }
}
