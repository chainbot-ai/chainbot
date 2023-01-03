using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Extensions
{
    public static class ActivityContextExtensions
    {
        public static Activity GetRootActivity(this ActivityContext context)
        {
            IWorkflowRuntime workflowRuntime = context.GetWorkflowRuntime();
            if (workflowRuntime == null)
            {
                return null;
            }
            return workflowRuntime.GetRootActivity();
        }

        

        public static void CreateAndSetCancellationTokenSource(this AsyncCodeActivityContext context)
        {
            CancellationTokenSource userState = new CancellationTokenSource();
            context.UserState = userState;
        }


        public static CancellationTokenSource GetCTS(this AsyncCodeActivityContext context)
        {
            if (context.UserState is CancellationTokenSource)
            {
                return context.UserState as CancellationTokenSource;
            }
            return null;
        }


        public static CancellationToken GetCT(this AsyncCodeActivityContext context)
        {
            CancellationTokenSource cts = context.GetCTS();
            if (cts == null)
            {
                return default(CancellationToken);
            }
            return cts.Token;
        }

        public static IWorkflowRuntime GetWorkflowRuntime(this ActivityContext context)
        {
            return context.GetExtension<IWorkflowRuntime>();
        }

        public static void SetOutArgument(this ActivityContext context, OutArgument outArgument, object value)
        {
            if (outArgument == null)
            {
                return;
            }
            try
            {
                outArgument.Set(context, value);
            }
            catch (InvalidOperationException)
            {
                outArgument.Set(context, TypeDescriptor.GetConverter(outArgument.ArgumentType).ConvertFrom(value));
            }
        }

        public static T GetValueOrDefault<T>(this ActivityContext context, InArgument<T> arg, string settingKey, T defaultValue)
        {
            return context.GetValueOrDefault(arg, settingKey, (T a) => a, defaultValue);
        }

        public static TArg GetValueOrDefault<TArg, TSetting>(this ActivityContext context, InArgument<TArg> arg, string settingKey, Func<TSetting, TArg> selector, TArg defaultValue)
        {
            if (((arg != null) ? arg.Expression : null) != null)
            {
                return arg.Get(context);
            }
            //TSetting tsetting;
            //if (ActivityContextExtensions.TryGetValue<TSetting>(context, settingKey, out tsetting) && tsetting != null)
            //{
            //    return selector(tsetting);
            //}
            return defaultValue;
        }

        public static T GetValueOrDefault<T>(this ActivityContext context, InArgument<T> source, T defaultValue)
        {
            T result = defaultValue;
            if (source != null && source.Expression != null)
            {
                result = source.Get(context);
            }
            return result;
        }

        public static T GetValueOrDefault<T>(this ActivityContext context, InArgument<T> source)
        {
            return context.GetValueOrDefault(source, default(T));
        }

    }
}
