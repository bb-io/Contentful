using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class UpdateEntryTaskRequest : EntryTaskIdentifier
{
    [Display("Body", Description = "The body of the task")]
    public string? Body { get; set; }

    [StaticDataSource(typeof(EntryTaskStatusDataSource))]
    public string? Status { get; set; }

    [Display("Assigned to", Description = "The user assigned to this task"), DataSource(typeof(UserDataSourceHandler))]
    public string? AssignedTo { get; set; }
}