using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IActivitiesChildrenOrderService
    {
        void Init();

        int GetOrder(string name, int initOrder);
    }
}
