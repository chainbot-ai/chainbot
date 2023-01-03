using System.Activities;
using System.Reflection;

namespace Chainbot.Cores.Classes
{
    public static class ActivityExtensions
    {
        private static PropertyInfo _activityParentProperty;

        private static PropertyInfo ActivityParentProperty
        {
            get
            {
                PropertyInfo result;
                if ((result = _activityParentProperty) == null)
                {
                    result = (_activityParentProperty = typeof(Activity).GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic));
                }
                return result;
            }
        }


        public static Activity GetParent(this Activity activity)
        {
            return ActivityParentProperty.GetValue(activity, null) as Activity;
        }
    }
}
