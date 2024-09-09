using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class ListEntriesRequest : ContentModelOptionalIdentifier
{
    [Display("Tags"), DataSource(typeof(TagDataHandler))]
    public IEnumerable<string>? Tags { get; set; }
}