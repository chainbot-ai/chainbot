using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.App
{
    public interface IUpgradeAppSettingsService
    {
        bool Upgrade();
    }
}
