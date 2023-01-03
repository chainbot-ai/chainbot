using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IServerSettingsService
    {
        string ControlServerUrl { get; set; }
        string CCLX { get; set; }
        string AIServerUrl { get; set; }
        string HelpLinkUrl { get; set; }

        void Load();
        void Save();
    }
}
