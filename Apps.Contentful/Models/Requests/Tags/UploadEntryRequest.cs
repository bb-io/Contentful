using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Contentful.Models.Requests.Tags;
public class UploadEntryRequest : EnvironmentIdentifier, IUploadContentInput
{
    public FileReference Content { get; set; }

    [DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; }

    [Display("Entry ID")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string? ContentId { get; set; }

    [Display("Don't update reference fields")]
    public bool? DontUpdateReferenceFields { get; set; }
}
