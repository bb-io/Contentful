using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class PublishContentOutput
{
    [Display("Main entry detected")]
    public required string? RootEntryId { get; set; }

    [Display("Published referenced entry IDs", Description = "Successfully published entries, including main entry. If entry was already published it will be included in this list too.")]
    public IEnumerable<string> PublishedEntryIds { get; set; } = [];

    [Display("Published referenced asset IDs", Description = "Successfully published assets. If asset was already published it will be included in this list too.")]
    public IEnumerable<string> PublishedAssetIds { get; set; } = [];

    [Display("Total items published", Description = "Counts successfully published entries and assets, includes items that were already published.")]
    public int TotalItemsPublished { get; set; }

    [Display("Errors during publishing", Description = "Action won't fail if publishing of one entry or asset has returned an error, but will list errors")]
    public IEnumerable<ContentProcessingError> Errors { get; set; } = [];
}
