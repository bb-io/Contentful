using Apps.Contentful.Models.Entities;

namespace Apps.Contentful.Models.Responses;

public class ListEntriesResponse
{
    public IEnumerable<EntryEntity> Entries { get; set; }
}