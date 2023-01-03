using Chainbot.Contracts.Classes;
using System.Collections.Generic;

namespace Chainbot.Contracts.Nupkg
{
    public interface IPackageRepositoryService
    {
        void Init(string packageSource);

        List<NugetPackageItem> GetMatchedPackagesByIdAndMaxVersion(string matchRegex);
    }
}
