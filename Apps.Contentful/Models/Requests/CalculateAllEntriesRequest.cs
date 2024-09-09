using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class CalculateAllEntriesRequest : EnvironmentIdentifier
{
    [Display("Content models"), DataSource(typeof(ContentModelDataSourceHandler))]
    public IEnumerable<string>? ContentModels { get; set; }
}