using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Contentful.Models.Responses;
public class DownloadContentOutput : IDownloadContentOutput
{
    public FileReference Content { get; set; }

    [Display("Main entry ID")]
    public string RootEntryId { get; set; } = string.Empty;

    [Display("Errors")]
    public IEnumerable<ContentProcessingError>? Errors { get; set; }
}
