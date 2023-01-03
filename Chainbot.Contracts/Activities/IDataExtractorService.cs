
namespace Chainbot.Contracts.Activities
{
    public interface IDataExtractorService
    {
        void Save(string path,string targetData, string metaData, string pageNextData);
    }
}