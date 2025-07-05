using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Contentful.Models.Identifiers;

public class DownloadContentInput : EnvironmentIdentifier, IDownloadContentInput
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string ContentId { get; set; }

    [DataSource(typeof(LocaleDataSourceHandler))]
    public string? Locale { get; set; }
}