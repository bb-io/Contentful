using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class PublishContentRequest : EnvironmentIdentifier
{
    public FileReference Content { get; set; }

    [Display("Locales to publish"), DataSource(typeof(LocaleDataSourceHandler))]
    public IEnumerable<string>? Locales { get; set; }
}
