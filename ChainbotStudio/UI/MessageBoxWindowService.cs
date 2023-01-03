using Chainbot.Contracts.UI;
using ChainbotStudio.ViewModel;
using ChainbotStudio.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotStudio.UI
{
    public class MessageBoxWindowService : IMessageBoxService
    {
        private IWindowService _windowService;

        public MessageBoxWindowService(IDispatcherService dispatcherService, IWindowService windowService)
        {
            _windowService = windowService;
        }

        public void Show(string txt)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = "";
            vm.Text = txt;

            vm.IsShowIcon = false;
            vm.IsShowDefault = true;


            _windowService.ShowDialog(window);
        }

        public void ShowInformation(string txt)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_PromptTitle;
            vm.Text = txt;

            vm.IsShowInformationIcon = true;
            vm.IsShowDefault = true;

            _windowService.ShowDialog(window);
        }

        public void ShowWarning(string txt)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_WarningTitle;
            vm.Text = txt;

            vm.IsShowWarningIcon = true;
            vm.IsShowDefault = true;

            _windowService.ShowDialog(window);
        }

        public bool ShowWarningYesNo(string txt, bool bDefaultYes = false)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_WarningTitle;
            vm.Text = txt;

            vm.IsShowWarningIcon = true;
            vm.IsShowYesNo = true;

            if (bDefaultYes)
            {
                vm.IsYesDefault = true;
            }
            else
            {
                vm.IsNoDefault = true;
            }

            _windowService.ShowDialog(window);

            return vm.DialogResult == true;
        }

        public void ShowError(string txt)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_ErrorTitle;
            vm.Text = txt;

            vm.IsShowErrorIcon = true;
            vm.IsShowDefault = true;

            _windowService.ShowDialog(window);
        }

        public bool ShowQuestion(string txt, bool bDefaultYes = false)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_QuestionTitle;
            vm.Text = txt;

            vm.IsShowQuestionIcon = true;
            vm.IsShowYesNo = true;

            if(bDefaultYes)
            {
                vm.IsYesDefault = true;
            }
            else
            {
                vm.IsNoDefault = true;
            }

            _windowService.ShowDialog(window);

            return vm.DialogResult == true;
        }

        public bool? ShowQuestionYesNoCancel(string txt, bool bDefaultYes = false)
        {
            var window = new MessageBoxWindow();
            window.Topmost = true;
            var vm = window.DataContext as MessageBoxViewModel;

            vm.Title = Chainbot.Resources.Properties.Resources.MessageBox_QuestionTitle;
            vm.Text = txt;

            vm.IsShowQuestionIcon = true;
            vm.IsShowYesNoCancel = true;

            if (bDefaultYes)
            {
                vm.IsYesDefault = true;
            }
            else
            {
                vm.IsNoDefault = true;
            }

            _windowService.ShowDialog(window);

            return vm.DialogResult;
        }

        
    }
}
