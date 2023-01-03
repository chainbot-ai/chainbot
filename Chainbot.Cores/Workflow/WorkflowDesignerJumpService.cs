using Chainbot.Contracts.Classes;
using Chainbot.Contracts.Utils;
using Chainbot.Contracts.Workflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using ReflectionMagic;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Presentation.ViewState;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Workflow
{
    public class WorkflowDesignerJumpService : MarshalByRefServiceBase, IWorkflowDesignerJumpService
    {
        private WorkflowDesigner _designer;
        private ICommonService _commonService;

        public string Path { get; set; }

        public WorkflowDesignerJumpService(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public void Init(WorkflowDesigner designer)
        {
            _designer = designer;
        }

        public void Init(string path)
        {
            Path = path;
            _designer = new WorkflowDesigner();
            _designer.Load(Path);
        }

        private static Predicate<Type> AllActivitiesPredicate
        {
            get
            {
                return (Type type) => (typeof(Activity).IsAssignableFrom(type) ^ (typeof(ITextExpression).IsAssignableFrom(type)
                || typeof(IValueSerializableExpression).IsAssignableFrom(type))) || typeof(FlowNode).Namespace == type.Namespace;
            }
        }


        private void HighlightVariable(ModelItem variableModelItem)
        {
            WorkflowDesigner designer = this._designer;
            ModelSearchService modelSearchService = (designer != null) ? designer.Context.Services.GetService<ModelSearchService>() : null;
            ModelItem parent = variableModelItem.Parent.Parent;
            ModelSearchService modelSearchService2 = modelSearchService;
            if (modelSearchService2 != null)
            {
                dynamic dyObject = modelSearchService2.AsDynamic();
                dyObject.HighlightModelItem(parent);
            }
            WorkflowDesigner designer2 = this._designer;
            DesignerView designerView = (designer2 != null) ? designer2.Context.Services.GetService<DesignerView>() : null;
            DesignerView designerView2 = designerView;
            if (designerView2 != null)
            {
                dynamic dyObject = designerView2.AsDynamic();
                dyObject.CheckButtonVariables();
            }
            designerView2 = designerView;
            object obj;
            if (designerView2 == null)
            {
                obj = null;
            }
            else
            {
                dynamic dyObject = designerView2.AsDynamic();
                obj = dyObject.variables1;
            }
            object obj2 = obj;
            if (obj2 != null)
            {
                dynamic dyObject = obj2.AsDynamic();
                dyObject.SelectVariable(variableModelItem);
            }
        }


        private ModelItemCollection GetVariableCollection(ModelItem modelItem)
        {
            ModelProperty obj;
            if (modelItem == null)
            {
                obj = null;
            }
            else
            {
                ModelPropertyCollection properties = modelItem.Properties;
                if (properties == null)
                {
                    obj = null;
                }
                else
                {
                    obj = properties.FirstOrDefault((ModelProperty property) => property.IsCollection && property.PropertyType == typeof(Collection<Variable>));
                }
            }

            if (obj == null)
            {
                return null;
            }
            return obj.Collection;
        }


        private IEnumerable<ModelItem> GetProperties(ModelItem modelItem)
        {
            IEnumerable<ModelItem> enumerable;
            if (modelItem == null)
            {
                enumerable = null;
            }
            else
            {
                ModelProperty modelProperty = modelItem.Properties["Properties"];
                enumerable = ((modelProperty != null) ? modelProperty.Collection : null);
            }
            IEnumerable<ModelItem> enumerable2 = enumerable;
            return enumerable2 ?? Enumerable.Empty<ModelItem>();
        }

        private string GetName(ModelItem modelItem)
        {
            object obj;
            if (modelItem == null)
            {
                obj = null;
            }
            else
            {
                ModelPropertyCollection properties = modelItem.Properties;
                if (properties == null)
                {
                    obj = null;
                }
                else
                {
                    ModelProperty modelProperty = properties["Name"];
                    obj = ((modelProperty != null) ? modelProperty.ComputedValue : null);
                }
            }
            return obj as string;
        }

        private bool TryHighlightArgument(string argumentName)
        {
            if (string.IsNullOrEmpty(argumentName))
            {
                return false;
            }
            var modelService = _designer.Context.Services.GetService<ModelService>();
            IEnumerable<ModelItem> properties = GetProperties((modelService != null) ? modelService.Root : null);
            ModelItem modelItem = (properties != null) ? properties.FirstOrDefault((ModelItem o) => GetName(o) == argumentName) : null;
            if (modelItem == null)
            {
                return false;
            }
            this.HighlightArgument(modelItem);
            return true;
        }


        private void HighlightArgument(ModelItem argumentModelItem)
        {
            WorkflowDesigner designer = this._designer;
            ModelService modelService = (designer != null) ? designer.Context.Services.GetService<ModelService>() : null;
            ModelItem arg = (modelService != null) ? modelService.Root : null;
            WorkflowDesigner designer2 = this._designer;
            ModelSearchService o = (designer2 != null) ? designer2.Context.Services.GetService<ModelSearchService>() : null;

            if (o != null)
            {
                dynamic dyObject = o.AsDynamic();
                dyObject.HighlightModelItem(arg);
            }

            WorkflowDesigner designer3 = this._designer;
            DesignerView o2 = (designer3 != null) ? designer3.Context.Services.GetService<DesignerView>() : null;


            if (o2 != null)
            {
                dynamic dyObject = o2.AsDynamic();
                dyObject.CheckButtonArguments();
            }


            if (o2 != null)
            {
                dynamic dyObject = o2.AsDynamic();
                dyObject.CheckButtonArguments();
            }

            if (o2 != null)
            {
                dynamic dyObject = o2.AsDynamic();
                dyObject.arguments1.SelectArgument(argumentModelItem);
            }
        }



        private void FocusActivity(string idRef, IReadOnlyCollection<ModelItem> activities)
        {
            ModelItem modelItem = activities.FirstOrDefault((ModelItem a) => WorkflowViewState.GetIdRef(a.GetCurrentValue()) == idRef);
            if (modelItem == null)
            {
                return;
            }
            modelItem.Focus();
        }

        private void FocusVariable(string variableName, IEnumerable<ModelItem> activities, string idRef)
        {
            ModelItem modelItem = activities.FirstOrDefault((ModelItem a) => WorkflowViewState.GetIdRef(a.GetCurrentValue()) == idRef);
            if (modelItem != null)
            {
                ModelItemCollection variableCollection = GetVariableCollection(modelItem);
                ModelItem modelItem2 = (variableCollection != null) ? variableCollection.FirstOrDefault((ModelItem v) => ((Variable)v.GetCurrentValue()).Name == variableName) : null;
                if (modelItem2 != null)
                {
                    HighlightVariable(modelItem2);
                }
            }
        }




        private string GetActivityItemAssemblyQualifiedName(ModelItem modelItem)
        {
            if (modelItem.ItemType == typeof(System.Activities.Statements.State))
            {
                System.Activities.Statements.State state = modelItem.GetCurrentValue() as System.Activities.Statements.State;
                if (state != null && state.IsFinal)
                {
                    return typeof(FinalState).AssemblyQualifiedName;
                }
            }
            return modelItem.ItemType.AssemblyQualifiedName;
        }

        public string GetAllActivities()
        {
            JArray retArr = new JArray();

            var modelService = _designer.Context.Services.GetService<ModelService>();

            var activities = modelService.Find(modelService.Root, AllActivitiesPredicate).ToList();

            //Console.WriteLine("====================================");

            foreach (var activity in activities)
            {
                var val = activity.GetCurrentValue();
                if (val is Activity)
                {
                    var assemblyQualifiedName = GetActivityItemAssemblyQualifiedName(activity);//类型名称
                    var displayName = (val as Activity).DisplayName;
                    var idRef = WorkflowViewState.GetIdRef(val);

                    var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, Path);
                    var location = relativeFile.Replace("\\", " > ");

                    var jObj = new JObject();
                    jObj["DisplayName"] = displayName;
                    jObj["IdRef"] = idRef;
                    jObj["Path"] = Path;
                    jObj["Location"] = location;
                    jObj["AssemblyQualifiedName"] = assemblyQualifiedName;

                    retArr.Add(jObj);

                    //Console.WriteLine($"{displayName}    {location}       {idRef}       {assemblyQualifiedName}");
                }
            }


            //FocusActivity("ClickActivity_3", activities);

            //Console.WriteLine("====================================");

            return JsonConvert.SerializeObject(retArr);
        }


        public string GetAllVariables()
        {
            JArray retArr = new JArray();

            var modelService = _designer.Context.Services.GetService<ModelService>();

            var activities = modelService.Find(modelService.Root, AllActivitiesPredicate).ToList();

            //Console.WriteLine("====================================");

            foreach (var activity in activities)
            {
                var val = activity.GetCurrentValue();

                if (val is Activity)
                {
                    var idRef = WorkflowViewState.GetIdRef(val);
                    var variables = GetVariableCollection(activity);

                    if (variables == null)
                    {
                        continue;
                    }

                    var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, Path);
                    var location = relativeFile.Replace("\\", " > ");

                    foreach (var variable in variables)
                    {
                        try
                        {
                            var varItem = (variable.GetCurrentValue()) as Variable;
                            var varName = varItem.Name;
                            var parentActivity = varItem.AsDynamic().Owner.RealObject as Activity;

                            var jObj = new JObject();
                            jObj["DisplayName"] = varName;
                            jObj["ScopeName"] = parentActivity.DisplayName;
                            jObj["IdRef"] = idRef;
                            jObj["Path"] = Path;
                            jObj["Location"] = location;

                            retArr.Add(jObj);
                            //Console.WriteLine($"{varName}    {location}       {idRef}");
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    //variable5    Main.xaml       Sequence_2
                    //FocusVariable("variable5", activities, "Sequence_2");
                }

            }

            //Console.WriteLine("====================================");

            return JsonConvert.SerializeObject(retArr);
        }

        public string GetAllArguments()
        {
            JArray retArr = new JArray();

            var modelService = _designer.Context.Services.GetService<ModelService>();
            IEnumerable<ModelItem> properties = GetProperties((modelService != null) ? modelService.Root : null);

            if (properties == null)
            {
                return JsonConvert.SerializeObject(retArr);
            }

            //Console.WriteLine("====================================");

            foreach (var argument in properties)
            {
                var argumentName = GetName(argument);

                var relativeFile = _commonService.MakeRelativePath(SharedObject.Instance.ProjectPath, Path);
                var location = relativeFile.Replace("\\", " > ");

                var jObj = new JObject();
                jObj["DisplayName"] = argumentName;
                jObj["Path"] = Path;
                jObj["Location"] = location;

                retArr.Add(jObj);

                //Console.WriteLine($"{argumentName}    {location}");
            }

            //FocusArgument("argument2");

            //Console.WriteLine("====================================");

            return JsonConvert.SerializeObject(retArr);
        }



        public void FocusActivity(string idRef)
        {
            var modelService = _designer.Context.Services.GetService<ModelService>();

            var activities = modelService.Find(modelService.Root, AllActivitiesPredicate).ToList();

            FocusActivity(idRef, activities);
        }


        public void FocusVariable(string variableName, string idRef)
        {
            var modelService = _designer.Context.Services.GetService<ModelService>();

            var activities = modelService.Find(modelService.Root, AllActivitiesPredicate).ToList();

            FocusVariable(variableName, activities, idRef);
        }


        public void FocusArgument(string argumentName)
        {
            TryHighlightArgument(argumentName);
        }

    }
}
