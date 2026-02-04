using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests.Tags;

public class UnpublishEntryRequest
{
    [Display("Locales to unpublish"), DataSource(typeof(LocaleDataSourceHandler))]
    public IEnumerable<string>? Locales { get; set; }
}
