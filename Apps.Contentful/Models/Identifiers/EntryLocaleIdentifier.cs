using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Models.Identifiers;

public class EntryLocaleIdentifier : LocaleIdentifier
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryLocaleDataSourceHandler))]
    public string EntryId { get; set; }

    public new void Validate()
    {
        base.Validate();
        if (string.IsNullOrWhiteSpace(EntryId))
            throw new PluginMisconfigurationException("Entry ID must be provided. Please check your input and try again");
    }
}