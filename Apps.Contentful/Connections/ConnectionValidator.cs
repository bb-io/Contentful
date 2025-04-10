﻿using Apps.Contentful.Api;
using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.Contentful.Connections;

public class ConnectionValidator(InvocationContext invocationContext) : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var client = new ContentfulClient(authProviders, null);
            var spaceId = authProviders.Get(CredNames.SpaceId)?.Value ?? throw new Exception("Space ID is missing");

            var result = await client.GetEnvironments(spaceId, cancellationToken: cancellationToken);
            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            var managementApiKey = authProviders.FirstOrDefault(p => p.KeyName == "Authorization")?.Value ?? string.Empty;
            var spaceId = authProviders.FirstOrDefault(p => p.KeyName == "spaceId")?.Value ?? string.Empty;

            invocationContext.Logger?.LogError($"[ContentfulValidator] Error validating connection: {ex.Message}; Management API key: {managementApiKey}; Space ID: {spaceId}", []);

            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}