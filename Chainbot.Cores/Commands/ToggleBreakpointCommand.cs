using System;
using System.Windows.Input;
using Chainbot.Contracts.Workflow;

namespace Chainbot.Cores.Commands
{
    public class ToggleBreakpointCommand : ICommand
    {
        private IWorkflowBreakpointsService _workflowBreakpointsService;
        public string Path;

        public ToggleBreakpointCommand(IWorkflowBreakpointsService workflowBreakpointsService, string path)
        {
            this._workflowBreakpointsService = workflowBreakpointsService;
            this.Path = path;
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
            this._workflowBreakpointsService.ToggleBreakpoint(this.Path);
        }
    }
}
