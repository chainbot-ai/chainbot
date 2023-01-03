using Chainbot.Contracts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Cores.UI
{
    public class MessageBoxService : IMessageBoxService
    {
        public void Show(string txt)
        {
            MessageBox.Show(txt);
        }

        public void ShowInformation(string txt)
        {
            MessageBox.Show(txt, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string txt)
        {
            MessageBox.Show(txt, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public bool ShowWarningYesNo(string txt, bool bDefaultYes = false)
        {
            var ret = MessageBox.Show(txt, "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning, bDefaultYes ? MessageBoxResult.Yes : MessageBoxResult.No);
            if (ret == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }

        public void ShowError(string txt)
        {
            MessageBox.Show(txt, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowQuestion(string txt,bool bDefaultYes = false)
        {
            var ret = MessageBox.Show(txt, "询问", MessageBoxButton.YesNo, MessageBoxImage.Question, bDefaultYes ? MessageBoxResult.Yes : MessageBoxResult.No);
            if (ret == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }

        public bool? ShowQuestionYesNoCancel(string txt, bool bDefaultYes = false)
        {
            var ret = MessageBox.Show(txt, "询问", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, bDefaultYes ? MessageBoxResult.Yes : MessageBoxResult.Cancel);

            if (ret == MessageBoxResult.Yes)
            {
                return true;
            }
            else if (ret == MessageBoxResult.No)
            {
                return false;
            }
            else
            {
                return null;
            }
        }

    }
}
