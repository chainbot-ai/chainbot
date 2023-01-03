using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Utils;
using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Chainbot.Cores.Activities
{
    public class SystemActivityIconService : ISystemActivityIconService
    {
        private ICommonService _commonService;

        private Dictionary<string, Brush> TypeOfBrushDict = new Dictionary<string, Brush>();

        public SystemActivityIconService(ICommonService commonService)
        {
            _commonService = commonService;

            RegisterSystemIcons();
        }

        private void RegisterSystemIcons()
        {
            //TypeOfBrushDict["Sequence"] = WorkflowDesignerIcons.Activities.Sequence;
        }

        public ImageSource GetIcon(string typeOf)
        {
            typeOf = typeOf.Split('`')[0];
            if (TypeOfBrushDict.ContainsKey(typeOf))
            {
                return _commonService.BitmapSourceFromBrush(TypeOfBrushDict[typeOf]);
            }
            else
            {
                Type activitiesIcons = typeof(WorkflowDesignerIcons.Activities);
                var info = activitiesIcons.GetProperty(typeOf);
                if(info != null)
                {
                    TypeOfBrushDict[typeOf] = (DrawingBrush)info.GetValue(null);
                    return _commonService.BitmapSourceFromBrush(TypeOfBrushDict[typeOf]);
                }
            }

            return null;
        }
    }
}
