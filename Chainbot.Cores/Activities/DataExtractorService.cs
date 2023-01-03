using System;
using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;
using System.Activities;
using System.Collections.Generic;
using System.Activities.Statements;
using System.Activities.Presentation;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Model;
using Chainbot.Contracts.Workflow;
using Chainbot.Contracts.Project;

namespace Chainbot.Cores.Activities
{
    public class DataExtractorService : MarshalByRefServiceBase, IDataExtractorService
    {
        private IWorkflowDesignerCollectService _workflowDesignerCollectService;
        private IProjectManagerService _projectManagerService;

        public DataExtractorService(IWorkflowDesignerCollectService workflowDesignerCollectService, IProjectManagerService projectManagerService)
        {
            _workflowDesignerCollectService = workflowDesignerCollectService;
            _projectManagerService = projectManagerService;
        }


        struct stuActivityInfo
        {
            public Activity activity;
            public Action<Activity> postAction;
        }

        private List<stuActivityInfo> _activityDataExtractorList = new List<stuActivityInfo>();

        public void Save(string path, string targetData, string metaData, string pageNextData)
        {
            var typeStr = "Chainbot.Core.Activities.DataExtractor.DataExtractorActivity,Chainbot.Core.Activities";
            Type activityType = Type.GetType(typeStr);
            if (activityType == null)
            {
                return;
            }

            dynamic activity = Activator.CreateInstance(activityType);
            activity.target = targetData;
            activity.metadata = metaData;
            activity.PageElements = pageNextData;

            stuActivityInfo info = new stuActivityInfo();
            info.activity = activity;
            info.postAction = (activityItem) =>
            {
                activityItem.DisplayName = Chainbot.Resources.Properties.Resources.DataExtractorActivityDisplayName;
            };
            WorkflowDesigner wd = _workflowDesignerCollectService.Get(path)?.GetWorkflowDesigner();

            if (wd != null)
            {
                ModelService modelService = wd.Context.Services.GetService<ModelService>();
                ModelItem rootModelItem = modelService.Root.Properties["Implementation"].Value;

                info.postAction?.Invoke(info.activity);

                if (rootModelItem == null)
                {
                    modelService.Root.Content.SetValue(info.activity);
                }
                else
                {
                    rootModelItem.AddActivity(info.activity);
                }
            }
        }
    }
}
