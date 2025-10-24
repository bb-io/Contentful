using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Models.Identifiers;

public class LocaleIdentifier : EnvironmentIdentifier
{
    [DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrEmpty(Locale))
            throw new PluginMisconfigurationException("Locale must be provided. Please check your input and try again");
    }
}