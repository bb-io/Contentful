using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Connections;

public class ConnectionValidator(InvocationContext invocationContext) : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            throw new Exception("Test exception");

            var client = new ContentfulClient(authProviders, null);
            var result = await client.GetContentTypes(cancellationToken: cancellationToken);

            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            invocationContext.Logger?.LogError($"[ContentfulValidator] Error validating connection: {ex.Message}", []);
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}