using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EntryLocaleIdentifier : LocaleIdentifier
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryLocaleDataSourceHandler))]
    public string EntryId { get; set; }
}