using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.Contentful.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseInvocable(invocationContext)
{
    [Action("[DEBUG] Action", Description = "Debug action")]
    public List<AuthenticationCredentialsProvider> GetCredentialsProviders()
    {
        return InvocationContext.AuthenticationCredentialsProviders.ToList();
    }
}