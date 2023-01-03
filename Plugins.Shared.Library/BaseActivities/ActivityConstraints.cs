using System;
using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.BaseActivities
{
    public static class ActivityConstraints
    {
        public static Constraint HasParentType<TActivity, TParent>(string validationMessage) where TActivity : Activity where TParent : Activity
        {
            return HasParent<TActivity>((Activity p) => p as TParent != null, validationMessage);
        }

        public static Constraint HasParent<TActivity>(Func<Activity, bool> condition, string validationMessage) where TActivity : Activity
        {
            DelegateInArgument<TActivity> argument = new DelegateInArgument<TActivity>();
            DelegateInArgument<ValidationContext> delegateInArgument = new DelegateInArgument<ValidationContext>();
            Variable<bool> variable = new Variable<bool>();
            DelegateInArgument<Activity> parent = new DelegateInArgument<Activity>();
            Constraint<TActivity> constraint = new Constraint<TActivity>();
            constraint.Body = new ActivityAction<TActivity, ValidationContext>
            {
                Argument1 = argument,
                Argument2 = delegateInArgument,
                Handler = new Sequence
                {
                    Variables =
                {
                    (Variable)variable
                },
                    Activities =
                {
                    (Activity)new ForEach<Activity>
                    {
                        Values = new GetParentChain
                        {
                            ValidationContext = delegateInArgument
                        },
                        Body = new ActivityAction<Activity>
                        {
                            Argument = parent,
                            Handler = new If
                            {
                                Condition = new InArgument<bool>((ActivityContext ctx) => condition(parent.Get(ctx))),
                                Then = new Assign<bool>
                                {
                                    Value = true,
                                    To = variable
                                }
                            }
                        }
                    },
                    (Activity)new AssertValidation
                    {
                        Assertion = new InArgument<bool>(variable),
                        Message = new InArgument<string>(validationMessage)
                    }
                }
                }
            };
            return constraint;
        }
    }
}
