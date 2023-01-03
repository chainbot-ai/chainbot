using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.UI
{
    public interface IMessageBoxService
    {
        void Show(string txt);

        void ShowInformation(string txt);

        void ShowWarning(string txt);

        bool ShowWarningYesNo(string txt, bool bDefaultYes = false);

        void ShowError(string txt);

        bool ShowQuestion(string txt, bool bDefaultYes = false);

        bool? ShowQuestionYesNoCancel(string txt, bool bDefaultYes = false);
    }
}
