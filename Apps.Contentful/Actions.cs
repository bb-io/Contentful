using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Contentful.Core.Configuration;
using Contentful.Core;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get content", Description = "Get content by id")]
        public GetEntryResponse GetContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] GetEntryRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            
            return new GetEntryResponse()
            {
                TestText = fields["testText"][input.Locale].ToString(),
                TestBoolean = (bool)fields["testBoolean"][input.Locale]
            };
        }

        [Action("Get all content types", Description = "Get all content types in space")]
        public GetAllContentTypesResponse GetAllContentTypes(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetContentTypesRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var contentTypes = client.GetContentTypes().Result;
            var contentTypeDtos = contentTypes.Select(t => new ContentTypeDto() { Name = t.Name }).ToList();
            return new GetAllContentTypesResponse
            {
                ContentTypes = contentTypeDtos
            };
        }

        [Action("Get asset", Description = "Get asset by Id")]
        public GetAssetResponse GetAssetById(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetAssetRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var asset = client.GetAsset(input.AssetId).Result;

            return new GetAssetResponse()
            {
                Title = asset.Title.First().Value,
                Description = asset.Description.First().Value,
                FileSize = asset.Files.First().Value.Details.Size
            };
        }

        private ContentfulManagementClient GetContentfulClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, string spaceId)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;

            var httpClient = new HttpClient();
            var options = new ContentfulOptions
            {
                ManagementApiKey = accessToken.Split(' ')[1],
                SpaceId = spaceId
            };
            var client = new ContentfulManagementClient(httpClient, options);
            return client;
        }
    }
}
