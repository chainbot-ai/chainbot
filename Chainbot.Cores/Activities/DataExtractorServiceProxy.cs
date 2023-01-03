
using Chainbot.Contracts.Activities;
using Chainbot.Contracts.AppDomains;
using Chainbot.Contracts.Classes;

namespace Chainbot.Cores.Activities
{
    public class DataExtractorServiceProxy : MarshalByRefServiceProxyBase<IDataExtractorService>, IDataExtractorServiceProxy
    {
        public DataExtractorServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {
        }

        public void Save(string path, string targetData, string metaData, string pageNextData)
        {
            InnerService.Save(path,targetData,metaData,pageNextData);
        }
    }
}
