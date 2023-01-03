using System.ComponentModel;
using System.Resources;

namespace Plugins.Shared.Library
{
    internal class Localize
    {
        internal class LocalizedCategoryAttribute : CategoryAttribute
        {
            public LocalizedCategoryAttribute(string resourceKey)
                : base(LocalizedResources.GetString(resourceKey)) { }
        }

        internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            public LocalizedDisplayNameAttribute(string resourceKey)
                : base(LocalizedResources.GetString(resourceKey)) { }
        }

        internal class LocalizedDescriptionAttribute : DescriptionAttribute
        {
            public LocalizedDescriptionAttribute(string resourceKey)
                : base(LocalizedResources.GetString(resourceKey)) { }
        }

        internal static class LocalizedResources
        {
            readonly static ResourceManager _ResourceManager = new ResourceManager(typeof(Properties.Resources));

            public static string GetString(string resourceKey)
            {
                return _ResourceManager.GetString(resourceKey) ?? resourceKey;
            }
        }

    }
}
