﻿using Apps.Contentful.Constants;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient : BlackBirdRestClient
{
    public ContentfulRestClient(AuthenticationCredentialsProvider[] creds) : base(new()
    {
        BaseUrl = $"{Urls.Api}/spaces/{creds.Get("spaceId").Value}".ToUri()
    })
    {
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
        return new(error.Message);
    }
}