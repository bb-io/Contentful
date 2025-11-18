using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Contentful.Models.Responses;
public class DownloadContentOutput : IDownloadContentOutput
{
    public FileReference Content { get; set; }

    [Display("Errors")]
    public IEnumerable<ContentProcessingError>? Errors { get; set; }
}
