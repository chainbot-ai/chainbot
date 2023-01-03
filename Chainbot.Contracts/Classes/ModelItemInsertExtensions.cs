using Chainbot.Contracts.Classes;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Shared.Extensions
{
    public abstract class ModelItemContainerInfo
    {
        public abstract ModelItemCollection GetCollection();

        public const string PropertyName = "ContainerInfo";
    }

    public static class ModelItemInsertExtensions
    {
        public static ModelItem GetTargetInsertSequence(this ModelItem target, out int insertIndex, ModelItem suggestedChildSibling)
        {
            ModelItem result = null;
            insertIndex = -1;
            ModelProperty selectedActivityContainerProperty = target.GetSelectedActivityContainerProperty(suggestedChildSibling);
            if (selectedActivityContainerProperty != null)
            {
                result = GetTargetInsertSequence(selectedActivityContainerProperty, out insertIndex);
            }
            else if (target.IsSequence())
            {
                result = target;
                insertIndex = ((ModelItemCollection)target.Properties["Activities"].Value).Count;
            }
            else
            {
                ModelItem flowchartInsertContainer = target.GetFlowchartInsertContainer();
                if (flowchartInsertContainer != null)
                {
                    insertIndex = 0;
                    return flowchartInsertContainer;
                }
                if (target.IsStateMachine())
                {
                    result = target.AddActivity(new State
                    {
                        Entry = new Sequence()
                    }, -1).Properties["Entry"].Value;
                    insertIndex = 0;
                }
                else if (target.IsState())
                {
                    ModelItem parent = target.Parent;
                    ModelItem modelItem = (parent != null) ? parent.Parent : null;
                    if (modelItem.IsStateMachine())
                    {
                        result = modelItem.AddActivity(new State
                        {
                            Entry = new Sequence()
                        }, -1).Properties["Entry"].Value;
                        insertIndex = 0;
                    }
                }
            }
            return result;
        }

        private static ModelItemCollection GetActivityContainerCollection(ModelItem modelItem)
        {
            DependencyObject view = modelItem.View;
            PropertyInfo propertyInfo = (view != null) ? view.GetType().GetProperty("ContainerInfo") : null;
            ModelItemContainerInfo modelItemContainerInfo = ((propertyInfo != null) ? propertyInfo.GetValue(modelItem.View) : null) as ModelItemContainerInfo;
            if (modelItemContainerInfo == null)
            {
                return null;
            }
            return modelItemContainerInfo.GetCollection();
        }

        public static bool IsActivitiesContainer(this ModelItem modelItem)
        {
            bool result;
            try
            {
                result = (GetActivityContainerCollection(modelItem) != null);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static bool IsIfActivity(this ModelItem modelItem)
        {
            Type left;
            if (modelItem == null)
            {
                left = null;
            }
            else
            {
                object currentValue = modelItem.GetCurrentValue();
                left = ((currentValue != null) ? currentValue.GetType() : null);
            }
            return left == typeof(If);
        }

        private static bool IsParentInsertContainer(this ModelItem targetItemParent)
        {
            return targetItemParent.HasBody() || targetItemParent.IsIfActivity() || targetItemParent.IsSwitchDefaultOrCaseActivity() || targetItemParent.IsActivitiesContainer() || targetItemParent.IsSequence() || targetItemParent.IsFlowchart() || targetItemParent.IsStateMachine();
        }


        public static ModelItem GetParentInsertContainer(this ModelItem targetItem)
        {
            ModelItem modelItem;
            for (modelItem = targetItem; modelItem != null; modelItem = modelItem.Parent)
            {
                ModelItem parent = modelItem.Parent;
                if (parent != null && parent.IsParentInsertContainer())
                {
                    return parent;
                }
                foreach (ModelItem modelItem2 in modelItem.Parents)
                {
                    if (modelItem2.IsParentInsertContainer())
                    {
                        return modelItem2;
                    }
                }
            }
            return modelItem;
        }

        private static ModelItem GetFlowchartInsertContainer(this ModelItem target)
        {
            //if (target.IsFlowchart())
            //{
            //    return target.AddActivity(new Sequence(), -1).Properties["Action"].Value;
            //}

            ModelProperty modelProperty = null;
            ModelItem modelItem = null;
            ModelItem parent = target.Parent;
            if (parent != null && parent.IsFlowStep())
            {
                ModelItem parent2 = parent.Parent;
                modelItem = ((parent2 != null) ? parent2.Parent : null);
                if (modelItem.IsFlowchart())
                {
                    modelProperty = parent.Properties["Next"];
                }
            }
            else if (target.IsFlowDecision())
            {
                ModelItem parent3 = target.Parent;
                modelItem = ((parent3 != null) ? parent3.Parent : null);
                if (modelItem.IsFlowchart())
                {
                    modelProperty = target.Properties["True"];
                }
            }
            else if (target.IsFlowSwitch())
            {
                ModelItem parent4 = target.Parent;
                modelItem = ((parent4 != null) ? parent4.Parent : null);
                if (modelItem.IsFlowchart())
                {
                    modelProperty = target.Properties["Default"];
                }
            }
            if (modelProperty != null && modelItem != null && modelItem.IsFlowchart())
            {
                ModelItem modelItem2 = modelItem.AddActivity(new Sequence(), -1);
                ModelItem value = modelProperty.Value;
                modelProperty.SetValue(modelItem2);
                modelItem2.Properties["Next"].SetValue(value);
                return modelItem2.Properties["Action"].Value;
            }
            return null;
        }

        private static ModelItem GetTargetInsertSequence(ModelProperty containerProperty, out int insertIndex)
        {
            insertIndex = -1;
            if (containerProperty == null)
            {
                return null;
            }
            ModelItem modelItem = containerProperty.Value;
            if (modelItem == null)
            {
                modelItem = containerProperty.SetValue(new Sequence());
                insertIndex = 0;
            }
            else if (modelItem.IsSequence())
            {
                insertIndex = modelItem.Properties["Activities"].Collection.Count;
            }
            else
            {
                ModelItem item = modelItem;
                modelItem = containerProperty.SetValue(new Sequence());
                ((ModelItemCollection)modelItem.Properties["Activities"].Value).Add(item);
                insertIndex = 1;
            }
            return modelItem;
        }

        public static bool IsActivityActionType(this Type type)
        {
            return !(type == null) && (type == typeof(ActivityAction) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActivityAction<>)) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActivityAction<,>)));
        }

        public static bool HasBody(this ModelItem modelItem)
        {
            ModelProperty modelProperty = (modelItem != null) ? modelItem.Properties["Body"] : null;
            return ((modelProperty != null) ? modelProperty.PropertyType : null).IsActivityActionType();
        }

        private static ModelProperty GetSelectedActivityContainerProperty(this ModelItem containerParent, ModelItem selectedActivity)
        {
            ModelProperty[] array = containerParent.GetContainerActivityProperties().ToArray<ModelProperty>();
            foreach (ModelProperty modelProperty in array)
            {
                if(selectedActivity != null)
                {
                    if (modelProperty.Value == selectedActivity || modelProperty.Value.IsParentOf(selectedActivity))
                    {
                        return modelProperty;
                    }
                }
            }
            return array.FirstOrDefault<ModelProperty>();
        }

        public static bool IsGenericTypeEx(this Type type)
        {
            return type.IsGenericType;
        }

        public static Type[] GetGenericArgumentsEx(this Type type)
        {
            return type.GetGenericArguments();
        }

        public static Type[] GetInterfacesEx(this Type type)
        {
            return type.GetInterfaces();
        }

        public static Type GetBaseTypeEx(this Type type)
        {
            return type.BaseType;
        }

        public static bool IsSubclassOfRawGeneric(this Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type type = toCheck.IsGenericTypeEx() ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic.IsInterface)
                {
                    using (IEnumerator<Type> enumerator = (from t in type.GetInterfacesEx()
                                                           where t.IsGenericType
                                                           select t).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Type type2 = enumerator.Current;
                            if (generic == type2.GetGenericTypeDefinition())
                            {
                                return true;
                            }
                        }
                        goto IL_9F;
                    }
                }
                goto IL_94;
                IL_9F:
                toCheck = toCheck.GetBaseTypeEx();
                continue;
                IL_94:
                if (generic == type)
                {
                    return true;
                }
                goto IL_9F;
            }
            return false;
        }

        public static bool IsSwitchDefaultOrCaseActivity(this ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return false;
            }
            Type type;
            if (modelItem == null)
            {
                type = null;
            }
            else
            {
                object currentValue = modelItem.GetCurrentValue();
                type = ((currentValue != null) ? currentValue.GetType() : null);
            }
            Type type2 = type;
            if (type2 == null || !type2.IsGenericType)
            {
                return false;
            }
            if (type2.GetGenericTypeDefinition() == typeof(Switch<>))
            {
                return true;
            }
            if (type2.GetInterfaces().Any((Type i) => i.IsSubclassOfRawGeneric(typeof(IDictionary<,>))))
            {
                Type type3;
                if (modelItem == null)
                {
                    type3 = null;
                }
                else
                {
                    ModelItem parent = modelItem.Parent;
                    if (parent == null)
                    {
                        type3 = null;
                    }
                    else
                    {
                        object currentValue2 = parent.GetCurrentValue();
                        type3 = ((currentValue2 != null) ? currentValue2.GetType() : null);
                    }
                }
                type2 = type3;
                if (type2 == null || !type2.IsGenericType)
                {
                    return false;
                }
            }
            return type2.GetGenericTypeDefinition() == typeof(Switch<>);
        }

        private static IEnumerable<ModelProperty> GetContainerActivityProperties(this ModelItem target)
        {
            if (target.ItemType == typeof(ActivityBuilder))
            {
                yield return target.Properties["Implementation"];
            }
            else if (target.ItemType == typeof(If))
            {
                yield return target.Properties["Then"];
                yield return target.Properties["Else"];
            }
            else if (target.IsSwitchDefaultOrCaseActivity())
            {
                ModelProperty modelProperty = target.Properties.Find("Default");
                if (modelProperty != null)
                {
                    yield return modelProperty;
                }
                if (target is ModelItemDictionary)
                {
                    ModelProperty modelProperty2 = target.Properties.Find("ItemsCollection");
                    if (modelProperty2 != null)
                    {
                        ModelItemCollection modelItemCollection = modelProperty2.Value as ModelItemCollection;
                        if (modelItemCollection != null)
                        {
                            foreach (ModelItem modelItem in modelItemCollection)
                            {
                                ModelProperty modelProperty3 = modelItem.Properties.Find("Value");
                                if (modelProperty3 != null)
                                {
                                    yield return modelProperty3;
                                }
                            }
                        }
                    }
                }
            }
            else if (target.HasBody())
            {
                ModelProperty modelProperty4 = target.Properties["Body"];
                ModelProperty modelProperty5;
                if (modelProperty4 == null)
                {
                    modelProperty5 = null;
                }
                else
                {
                    ModelItem value = modelProperty4.Value;
                    modelProperty5 = ((value != null) ? value.Properties["Handler"] : null);
                }
                yield return modelProperty5;
            }
            yield break;
        }


    }
}
