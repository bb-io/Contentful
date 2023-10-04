using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests.Tags;

public class TagToEntryInput
{
    [Display("Entry")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string EntryId { get; set; }
    
    [Display("Tag")]
    [DataSource(typeof(TagDataHandler))]
    public string TagId { get; set; }
}