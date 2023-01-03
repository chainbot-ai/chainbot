using Chainbot.Contracts.Workflow;
using System;
using System.Windows.Input;

namespace Chainbot.Cores.Commands
{
    public class RunThisActivityCommand : ICommand
    {
        private IWorkflowActivityService _workflowActivityService;

        public RunThisActivityCommand(IWorkflowActivityService _workflowActivityService)
        {
            this._workflowActivityService = _workflowActivityService;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
           
        }
    }
}
