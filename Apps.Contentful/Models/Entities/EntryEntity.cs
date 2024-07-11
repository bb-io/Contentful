using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models;

namespace Apps.Contentful.Models.Entities;

public class EntryEntity
{
    [Display("Entry ID")]
    public string Id { get; set; }

    [Display("Tag IDs")]
    public IEnumerable<string> TagIds { get; set; }

    [Display("Content type ID")]
    public string ContentTypeId { get; set; }
    
    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }
    
    public int Version { get; set; }

    public string Locale { get; set; }

    public EntryEntity(Entry<object> entry)
    {
        Id = entry.SystemProperties.Id;
        TagIds = entry.Metadata.Tags.Select(x => x.Sys.Id);
        ContentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        CreatedAt = entry.SystemProperties.CreatedAt;
        Version = entry.SystemProperties.Version ?? default;
        Locale = entry.SystemProperties.Locale;
    }
}