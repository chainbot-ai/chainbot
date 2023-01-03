using Chainbot.Contracts.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Contracts.Classes
{
    public static class GlobalVars
    {
        public static FrameworkElement CurrentWorkflowDesignerView { get; set; }
        public static FrameworkElement CurrentWorkflowPropertyView { get; set; }

        public static IWorkflowDesignerService CurrentWorkflowDesignerService { get; set; }
    }
}
