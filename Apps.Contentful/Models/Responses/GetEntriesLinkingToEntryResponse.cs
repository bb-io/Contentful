using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetEntriesLinkingToEntryResponse
{
    [Display("Entries")]
    public IEnumerable<EntryEntity> Entries { get; set; } = [];

    [Display("Entry IDs")]
    public IEnumerable<string> EntriesIds { get; set; } = [];

    [Display("First entry ID")]
    public string FirstEntryId { get; set; } = string.Empty;

    [Display("Total count")]
    public double TotalCount { get; set; }
}
