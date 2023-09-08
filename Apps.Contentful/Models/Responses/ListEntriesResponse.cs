using Apps.Contentful.Models.Identifiers;

namespace Apps.Contentful.Models.Responses;

public class ListEntriesResponse
{
    public IEnumerable<EntryIdentifier> Entries { get; set; }
}