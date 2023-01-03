using Chainbot.Contracts.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Project
{
    public class NupkgIdVer
    {
        public NupkgIdVer(string id, string ver)
        {
            this.Id = id;
            this.Version = ver;
        }

        public string Id { get; set; }

        public string Version { get; set; }
    }

    public interface IProjectDependenciesService
    {
        bool Init(string projectPath);

        bool Install(string nupkgPath);

        bool Install(List<string> nupkgPathList);

        bool Uninstall(NupkgIdVer IdVer);

        bool Uninstall(List<NupkgIdVer> IdVerList);
    }
}
