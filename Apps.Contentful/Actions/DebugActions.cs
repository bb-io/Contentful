using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext)
    : BaseInvocable(invocationContext)
{
    [Action("[DEBUG] Get authentication credential providers", Description = "Get authentication credential providers")]
    public List<AuthenticationCredentialsProvider> GetAuthenticationCredentialProviders()
    {
        return InvocationContext.AuthenticationCredentialsProviders.ToList();
    }
}