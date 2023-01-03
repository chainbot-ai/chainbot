using Microsoft.VisualBasic.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.ExpressionEditor
{
    internal interface IWorkflowImportReferenceService
    {
        VisualBasicSettings DefaultSettings { get; }
    }
}
