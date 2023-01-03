using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Project
{
    public interface IProjectManagerService
    {
        event EventHandler ProjectLoadingBeginEvent;

        event EventHandler ProjectLoadingEndEvent;

        event EventHandler ProjectLoadingExceptionEvent;

        event EventHandler ProjectPreviewOpenEvent;

        event EventHandler ProjectOpenEvent;

        event EventHandler<CancelEventArgs> ProjectPreviewCloseEvent;

        event EventHandler ProjectCloseEvent;


        string CurrentProjectPath { get; }



        string CurrentProjectMainXamlFileAbsolutePath { get; }


        string CurrentProjectConfigFilePath { get; }


        ProjectJsonConfig CurrentProjectJsonConfig { get; }


        List<string> AllActivityConfigXmls { get;}


        List<ActivityGroupOrLeafItem> Activities { get;}


        Dictionary<string, ActivityGroupOrLeafItem> ActivitiesTypeOfDict { get;}


        IActivitiesServiceProxy CurrentActivitiesServiceProxy { get;}



        List<string> CurrentActivitiesDllLoadFrom { get;}

 
        List<string> CurrentDependentAssemblies { get; }

        bool CloseCurrentProject();

        void ReopenCurrentProject();


        bool NewProject(string projectsPath, string projectName, string projectDescription, string projectVersion);


        Task OpenProject(string projectConfigFilePath);

        bool IsAlreadyOpened(string projectConfigFilePath);

        bool SaveCurrentProjectJson();


        void UpdateCurrentProjectConfigFilePath(string projectConfigFilePath);

        string GetCurrentProjectDependencyVersionById(string id);
    }
}
