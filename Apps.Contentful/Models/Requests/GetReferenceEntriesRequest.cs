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

    [Display("Content type IDs", Description = "Specific content type model IDs to check. If not specified, all content type models will be checked.")]
    public IEnumerable<string>? ContentTypeIds { get; set; }

    [Display("Search recursively")]
    public bool? SearchRecursively { get; set; } = false;
}
