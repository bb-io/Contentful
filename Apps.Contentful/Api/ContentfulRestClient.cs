using Apps.Contentful.Constants;
using Apps.Contentful.Models.Wrappers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient(AuthenticationCredentialsProvider[] creds, string? environment)
    : BlackBirdRestClient(new()
    {
        BaseUrl =
            $"{creds.Get(CredNames.BaseUrl).Value}/spaces/{creds.Get(CredNames.SpaceId).Value}{GetEnvironmentSegment(environment)}"
                .ToUri(),
        MaxTimeout=180000,
        ConfigureMessageHandler = ContentfulHttpHandlers.Wrap
    })
{
    public async Task<IEnumerable<T>> Paginate<T>(ContentfulRestRequest request)
    {
        var result = new List<T>();
        var total = -1;
        while (result.Count != total)
        {
            request.AddOrUpdateParameter("skip", result.Count.ToString());
            var res = await ExecuteWithErrorHandling<ItemWrapper<T>>(request);
            total = res.Total;
            if (res.Items != null)
                result.AddRange(res.Items);
        }

        return result;
    }

    protected override Exception ConfigureErrorException(RestResponse response) =>
        response.ErrorException as PluginApplicationException
        ?? new PluginApplicationException(
            $"Connection error while communicating with Contentful: {response.ErrorMessage}",
            response.ErrorException);

    private static string GetEnvironmentSegment(string? environment) =>
        string.IsNullOrWhiteSpace(environment) ? string.Empty : $"/environments/{environment}/";
}
