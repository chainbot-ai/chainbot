using Chainbot.Contracts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.UI
{
    public class DialogService : IDialogService
    {
        private string defaultSelectDirPath = "";  

        public bool ShowSelectDirDialog(string description, ref string selectedDir)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = description;
            if (defaultSelectDirPath != "")
            {
                dialog.SelectedPath = defaultSelectDirPath;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return false;
                }

                selectedDir = dialog.SelectedPath;

                defaultSelectDirPath = dialog.SelectedPath;
                return true;
            }

            return false;
        }
    }
}
