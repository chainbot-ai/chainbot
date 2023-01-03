namespace Chainbot.Contracts.Activities
{
    public interface IDataExtractorServiceProxy
    {
        void Save(string path,string targetData,string metaData,string pageNextData);
    }
}
