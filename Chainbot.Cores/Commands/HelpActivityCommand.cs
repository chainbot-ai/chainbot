using System;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Diagnostics;
using System.Windows.Input;

namespace Chainbot.Cores.Commands
{
    public class HelpActivityCommand : ICommand
    {
        private WorkflowDesigner _designer;
        private string _helpLinkUrl;

        public HelpActivityCommand(WorkflowDesigner _designer, string _helpLinkUrl)
        {
            this._designer = _designer;
            this._helpLinkUrl = _helpLinkUrl;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = false;

            var selectedModelItem = this._designer.Context.Items.GetValue<Selection>().PrimarySelection;
            Type activityType = (selectedModelItem != null) ? selectedModelItem.ItemType : null;

            if (selectedModelItem != null)
            {
                canExecute = true;
            }

            return canExecute;
        }

        private void LaunchProcess(string fileName, bool useShellExecute = true, string arguments = null)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = useShellExecute
                };
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    process.StartInfo.Arguments = arguments;
                }
                process.Start();
            }
        }

        private void OpenHelpLink(Type itemType)
        {
            var url = this._helpLinkUrl + "/help/" + $"{itemType.Name.Split('`')[0]}.html";

            LaunchProcess(url);
        }

        public void Execute(object parameter)
        {
            var selectedModelItem = this._designer.Context.Items.GetValue<Selection>().PrimarySelection;
            Type itemType = selectedModelItem.ItemType;
            OpenHelpLink(itemType);
        }
       
    }
}
