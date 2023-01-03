using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Classes
{
    public static class EditingContextExtensions
    {
        public static ModelItem GetRootModelItem(this EditingContext context)
        {
            ModelTreeManager requiredService = context.Services.GetRequiredService<ModelTreeManager>();
            ModelItem modelItem = (requiredService != null) ? requiredService.Root : null;
            ActivityBuilder activityBuilder = ((modelItem != null) ? modelItem.GetCurrentValue() : null) as ActivityBuilder;
            if (activityBuilder == null)
            {
                return modelItem;
            }
            return requiredService.GetModelItem(activityBuilder.Implementation, false);
        }

        public static bool IsReadOnly(this EditingContext context)
        {
            return context.Items.GetValue<ReadOnlyState>().IsReadOnly;
        }
    }
}
