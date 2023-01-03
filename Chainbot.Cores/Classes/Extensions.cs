using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Chainbot.Cores.Classes
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string text, string value,
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }

        public static bool EqualsIgnoreCase(this string text, string value)
        {
            return text.ToLower() == value.ToLower();
        }
    }

    public static class StringExtensions
    {

        public static bool IsSubPathOf(this string path, string baseDirPath)
        {
            string normalizedPath = System.IO.Path.GetFullPath(path.Replace('/', '\\')
                .WithEnding("\\"));

            string normalizedBaseDirPath = System.IO.Path.GetFullPath(baseDirPath.Replace('/', '\\')
                .WithEnding("\\"));

            return normalizedPath.StartsWith(normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase);
        }


        public static string WithEnding(this string str, string ending)
        {
            if (str == null)
                return ending;

            string result = str;


            for (int i = 0; i <= ending.Length; i++)
            {
                string tmp = result + ending.Right(i);
                if (tmp.EndsWith(ending))
                    return tmp;
            }

            return result;
        }


        public static string Right(this string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
            }

            return (length < value.Length) ? value.Substring(value.Length - length) : value;
        }
    }


    public static class EnumerablePolyfills
    {
        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToHashSet<TSource>(null);
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new HashSet<TSource>(source, comparer);
        }
    }

    public static class ObjectExtensions
    {
        public static R TryGet<T, R>(this T target, Func<T, R> getter)
        where T : class
        where R : class
        {
            if (target != null)
            {
                return getter(target);
            }
            return default(R);
        }
    }



    public static class DependencyObjectExtensions
    {
        public static T GetParent<T>(this DependencyObject dependencyObject)
        where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return default(T);
            }
            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
            T t = (T)(parent as T);
            if (t != null)
            {
                return t;
            }
            return parent.GetParent<T>();
        }
    }



    public static class WindowExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        public static double ActualTop(this Window window)
        {
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    return window.Top;
                case WindowState.Minimized:
                    return window.RestoreBounds.Top;
                case WindowState.Maximized:
                    {
                        RECT rect;
                        GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
                        return rect.Top;
                    }
            }
            return 0;
        }
        public static double ActualLeft(this Window window)
        {
            switch (window.WindowState)
            {
                case WindowState.Normal:
                    return window.Left;
                case WindowState.Minimized:
                    return window.RestoreBounds.Left;
                case WindowState.Maximized:
                    {
                        RECT rect;
                        GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);
                        return rect.Left;
                    }
            }
            return 0;
        }
    }


}
