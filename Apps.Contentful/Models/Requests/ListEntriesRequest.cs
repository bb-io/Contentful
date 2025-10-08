using Apps.Contentful.DataSourceHandlers.Tags;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class ListEntriesRequest : ContentModelOptionalIdentifier
{
    [Display("Tags"), DataSource(typeof(ListEntriesTagDataHandler))]
    public IEnumerable<string>? Tags { get; set; }    
    
    [Display("Exclude tags"), DataSource(typeof(ListEntriesTagDataHandler))]
    public IEnumerable<string>? ExcludeTags { get; set; }

    [Display("Updated from")]
    public DateTime? UpdatedFrom { get; set; }

    [Display("Updated to")]
    public DateTime? UpdatedTo { get; set; }

    [Display("Published before")]
    public DateTime? PublishedBefore { get; set; }

    [Display("Published after")]
    public DateTime? PublishedAfter { get; set; }

    [Display("First published before")]
    public DateTime? FirstPublishedBefore { get; set; }

    [Display("First published after")]
    public DateTime? FirstPublishedAfter { get; set; }

    [Display("Published", Description = "Filter by published entries. If not set, all entries are returned.")]
    public bool? Published { get; set; }
    
    [Display("Changed", Description = "Filter by changed entries. If not set, all entries are returned.")]
    public bool? Changed { get; set; }
    
    [Display("Draft", Description = "Filter by draft entries. If not set, all entries are returned.")]
    public bool? Draft { get; set; }

    [Display("Search term", Description = "Full‑text search across all text and symbol fields")]
    public string? SearchTerm { get; set; }
}