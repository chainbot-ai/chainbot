using Chainbot.Contracts.Classes;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;

namespace Chainbot.Cores.Classes
{
    public static class ModelItemHelper
    {
        public static void RemoveVariableFromParent(ModelItem variable)
        {
            if (variable != null)
            {
                ModelItem parent = variable.Parent;
                if (parent == null)
                {
                    return;
                }
                ModelItem parent2 = parent.Parent;
                if (parent2 == null)
                {
                    return;
                }
                ModelItemCollection variableCollection = parent2.GetVariableCollection();
                if (variableCollection == null)
                {
                    return;
                }
                variableCollection.Remove(variable);
            }
        }

        public static IEnumerable<ModelItem> GetActivityDelegateArguments(ModelItem modelItem)
        {
            if (!(modelItem.GetCurrentValue() is ActivityDelegate))
            {
                return Enumerable.Empty<ModelItem>();
            }
            return modelItem.Properties.Where<ModelProperty>((ModelProperty p) => {
                if (!typeof(DelegateArgument).IsAssignableFrom(p.PropertyType))
                {
                    return false;
                }
                return p.Value != null;
            }).Select<ModelProperty, ModelItem>((ModelProperty p) => p.Value);
        }

        public static IEnumerable<ModelItem> GetVariablesAndArguments(ModelItem modelItem)
        {
            List <ModelItem> list;
            if (modelItem == null)
            {
                return Enumerable.Empty<ModelItem>();
            }
            ModelItemCollection variableCollection = modelItem.GetVariableCollection();
            if (variableCollection != null)
            {
                list = variableCollection.ToList<ModelItem>();
            }
            else
            {
                list = null;
            }
            if (list == null)
            {
                list = new List<ModelItem>();
            }
            list.AddRange(GetActivityDelegateArguments(modelItem));
            return list;
        }

        public static IEnumerable<ModelItem> GetProperties(ModelItem modelItem)
        {
            IEnumerable<ModelItem> enumerable;
            if (modelItem == null)
            {
                enumerable = null;
            }
            else
            {
                ModelProperty modelProperty = modelItem.Properties["Properties"];
                enumerable = ((modelProperty != null) ? modelProperty.Collection : null);
            }
            IEnumerable<ModelItem> enumerable2 = enumerable;
            return enumerable2 ?? Enumerable.Empty<ModelItem>();
        }

        private static bool IsVariableAvailableInContext(IEnumerable<string> variablesNamesInContext, ModelItem newVariable)
        {
            string standardizedVariableName = newVariable.GetStandardizedVariableName();
            if (string.IsNullOrEmpty(standardizedVariableName))
            {
                return false;
            }
            return !variablesNamesInContext.Contains<string>(standardizedVariableName);
        }

        public static string GetVariableName(this ModelItem modelItem)
        {
            return (modelItem != null ? modelItem.Properties["Name"].ComputedValue : null) as string;
        }

        public static string GetStandardizedVariableName(this ModelItem modelItem)
        {
            string variableName = modelItem.GetVariableName();
            if (variableName != null)
            {
                return variableName.ToUpperInvariant();
            }
            return null;
        }

        public static IEnumerable<ModelItem> GetVariablesInScope(ModelItem modelItem)
        {
            Func<ModelItem, bool> func = null;
            if (modelItem == null)
            {
                return Enumerable.Empty<ModelItem>();
            }
            List<ModelItem> modelItems = new List<ModelItem>();
            HashSet<string> strs = new HashSet<string>();
            while (modelItem != null)
            {
                IEnumerable<ModelItem> variablesAndArguments = ModelItemHelper.GetVariablesAndArguments(modelItem);
                Func<ModelItem, bool> func1 = func;
                if (func1 == null)
                {
                    Func<ModelItem, bool> func2 = (ModelItem o) => ModelItemHelper.IsVariableAvailableInContext(strs, o);
                    Func<ModelItem, bool> func3 = func2;
                    func = func2;
                    func1 = func3;
                }
                IEnumerable<ModelItem> modelItems1 = variablesAndArguments.Where<ModelItem>(func1);
                modelItems.AddRange(modelItems1);
                foreach (ModelItem modelItem1 in modelItems1)
                {
                    strs.Add(modelItem1.GetStandardizedVariableName());
                }
                modelItem = modelItem.Parent;
            }
            return modelItems;
        }
    }
}
