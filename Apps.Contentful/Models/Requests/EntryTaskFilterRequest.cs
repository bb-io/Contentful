using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class EntryTaskFilterRequest
{
    [Display("User ID"), DataSource(typeof(UserDataSourceHandler))]
    public string? UserId { get; set; }
}