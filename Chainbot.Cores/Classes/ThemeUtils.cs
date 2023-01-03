using ActiproSoftware.Windows.Controls.Ribbon;
using ActiproSoftware.Windows.Themes;
using Chainbot.Contracts.Classes;
using Chainbot.Cores.ExpressionEditor;
using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Chainbot.Cores.Classes
{
    public class ThemeUtils
    {
        public static void AddThemeWindowLoadedEvent()
        {
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoadedTheme));
        }

        private static void OnChromeWindowClosed(object sender, EventArgs e)
        {
            Window window = sender as Window;
            window.Closed -= OnChromeWindowClosed;
            try
            {
                WindowChrome.SetChrome(window, null);
            }
            catch (Exception exception)
            {
                exception.Trace("OnChromeWindowClosed");
            }
        }

        private static void AttachWindowClosedEvent(Window window)
        {
            window.Closed += OnChromeWindowClosed;
        }

        private static DynamicArgumentDialog GetArgumentsDialog(Window window)
        {
            if (((window != null) ? window.GetType().FullName : null) != "System.Activities.Presentation.WorkflowElementDialogWindow")
            {
                return null;
            }
            DockPanel dockPanel = ((window != null) ? window.Content : null) as DockPanel;
            if (dockPanel != null)
            {
                if (dockPanel != null)
                {
                    UIElementCollection children = dockPanel.Children;
                    int? num = (children != null) ? new int?(children.Count) : null;
                    int num2 = 2;
                    if (num.GetValueOrDefault() < num2 & num != null)
                    {
                        return null;
                    }
                }
                Border border = dockPanel.Children[1] as Border;
                return ((border != null) ? border.Child : null) as DynamicArgumentDialog;
            }

            return null;
        }

        private static bool TryApplyArgumentsDialogStyle(Window window)
        {
            if (window == null)
            {
                return false;
            }
            try
            {
                DynamicArgumentDialog argumentsDialog = GetArgumentsDialog(window);
                if (argumentsDialog != null)
                {
                    window.ResizeMode = ResizeMode.CanResize;
                    UserControl userControl = argumentsDialog.Content as UserControl;
                    DataGrid dataGrid = ((userControl != null) ? userControl.FindName("WPF_DataGrid") : null) as DataGrid;
                    if (dataGrid != null)
                    {
                        dataGrid.ApplyThemedStyle();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            return false;
        }

        private static bool CheckExistingWindowChrome(object sender)
        {
            try
            {
                Window window = sender as Window;
                if (window != null && (window is RibbonWindow || WindowChrome.GetChrome(window) != null))
                {
                    AttachWindowClosedEvent(window);
                    Application.Current.Dispatcher.Invoke(delegate ()
                    {
                    }, DispatcherPriority.DataBind);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            return false;
        }

        private static void OnWindowLoadedTheme(object sender, RoutedEventArgs e)
        {
            try
            {
                Window window = sender as Window;
                if (window != null)
                {
                    if (!CheckExistingWindowChrome(sender))
                    {
                        if (!window.GetType().FullName.StartsWith("ActiproSoftware") || !(window.GetType().Name == "WindowChromeShadow"))
                        {
                            if (window.Foreground != Brushes.Transparent)
                            {
                                window.Foreground = (Application.Current.TryFindResource(AssetResourceKeys.WindowForegroundNormalBrushKey) as SolidColorBrush);
                            }
                            if (window.Background != Brushes.Transparent && window.Background != null)
                            {
                                window.Background = (Application.Current.TryFindResource(AssetResourceKeys.WindowBackgroundNormalBrushKey) as SolidColorBrush);
                                if (window.GetType().Assembly.FullName.Contains("System.Activities.Presentation") && !TryApplyArgumentsDialogStyle(window))
                                {
                                    if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
                                    {
                                    window.Resources.MergedDictionaries.Add(new ResourceDictionary
                                    {
                                            Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/SystemColors.Dark.xaml", UriKind.Absolute)
                                    });
                                }
                                    else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
                                    {
                                        window.Resources.MergedDictionaries.Add(new ResourceDictionary
                                        {
                                            Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/SystemColors.Light.xaml", UriKind.Absolute)
                                        });
                                    }
                                }
                                WindowChrome.SetChrome(window, new WindowChrome());
                                AttachWindowClosedEvent(window);
                                Application.Current.Dispatcher.Invoke(delegate ()
                                {
                                }, DispatcherPriority.Loaded);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }
}
