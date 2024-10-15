using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class ListEntriesRequest : ContentModelOptionalIdentifier
{
    [Display("Tags"), DataSource(typeof(TagDataHandler))]
    public IEnumerable<string>? Tags { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedTo { get; set; }
}