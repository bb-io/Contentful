using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class DuplicateEntryRequest
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string EntryId { get; set; } = string.Empty;

    [Display("New content model", Description = "New content type for the cloned entry. If not specified, uses the original content type.")]
    [DataSource(typeof(ContentModelDataSourceHandler))]
    public string? NewRootContenTypeId { get; set; }

    [Display("Duplicate referenced content", Description = "Enable recursive cloning of referenced entries.")]
    public bool? DuplicateRecursively { get; set; }

    [Display("Duplicate content referenced in fields", Description = "Field IDs that should have their referenced entries cloned recursively. Other fields will maintain original references.")]
    [DataSource(typeof(ReferenceFieldDataHandler))]
    public IEnumerable<string>? DuplicateFromFieldIds { get; set; }

    [Display("Duplicate referenced assets", Description = "Enable recursive cloning of referenced assets. If not specified, links the original asset.")]
    public bool? EnableAssetCloning { get; set; }

    [DataSource(typeof(EnvironmentDataSourceHandler))]
    public string? Environment { get; set; }
}