using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class UpdateAssetFromHtmlRequest : EnvironmentIdentifier
{
    [Display("HTML file")]
    public FileReference File { get; set; } = default!;

    [Display("Locale"), DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; } = default!;
}
