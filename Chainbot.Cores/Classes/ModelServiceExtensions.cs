using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;

namespace Chainbot.Cores.Classes
{
    public static class ModelServiceExtensions
    {
        public static IReadOnlyCollection<ModelItem> GetActivities(this ModelService modelService, Predicate<Type> predicate)
        {
            ModelItem root = modelService.Root;
            return modelService.Find(root, predicate).ToList<ModelItem>();
        }

        public static IEnumerable<ModelItem> GetVariables(this ModelService modelService)
        {
            return modelService.Find(modelService.Root, typeof(Variable));
        }

        public static List<ModelItem> GetActivities(this ModelService modelService, ModelItem startingPoint)
        {
            List<ModelItem> list = new List<ModelItem>();
            if (modelService.GetRootActivity() != null)
            {
                if (startingPoint == null)
                {
                    startingPoint = modelService.Root;
                }
                list.AddRange(modelService.Find(startingPoint, ModelServiceExtensions.AllActivitiesPredicate));
            }
            return list;
        }

        public static IReadOnlyCollection<ModelItem> GetActivities(this ModelService modelService)
        {
            return modelService.GetActivities(ModelServiceExtensions.AllActivitiesPredicate);
        }

        public static Predicate<Type> AllActivitiesPredicate
        {
            get
            {
                return (Type type) => (typeof(Activity).IsAssignableFrom(type) ^ (typeof(ITextExpression).IsAssignableFrom(type) || typeof(IValueSerializableExpression).IsAssignableFrom(type))) || typeof(FlowNode).Namespace == type.Namespace;
            }
        }

        public static Activity GetRootActivity(this ModelService modelService)
        {
            ModelItem root = modelService.Root;
            object obj = (root != null) ? root.GetCurrentValue() : null;
            ActivityBuilder activityBuilder = obj as ActivityBuilder;
            return ((activityBuilder != null) ? activityBuilder.Implementation : null) ?? (obj as Activity);
        }
        
        private static bool IsGenericVariableOrArgument(Type t)
        {
            if (!t.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = t.GetGenericTypeDefinition();
            return typeof(Variable<>).IsAssignableFrom(genericTypeDefinition) || typeof(InArgument<>).IsAssignableFrom(genericTypeDefinition) || typeof(OutArgument<>).IsAssignableFrom(genericTypeDefinition) || typeof(InOutArgument<>).IsAssignableFrom(genericTypeDefinition);
        }

        
    }
}

