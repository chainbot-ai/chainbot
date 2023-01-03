﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Plugins.Shared.Library.Extensions
{
    public static class SystemExtensions
    {
        private static readonly string quot_token = "{DEEDD22F-D466-4257-A5DC-E0341E57E63F}";
        private static readonly string apos_token = "{6322B96D-D9C1-453E-8A09-8D4C73458A11}";

        private class Comparer<T> : IComparer<T>
        {
            private Func<T, T, int> _compare;

            public Comparer(Func<T, T, int> comparison)
            {
                _compare = comparison;
            }

            public int Compare(T x, T y)
            {
                return _compare(x, y);
            }
        }

        private class Disposable : IDisposable
        {
            public static Disposable Empty = new Disposable(null)
            {
                _disposed = true
            };

            private Action _onDispose;

            private bool _disposed;

            public Disposable(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _onDispose?.Invoke();
                    _disposed = true;
                }
            }
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static IComparer<T> ToComparer<T>(this Func<T, T, int> compare)
        {
            return new Comparer<T>(compare);
        }

        public static KeyValuePair<TKey, TValue> ToKeyValue<TKey, TValue>(this TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static IDisposable DisposeWith<T>(this T obj, Action<T> onDispose, out T outObj)
        {
            outObj = obj;
            return obj.DisposeWith(onDispose);
        }

        public static IDisposable DisposeWith<T>(this T obj, Action<T> onDispose)
        {
            if (obj == null)
            {
                return Disposable.Empty;
            }
            return new Disposable(delegate
            {
                onDispose(obj);
            });
        }

        public static IDisposable DisposeWithReleaseComObject<T>(this T obj, out T outObj)
        {
            return obj.DisposeWith(delegate
            {
                Marshal.ReleaseComObject(obj);
            }, out outObj);
        }

        public static IDisposable DisposeWithReleaseComObject<T>(this T obj)
        {
            return obj.DisposeWith(delegate
            {
                if (obj != null && Marshal.IsComObject(obj))
                {
                    Marshal.ReleaseComObject(obj);
                }
            });
        }

        public static async Task<T> RetryAsync<T>(this Func<Task<T>> func, Func<T, Exception, Task<bool>> retry, TimeSpan timeout, TimeSpan retryDelay)
        {
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                T result;
                Exception lastException;
                while (true)
                {
                    result = default(T);
                    lastException = null;
                    try
                    {
                        result = await func();
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                    if (watch.Elapsed > timeout || !(await retry(result, lastException)))
                    {
                        break;
                    }
                    await Task.Delay(retryDelay);
                }
                if (lastException != null)
                {
                    ExceptionDispatchInfo.Capture(lastException).Throw();
                }
                return result;
            }
            finally
            {
                watch.Stop();
            }
        }


        public static string WrapXmlWithDoubleQuotationMarks(this string rawXml)
        {
            var selectorXml = $"<Selector>{rawXml}</Selector>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(selectorXml);
            var rootNode = doc.DocumentElement;

            InitChildrenWithDoubleQuotationMarks(rootNode);

            var xml = rootNode.InnerXml;

            xml = xml.Replace(apos_token, "&apos;");
            xml = xml.Replace(quot_token, "&quot;");

            return xml.Replace("\"", "\'");
        }

        private static void InitChildrenWithDoubleQuotationMarks(XmlNode node)
        {
            if(node.NodeType == XmlNodeType.Text)
            {
                node.InnerText = node.InnerText.Replace("\'", apos_token);
                node.InnerText = node.InnerText.Replace("\"", quot_token);
            }

            foreach (var child in node.ChildNodes)
            {
                InitChildrenWithDoubleQuotationMarks(child as XmlNode);
            }
        }


    }
}
