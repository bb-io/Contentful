using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class GetReferenceEntriesRequest : EntryIdentifier
{
    [Display("Field IDs", Description = "Specific reference field IDs to check. If not specified, all reference fields will be checked.")]
    [DataSource(typeof(ReferenceFieldDataHandler))]
    public IEnumerable<string>? FieldIds { get; set; }
}
