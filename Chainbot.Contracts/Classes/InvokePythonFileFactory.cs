using System;
using System.Activities;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Contracts.Classes
{
    public class InvokePythonFileFactory : IActivityTemplateFactory
    {
        public static string FilePath { get; set; }

        public Activity Create(DependencyObject target)
        {
            var type = Type.GetType(ConstantAssemblyQualifiedName.InvokePythonFileActivity);
            dynamic activity = Activator.CreateInstance(type);
            activity.SetPythonFilePath(FilePath);

            return activity;
        }
    }
}
