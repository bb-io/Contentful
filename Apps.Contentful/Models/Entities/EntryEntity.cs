using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Contentful.Core.Models;

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

    [Display("Is draft?")]
    public bool IsDraft {  get; set; }

    [Display("Is changed?")]
    public bool IsChanged { get; set; }

    [Display("Is published?")]
    public bool IsPublished { get; set; }
    
    [Display("Is archived?")]
    public bool IsArchived { get; set; }

    public EntryEntity(Entry<object> entry)
    {
        ContentId = entry.SystemProperties.Id;
        TagIds = entry.Metadata?.Tags.Select(x => x.Sys.Id) ?? Enumerable.Empty<string>();
        ContentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        CreatedAt = entry.SystemProperties.CreatedAt;
        UpdatedAt = entry.SystemProperties.UpdatedAt;
        Version = entry.SystemProperties.Version ?? default;
        UpdatedBy = entry.SystemProperties.UpdatedBy.SystemProperties.Id;

        // see docs about state: https://www.contentful.com/developers/docs/tutorials/general/determine-entry-asset-state/
        var publishedVersion = entry.SystemProperties.PublishedVersion ?? 0;
        IsDraft = publishedVersion < 1;
        IsChanged = publishedVersion > 0 && entry.SystemProperties.Version == publishedVersion + 2;
        IsPublished = publishedVersion > 0 && entry.SystemProperties.Version == publishedVersion + 1;
        IsArchived = (entry.SystemProperties.ArchivedVersion ?? 0) > 0;
    }

    public EntryEntity(EntryEntity other)
    {
        ContentId = other.ContentId;
        TagIds = other.TagIds;
        ContentTypeId = other.ContentTypeId;
        CreatedAt = other.CreatedAt;
        UpdatedAt = other.UpdatedAt;
        Version = other.Version;
        UpdatedBy = other.UpdatedBy;
        IsDraft = other.IsDraft;
        IsChanged = other.IsChanged;
        IsPublished = other.IsPublished;
        IsArchived = other.IsArchived;
    }
}