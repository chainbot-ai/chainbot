using ActiproSoftware.Windows.Themes;
using Chainbot.Cores.ExpressionEditor;
using System;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReflectionMagic;
using GalaSoft.MvvmLight.CommandWpf;
using Chainbot.Contracts.Classes;

namespace Chainbot.Cores.Classes
{
    public class DesignerThemeBehavior
    {
        private WorkflowDesigner _designer;

        private DataGrid _importedNamespacesDataGrid;
        private ModelItemCollection _importsModelItem;
        private ModelService _modelService;
        private readonly HashSet<string> _addedNamespaces = new HashSet<string>();
        private EditingContext _editingContext;

        public void Attach(WorkflowDesigner designer)
        {
            this.Dettach();
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            this._designer = designer;
            this.ChangeDesignerFontColor();
        }

        public void Dettach()
        {
            this.UsubscribeImportsControlEvents();
            this._designer = null;
        }
        public void Dispose()
        {
            this.Dettach();
        }

        private bool IsDarkTheme()
        {
            return true;
        }

        private void ChangeDesignerFontColor()
        {
            if (this._designer == null || !(this._designer.View is Grid))
            {
                return;
            }
            DesignerView service = this._designer.Context.Services.GetService<DesignerView>();
            if (service == null)
            {
                return;
            }
            ResourceDictionary resourceDictionary = new ResourceDictionary();

            if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Dark)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/SystemColors.Dark.xaml");
            }
            else if (GlobalConfig.CurrentTheme == GlobalConfig.enTheme.Light)
            {
                resourceDictionary.Source = new Uri("pack://application:,,,/Chainbot.Resources;component/WorkflowDesigner/SystemColors.Light.xaml");
            }

            if (IsDarkTheme())
            {
                service.Resources.MergedDictionaries.Add(resourceDictionary);
                service.Foreground = (Brush)resourceDictionary[SystemColors.ControlTextBrushKey];
                ((Grid)this._designer.PropertyInspectorView).Resources.MergedDictionaries.Add(resourceDictionary);
                ComboBox comboBox = service.FindName("zoomPicker") as ComboBox;
                if (comboBox != null)
                {
                    Style basedOn = Application.Current.TryFindResource(SharedResourceKeys.ComboBoxStyleKey) as Style;
                    comboBox.Style = StyleHelper.BaseStyleOn(comboBox.Style, basedOn, typeof(ComboBox));
                }
            }
            ContentControl contentControl = service.FindName("variables1") as ContentControl;
            DataGrid dataGrid = ((contentControl != null) ? contentControl.FindName("variableDataGrid") : null) as DataGrid;
            if (dataGrid != null)
            {
                dataGrid.ApplyThemedStyle();
            }
            ContentControl contentControl2 = service.FindName("imports1") as ContentControl;
            DataGrid dataGrid2 = ((contentControl2 != null) ? contentControl2.FindName("importedNamespacesDataGrid") : null) as DataGrid;
            if (dataGrid2 != null)
            {
                Style rowStyle = dataGrid2.RowStyle;
                if (rowStyle != null)
                {
                    TriggerCollection triggers = rowStyle.Triggers;
                    if (triggers != null)
                    {
                        triggers.Clear();
                    }
                }
            }
            Trigger trigger;
            if (dataGrid2 == null)
            {
                trigger = null;
            }
            else
            {
                Style cellStyle = dataGrid2.CellStyle;
                if (cellStyle == null)
                {
                    trigger = null;
                }
                else
                {
                    TriggerCollection triggers2 = cellStyle.Triggers;
                    if (triggers2 == null)
                    {
                        trigger = null;
                    }
                    else
                    {
                        IEnumerable<Trigger> enumerable = triggers2.OfType<Trigger>();
                        if (enumerable == null)
                        {
                            trigger = null;
                        }
                        else
                        {
                            trigger = enumerable.FirstOrDefault((Trigger t) => t.Property == DataGridCell.IsSelectedProperty);
                        }
                    }
                }
            }
            Trigger trigger2 = trigger;
            if (trigger2 != null && dataGrid2 != null)
            {
                Style cellStyle2 = dataGrid2.CellStyle;
                if (cellStyle2 != null)
                {
                    TriggerCollection triggers3 = cellStyle2.Triggers;
                    if (triggers3 != null)
                    {
                        triggers3.Remove(trigger2);
                    }
                }
            }
            if (dataGrid2 != null)
            {
                dataGrid2.ApplyThemedStyle();
            }
            ComboBox comboBox2 = ((contentControl2 != null) ? contentControl2.FindName("inputComboBox") : null) as ComboBox;
            if (comboBox2 != null)
            {
                Style basedOn = Application.Current.TryFindResource(SharedResourceKeys.ComboBoxStyleKey) as Style;
                comboBox2.Style = StyleHelper.BaseStyleOn(comboBox2.Style, basedOn, typeof(ComboBox));

                comboBox2.Loaded += this.ImportsComboBox_Loaded;
            }
            ContentControl contentControl3 = service.FindName("arguments1") as ContentControl;
            DataGrid dataGrid3 = ((contentControl3 != null) ? contentControl3.FindName("argumentsDataGrid") : null) as DataGrid;
            if (dataGrid3 == null)
            {
                return;
            }
            dataGrid3.ApplyThemedStyle();

