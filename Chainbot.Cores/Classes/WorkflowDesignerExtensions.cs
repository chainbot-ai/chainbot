using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Validation;
using System.Activities.Presentation.View;
using System.Diagnostics;

namespace Chainbot.Cores.Classes
{
    public static class WorkflowDesignerExtensions
    {
        public static ModelService GetModelService(this WorkflowDesigner designer)
        {
            return designer.Context.Services.GetService<ModelService>();
        }

        public static ViewService GetViewService(this WorkflowDesigner designer)
        {
            return designer.Context.Services.GetService<ViewService>();
        }

        public static ValidationService GetValidationService(this WorkflowDesigner designer)
        {
            return designer.Context.Services.GetService<ValidationService>();
        }

        public static T GetService<T>(this WorkflowDesigner designer)
        {
            return designer.Context.Services.GetService<T>();
        }

        public static ModelItem GetPrimarySelectedModelItem(this WorkflowDesigner designer)
        {
            return designer.Context.Items.GetValue<Selection>().PrimarySelection;
        }

        public static ModelItem GetModelItem(this WorkflowDesigner designer, string activityId)
        {
            Activity activity = designer.GetModelService().GetRootActivity();
            if (activity == null || activityId == null)
            {
                return null;
            }
            activity = (activity.GetParent() ?? activity);
            Activity instance = null;
            try
            {
                instance = WorkflowInspectionServices.Resolve(activity, activityId);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine("GetModelItem: " + ex.Message);
            }
            ModelItem modelItem = designer.Context.Services.GetRequiredService<ModelTreeManager>().GetModelItem(instance, true);
            if (modelItem == null)
            {
                Trace.WriteLine("Cannot find " + activityId);
            }
            return modelItem;
        }
    }
}
