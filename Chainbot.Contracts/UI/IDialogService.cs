using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.UI
{
    public interface IDialogService
    {
        bool ShowSelectDirDialog(string description, ref string selectedDir);
    }
}
