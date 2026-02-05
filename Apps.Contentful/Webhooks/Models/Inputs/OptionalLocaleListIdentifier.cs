using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Webhooks.Models.Inputs;
public class OptionalLocaleListIdentifier
{
    [Display("Published locales (at least one)",
        Description = "Filter the incoming events to only trigger when any of the following locales are present")]
    [DataSource(typeof(LocaleDataSourceHandler))]
    public IEnumerable<string>? Locales { get; set; }

    [Display("Exclude locales",
        Description = "Filter the incoming events to only trigger when all of the following locales are not present"), DataSource(typeof(LocaleDataSourceHandler))]
    public IEnumerable<string>? ExcludeLocales { get; set; }
}
