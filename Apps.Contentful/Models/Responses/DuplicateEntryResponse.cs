using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class DuplicateEntryResponse
{
    [Display("Duplicated entry")]
    public required EntryEntity RootEntry { get; set; }

    [Display("Duplicated referenced entry IDs")]
    public IEnumerable<string> RecursivelyClonedEntryIds { get; set; } = [];

    [Display("Duplicated referenced asset IDs")]
    public IEnumerable<string> RecursivelyClonedAssetIds { get; set; } = [];

    [Display("Total items duplicated")]
    public int TotalItemsCloned { get; set; }

    [Display("Duplication depth reached")]
    public int RecursionDepthReached { get; set; }
}