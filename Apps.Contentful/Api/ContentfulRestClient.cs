using Apps.Contentful.Constants;
using Apps.Contentful.Models.Exceptions;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Wrappers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient(AuthenticationCredentialsProvider[] creds, string? environment)
    : BlackBirdRestClient(new()
    {
        BaseUrl =
            $"{creds.Get(CredNames.BaseUrl).Value}/spaces/{creds.Get(CredNames.SpaceId).Value}{GetEnvironmentSegment(environment)}"
                .ToUri()
    })
{
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<JObject>(response.Content);
        var details = error["details"];
        
        var errorMessages = new List<string>();

        if (details != null)
        {
            if (details.Type == JTokenType.Object && details["errors"] != null)
            {
                foreach (var errorItem in details["errors"])
                {
                    foreach (var property in errorItem.Children<JProperty>())
                    {
                        var field = property.Name;
                        var message = property.Value["message"]?.ToString();

                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessages.Add($"{field}: {message}");
                        }
                    }
                }
            }
            else if (details.Type == JTokenType.String)
            {
                var detailMessage = details.ToString();
                if (!string.IsNullOrEmpty(detailMessage))
                {
                    errorMessages.Add(detailMessage);
                }
            }
            else
            {
                errorMessages.Add($"Unexpected error details format: {details}");
            }
        }

        var fullMessage = error["message"]?.ToString() ?? "Unknown error";
        return new ApiValidationException(fullMessage, errorMessages);
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