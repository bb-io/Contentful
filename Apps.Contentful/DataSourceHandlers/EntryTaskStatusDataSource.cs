using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryTaskStatusDataSource : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            { "active", "Active" },
            { "resolved", "Resolved" }
        };
    }
}