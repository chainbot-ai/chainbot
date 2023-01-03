using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Classes
{
    public static class ActivityHelper
    {
        public static ModelItem SurroundItemWithSequence(ModelItem item, EditingContext context,string sequenceDisplayName)
        {
            ModelItem modelItem = null;
            using (ModelEditingScope modelEditingScope = item.BeginEdit())
            {
                modelItem = ModelFactory.CreateItem(context, new Sequence() { DisplayName = sequenceDisplayName });
                MorphHelper.MorphObject(item, modelItem);
                modelItem.Properties["Activities"].Collection.Add(item);
                modelEditingScope.Complete();
            }
            return modelItem;
        }
    }
}
