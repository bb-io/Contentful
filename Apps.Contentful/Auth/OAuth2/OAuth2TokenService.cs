﻿using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Auth.OAuth2;

public class OAuth2TokenService : BaseInvocable, IOAuth2TokenService
{
    public OAuth2TokenService(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        return false;
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Dictionary<string, string?>> RequestToken(
        string state, 
        string code, 
        Dictionary<string, string> values, 
        CancellationToken cancellationToken)
    {
        return new Dictionary<string, string?>() { { "access_token", values["access_token"] } };
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }
}