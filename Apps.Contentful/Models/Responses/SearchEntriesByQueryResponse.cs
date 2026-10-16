using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class SearchEntriesByQueryResponse(EntryEntity[] entries)
{
    [Display("First entry")]
    public EntryEntity? FirstEntry => Entries.FirstOrDefault();

    [Display("First entry ID")]
    public string FirstEntryId => FirstEntry?.ContentId ?? string.Empty;

    [Display("Entries")]
    public EntryEntity[] Entries { get; set; } = entries;
}
