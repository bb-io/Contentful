using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class LocaleIdentifier : EnvironmentIdentifier
{
    [DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; } = string.Empty;
}