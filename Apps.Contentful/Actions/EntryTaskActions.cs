using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryTaskActions(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    [Action("Search entry tasks", Description = "Search for entry tasks by specific criteria")]
    public async Task<GetEntryTasksResponse> GetEntryTasks([ActionParameter] EntryIdentifier identifier)
    {
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest($"/entries/{identifier.EntryId}/tasks", Method.Get, Creds);
        var entryTasks = await client.Paginate<TaskDto>(request);

        var taskDtos = entryTasks as TaskDto[] ?? entryTasks.ToArray();
        return new()
        {
            EntryTasks = taskDtos.Select(x => new EntryTaskEntity(x)).ToList(),
            TotalCount = taskDtos.Length
        };
    }
    
    [Action("Get entry task", Description = "Get details of a specific entry task")]
    public async Task<EntryTaskEntity> GetEntryTask([ActionParameter] EntryTaskIdentifier identifier)
    {
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest($"/entries/{identifier.EntryId}/tasks/{identifier.EntryTaskId}", Method.Get, Creds);
        var entryTask = await client.ExecuteWithErrorHandling<TaskDto>(request);
        
        return new(entryTask);
    }
    
    [Action("Update entry task", Description = "Update an entry task with new details")]
    public async Task<EntryTaskEntity> UpdateEntryTask([ActionParameter] UpdateEntryTaskRequest entryTask)
    {
        var task = await GetEntryTask(entryTask);
        
        var client = new ContentfulRestClient(Creds, entryTask.Environment);
        var request = new ContentfulRestRequest($"/entries/{entryTask.EntryId}/tasks/{entryTask.EntryTaskId}", Method.Put, Creds)
            .WithJsonBody(new
            {
                body = entryTask.Body ?? task.Body,
                status = entryTask.Status ?? task.Status,
                assignedTo = new
                {
                    sys = new
                    {
                        type = "Link",
                        linkType = "User",
                        id = entryTask.AssignedTo ?? task.AssignedTo
                    }
                }
            })
            .AddHeader("X-Contentful-Version", task.Version);
        
        var updatedEntryTask = await client.ExecuteWithErrorHandling<TaskDto>(request);
        return new(updatedEntryTask);
    }
}