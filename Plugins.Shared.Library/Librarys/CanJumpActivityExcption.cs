using System;
using System.Activities;

namespace Plugins.Shared.Library.Librarys
{
    public class CanJumpActivityExcption : Exception
    {
        public Activity Activity { get; set; }

        public string WorkFilePath { get; set; }

        public CanJumpActivityExcption(Activity activity, Exception exception, string filePath = null) : base(exception.Message, exception)
        {
            Activity = activity;
            WorkFilePath = filePath;
        }

        public override Exception GetBaseException()
        {
            var inner = this;
            while (inner.InnerException as CanJumpActivityExcption != null)
            {
                inner = inner.InnerException as CanJumpActivityExcption;
            }
            return inner ?? this;
        }
    }
}
