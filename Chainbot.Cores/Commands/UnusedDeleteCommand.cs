using Chainbot.Contracts.Workflow;
using System;
using System.Windows.Input;

namespace Chainbot.Cores.Commands
{
    public class UnusedDeleteCommand : ICommand
    {
        public string Path;
        private IWorkflowActivityService _workflowActivityService;

        public UnusedDeleteCommand(IWorkflowActivityService _workflowActivityService, string path)
        {
            this._workflowActivityService = _workflowActivityService;
            this.Path = path;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return (_workflowActivityService.CheckUnusedVariables(Path) || _workflowActivityService.CheckUnusedArguments(Path));
            //return true;
        }

        public void Execute(object parameter)
        {
            _workflowActivityService.RemoveUnusedVariables(Path);
            _workflowActivityService.RemoveUnusedArguments(Path);
        }
    }
}
