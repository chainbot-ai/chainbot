using ChainbotRobot.Contracts;
using ChainbotRobot.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChainbotRobot.Cores
{
    public class AutoCloseMessageBoxService : IAutoCloseMessageBoxService
    {
        private int Timeout = 10000;

        public MessageBoxResult Show(string messageBoxText)
        {
            var msgBox = System.Windows.Forms.AutoClosingMessageBox.Factory(
                            showMethod: (_caption, _buttons) =>
                                System.Windows.Forms.MessageBox.Show(messageBoxText)
                        );

            var ret = msgBox.Show(timeout: Timeout);

            return (MessageBoxResult)ret;
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult = MessageBoxResult.Yes)
        {
            var msgBox = System.Windows.Forms.AutoClosingMessageBox.Factory(
                            showMethod: (_caption, _buttons) =>
                                System.Windows.Forms.MessageBox.Show(new WindowWrapper(owner), messageBoxText, _caption, _buttons, (System.Windows.Forms.MessageBoxIcon)icon),
                            caption: caption
                        );

            var ret = msgBox.Show(
                timeout: Timeout,
                buttons: (System.Windows.Forms.MessageBoxButtons)button,
                defaultResult: (System.Windows.Forms.DialogResult)defaultResult);

            return (MessageBoxResult)ret;
        }
    }
}
