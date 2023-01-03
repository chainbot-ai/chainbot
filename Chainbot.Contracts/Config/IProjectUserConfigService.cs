using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IProjectUserConfigService
    {
        string ProjectCreatePath { get; set; }

        bool Load();
        bool Save();
    }
}
