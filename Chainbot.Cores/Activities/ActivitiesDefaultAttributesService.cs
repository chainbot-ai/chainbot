using Chainbot.Contracts.Activities;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Statements;
using System.ComponentModel;

namespace Chainbot.Cores.Activities
{
    public class ActivitiesDefaultAttributesService : IActivitiesDefaultAttributesService
    {
        public ActivitiesDefaultAttributesService()
        {
            new DesignerMetadata().Register();
        }

        public void Register()
        {
            AttributeTableBuilder attributeTableBuilder = new AttributeTableBuilder();

            AddDefaultNameAttribute(attributeTableBuilder);

            AddGenericTypeEditor(attributeTableBuilder);
           
            MetadataStore.AddAttributeTable(attributeTableBuilder.CreateTable());
        }

        private void AddDefaultNameAttribute(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(ActivityBuilder), "ImplementationVersion", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ActivityBuilderImplementationVersion),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(ActivityBuilder), "Name", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ActivityBuilderName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(Activity<>), "Result", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.Result),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.OutputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Activity), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowDecision), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowSwitch<>), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(PickBranch), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(Transition), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(Transition), "Condition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ConditionPropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(State), "DisplayName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DisplayNamePropertyName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(AddToCollection<>), "Collection", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(AddToCollection<>), "Item", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ItemDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ClearCollection<>), "Collection", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), "Collection", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), "Item", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ItemDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), "Result", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionResultDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.OutputCategoryName)
            });
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), "Collection", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), "Item", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ItemDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), "Result", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.CollectionResultDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.OutputCategoryName)
            });
            builder.AddCustomAttributes(typeof(WriteLine), "Text", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.WriteLineTextDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(WriteLine), "TextWriter", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.WriteLineTextWriterDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "GenericTypeArguments", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.GenericTypeArgumentsDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "MethodName", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.MethodNameDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "Parameters", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ParametersDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "Result", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.InvokeMethodResultDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.OutputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "RunAsynchronously", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.RunAsynchronouslyDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "TargetObject", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.TargetObjectDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(InvokeMethod), "TargetType", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.TargetTypeDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowDecision), "Condition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.FlowDecisionConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowDecision), "True", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.FlowDecisionTrueDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowDecision), "False", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.FlowDecisionFalseDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(FlowSwitch<>), "Expression", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.FlowDecisionExpressionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Flowchart), "ValidateUnconnectedNodes", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.FlowchartValidateUnconnectedNodesDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.BasicCategoryName)
            });
            builder.AddCustomAttributes(typeof(TerminateWorkflow), "Exception", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.TerminateWorkflowExceptionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(TerminateWorkflow), "Reason", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.TerminateWorkflowReasonDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Throw), "Exception", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ThrowExceptionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Assign), "To", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.AssignToDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Assign), "Value", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.AssignValueDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Delay), "Duration", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DelayDurationDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });

            builder.AddCustomAttributes(typeof(While), "Condition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.WhileConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(DoWhile), "Condition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.DoWhileConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(If), "Condition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.IfConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Parallel), "CompletionCondition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ParallelCompletionConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ParallelForEach<>), "CompletionCondition", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ParallelForEachCompletionConditionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(ParallelForEach<>), "Values", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.ParallelForEachValuesDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
            builder.AddCustomAttributes(typeof(Switch<>), "Expression", new Attribute[]
            {
                new DisplayNameAttribute(Chainbot.Resources.Properties.Resources.SwitchExpressionDisplayName),
                new CategoryAttribute(Chainbot.Resources.Properties.Resources.InputCategoryName)
            });
        }

        private void AddGenericTypeEditor(AttributeTableBuilder builder)
        {
            Type type = Type.GetType("System.Activities.Presentation.FeatureAttribute, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type type2 = Type.GetType("System.Activities.Presentation.UpdatableGenericArgumentsFeature, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Attribute attribute = Activator.CreateInstance(type, new object[]
            {
                type2
            }) as Attribute;
            builder.AddCustomAttributes(typeof(FlowSwitch<>), new Attribute[]
            {
                attribute
            });

            builder.AddCustomAttributes(typeof(Switch<>), new Attribute[]
            {
                attribute
            });
            builder.AddCustomAttributes(typeof(AddToCollection<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(object))
            });
            builder.AddCustomAttributes(typeof(ClearCollection<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(object))
            });
            builder.AddCustomAttributes(typeof(ExistsInCollection<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(object))
            });
            builder.AddCustomAttributes(typeof(RemoveFromCollection<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(object))
            });

            builder.AddCustomAttributes(typeof(Switch<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(string))
            });
            builder.AddCustomAttributes(typeof(FlowSwitch<>), new Attribute[]
            {
                new DefaultTypeArgumentAttribute(typeof(int))
            });
        }

    }
}