            if (dataGrid2 != null)
            {
                InitializeContextMenu(dataGrid2);
            }
        }

        private void InitializeContextMenu(DataGrid dataGrid)
        {
            _importedNamespacesDataGrid = dataGrid;
            _modelService = _designer.Context.Services.GetService<ModelService>();
            _importsModelItem = _modelService.Root.Properties["Imports"].Collection;
            _editingContext = _designer.Context;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Style = (this._importedNamespacesDataGrid.TryFindResource(typeof(ContextMenu)) as Style);

            MenuItem menuItem = new MenuItem();
            menuItem.Header = ApplicationCommands.Delete.Text;
            menuItem.Command = ApplicationCommands.Delete;
            contextMenu.Items.Add(menuItem);

            CommandBinding commandBinding = new CommandBinding(ApplicationCommands.Delete, new ExecutedRoutedEventHandler(DeleteCommandExecute), new CanExecuteRoutedEventHandler(DeleteCommandCanExecute));
            menuItem.CommandBindings.Add(commandBinding);

            this._importedNamespacesDataGrid.CommandBindings.Add(commandBinding);
            this._importedNamespacesDataGrid.ContextMenu = contextMenu;
            this._importedNamespacesDataGrid.MouseRightButtonDown += this.NamespacesDataGrid_MouseRightButtonDown;
        }


        private RelayCommand _removeUnusedNamespaceCommand;

        /// <summary>
        /// Gets the RemoveUnusedNamespaceCommand.
        /// </summary>
        public RelayCommand RemoveUnusedNamespaceCommand
        {
            get
            {
                return _removeUnusedNamespaceCommand
                    ?? (_removeUnusedNamespaceCommand = new RelayCommand(
                    () =>
                    {
                        IEnumerable<string> usedNamespaces = this.GetUsedNamespaces();
                        IEnumerable<string> computedValue =
                            from x in this._importsModelItem
                            select x.Properties["Namespace"].ComputedValue as string;
                        List<string> list = computedValue.Except<string>(usedNamespaces).ToList<string>();
                        List<ModelItem> modelItems = (
                            from x in this._importedNamespacesDataGrid.Items.OfType<ModelItem>()
                            where list.Contains((string)x.Properties["Namespace"].ComputedValue)
                            select x).ToList<ModelItem>();
                        this.DeleteUsings(modelItems);
                    },
                    () => !this._editingContext.IsReadOnly() && this._modelService.Root != null));
            }
        }



        public IEnumerable<string> GetUsedNamespaces()
        {
            return new List<string>();
        }



        private void NamespacesDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl) ? false : !Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this._importedNamespacesDataGrid.SelectedItems.Clear();
            }
            DataGridCell dataGridCell = e.OriginalSource.TryGet<object, DataGridCell>((object o) => ((DependencyObject)o).GetParent<DataGridCell>());
            DataGridRow dataGridRow = dataGridCell.TryGet<DataGridCell, DataGridRow>((DataGridCell c) => dataGridCell.GetParent<DataGridRow>());
            if (dataGridRow != null)
            {
                dataGridRow.IsSelected = true;
            }
            this._importedNamespacesDataGrid.ContextMenu.IsOpen = true;
        }

        private void DeleteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute;
            if (!this._editingContext.IsReadOnly())
            {
                IList selectedItems = this._importedNamespacesDataGrid.SelectedItems;
                canExecute = (selectedItems != null && selectedItems.Count > 0);
            }
            else
            {
                canExecute = false;
            }
            e.CanExecute = canExecute;
        }

        private void DeleteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            IList selectedItems = _importedNamespacesDataGrid.SelectedItems;
            List<ModelItem> modelItemsToRemove = (selectedItems != null) ? selectedItems.OfType<ModelItem>().ToList<ModelItem>() : null;
            DeleteUsings(modelItemsToRemove);
        }

        private  void DeleteUsings(IEnumerable<ModelItem> modelItemsToRemove)
        {
            List<string> strs;
            if ((modelItemsToRemove != null ? !modelItemsToRemove.Any<ModelItem>() : true))
            {
                return;
            }
            try
            {
                Dictionary<string, List<string>> computedValue = this._importsModelItem.Properties["AvailableNamespaces"].ComputedValue as Dictionary<string, List<string>>;
                using (ModelEditingScope modelEditingScope = this._modelService.Root.BeginEdit())
                {
                    foreach (ModelItem modelItem in modelItemsToRemove)
                    {
                        this._importsModelItem.Remove(modelItem);
                        this._addedNamespaces.Remove((string)modelItem.Properties["Namespace"].ComputedValue);
                    }
                    modelEditingScope.Complete();
                }
                IEnumerable<string> computedValue1 =
                    from x in this._importsModelItem
                    select x.Properties["Namespace"].ComputedValue as string;
                IList<AssemblyReference> assemblyReferences = (IList<AssemblyReference>)((dynamic)this._importsModelItem.GetCurrentValue().AsDynamic()).TextExpressionReferences.RealObject;
                HashSet<string> hashSet = (
                    from x in assemblyReferences
                    select x.AssemblyName.Name).ToHashSet<string>(null);
                HashSet<string> strs1 = new HashSet<string>();
                foreach (string str in computedValue1)
                {
                    if (!computedValue.TryGetValue(str, out strs))
                    {
                        continue;
                    }
                    foreach (string str1 in strs)
                    {
                        if (!hashSet.Contains(str1))
                        {
                            continue;
                        }
                        strs1.Add(str1);
                    }
                }
                assemblyReferences.Clear();
                foreach (string str2 in strs1)
                {
                    assemblyReferences.Add(new AssemblyReference()
                    {
                        AssemblyName = new AssemblyName(str2)
                    });
                }
            }
            catch (Exception exception)
            {
                exception.Trace(null);
            }
        }

        private void ImportsComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            TextBox textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (textBox != null)
            {
                textBox.IsKeyboardFocusedChanged += this.ImportsEditableTextBox_IsKeyboardFocusedChanged;
            }
        }

        private void ImportsEditableTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                TextBox textBox = sender as TextBox;
                textBox.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    textBox.Foreground = (Application.Current.TryFindResource(AssetResourceKeys.ControlForegroundNormalBrushKey) as Brush);
                }));
            }
        }

        private void UsubscribeImportsControlEvents()
        {
            if (this._designer == null)
            {
                return;
            }
            if (!IsDarkTheme())
            {
                return;
            }
            DesignerView service = this._designer.Context.Services.GetService<DesignerView>();
            ContentControl contentControl = ((service != null) ? service.FindName("imports1") : null) as ContentControl;
            ComboBox comboBox = ((contentControl != null) ? contentControl.FindName("inputComboBox") : null) as ComboBox;
            if (comboBox == null)
            {
                return;
            }
            comboBox.Loaded -= this.ImportsComboBox_Loaded;
            TextBox textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (textBox != null)
            {
                textBox.IsKeyboardFocusedChanged -= this.ImportsEditableTextBox_IsKeyboardFocusedChanged;
            }
        }

        
    }
}
