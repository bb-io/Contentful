using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetReferenceEntriesResponse
{
    [Display("Referenced entries")]
    public IEnumerable<EntryEntity> ReferencedEntries { get; set; } = Enumerable.Empty<EntryEntity>();
    
    [Display("Referenced entry IDs")]
    public IEnumerable<string> ReferencedEntryIds { get; set; } = Enumerable.Empty<string>();
}
