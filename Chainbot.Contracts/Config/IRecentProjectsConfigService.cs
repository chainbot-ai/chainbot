using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Config
{
    public interface IRecentProjectsConfigService
    {
        event EventHandler ChangeEvent;

        List<object> Load();

        void Add(string projectConfigFilePath);

        void Remove(string projectConfigFilePath);

        void Update(string projectConfigFilePath, string name, string description);
    }
}
