using Apps.Contentful.Constants;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Wrappers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient : BlackBirdRestClient
{
    public ContentfulRestClient(AuthenticationCredentialsProvider[] creds, string? environment) : base(new()
    {
        BaseUrl = $"{Urls.Api}/spaces/{creds.Get("spaceId").Value}{GetEnvironmentSegment(environment)}".ToUri()
    })
    {
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
        return new(error.Message);
    }

    private static string GetEnvironmentSegment(string? environment) =>
        string.IsNullOrWhiteSpace(environment) ? string.Empty : $"/environments/{environment}/";

    public async Task<IEnumerable<T>> Paginate<T>(ContentfulRestRequest request)
    {
        var result = new List<T>();
        var total = -1;
        while(result.Count != total)
        {
            request.AddOrUpdateParameter("skip", result.Count.ToString());
            var res = await ExecuteWithErrorHandling<ItemWrapper<T>>(request);
            total = res.Total;
            if (res.Items != null)
                result.AddRange(res.Items);
        }

        return result;
                
    }
}