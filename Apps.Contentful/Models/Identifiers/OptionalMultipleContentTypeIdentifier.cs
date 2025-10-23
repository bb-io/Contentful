using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class OptionalMultipleContentTypeIdentifier
{
    [Display("Content types", Description = "Filter when it is of any of the following content types")]
    [DataSource(typeof(ContentModelDataSourceHandler))]
    public IEnumerable<string>? ContentModels { get; set; }
}
