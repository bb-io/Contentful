using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Contentful.Core.Models;
using Newtonsoft.Json;

namespace Apps.Contentful.Models.Entities;

public class EntryEntity : IDownloadContentInput
{
    [Display("Entry ID")]
    public string ContentId { get; set; }

    [Display("Tag IDs")]
    public IEnumerable<string> TagIds { get; set; }

    [Display("Content type ID")]
    public string ContentTypeId { get; set; }
    
    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime? UpdatedAt { get; set; }

    [Display("Updated by (user ID)")]
    public string? UpdatedBy { get; set; }

    public int Version { get; set; }

    public EntryEntity(Entry<object> entry)
    {
        ContentId = entry.SystemProperties.Id;
        TagIds = entry.Metadata?.Tags.Select(x => x.Sys.Id) ?? Enumerable.Empty<string>();
        ContentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        CreatedAt = entry.SystemProperties.CreatedAt;
        UpdatedAt = entry.SystemProperties.UpdatedAt;
        Version = entry.SystemProperties.Version ?? default;
        UpdatedBy = entry.SystemProperties.UpdatedBy.SystemProperties.Id;
    }
}