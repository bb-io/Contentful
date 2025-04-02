using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Apps.Contentful.Extensions;
using Apps.Contentful.HtmlHelpers;
using Apps.Contentful.Models;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Actions;

    [ActionList]
    public class UserActions(InvocationContext invocationContext)
    : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    [Action("Get user", Description = "Get details of a specific user")]
    public async Task<UserResponse> GetUser([ActionParameter] [Display("User ID", Description = "The ID of the user"), DataSource(typeof(UserDataSourceHandler))] string input, EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var user =  await client.ExecuteWithErrorHandling(async () => await client.GetUser(input, Creds.FirstOrDefault(x => x.KeyName == "spaceId")?.Value));
        return new(user);
    }
    
}
    

