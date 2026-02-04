using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class PublishEntryRequest
{
    [Display("Locales to publish"), DataSource(typeof(LocaleDataSourceHandler))]
    public IEnumerable<string>? Locales { get; set; }
}
