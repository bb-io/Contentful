using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Contentful.Core.Configuration;
using Contentful.Core;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get content", Description = "Get content by id")]
        public GetEntryResponse GetContent(AuthenticationCredentialsProvider authenticationCredentialsProvider, 
            [ActionParameter] GetEntryRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProvider.Value, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            
            return new GetEntryResponse()
            {
                TestText = fields["testText"][input.Locale].ToString(),
                TestBoolean = (bool)fields["testBoolean"][input.Locale]
            };
        }

        [Action("Get all content types", Description = "Get all content types in space")]
        public GetAllContentTypesResponse GetAllContentTypes(AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] GetContentTypesRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProvider.Value, input.SpaceId);
            var contentTypes = client.GetContentTypes().Result;
            var contentTypeDtos = contentTypes.Select(t => new ContentTypeDto() { Name = t.Name }).ToList();
            return new GetAllContentTypesResponse
            {
                ContentTypes = contentTypeDtos
            };
        }

        [Action("Get asset", Description = "Get asset by Id")]
        public GetAssetResponse GetAssetById(AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] GetAssetRequest input)
        {
            var client = GetContentfulClient(authenticationCredentialsProvider.Value, input.SpaceId);
            var asset = client.GetAsset(input.AssetId).Result;

            return new GetAssetResponse()
            {
                Title = asset.Title.First().Value,
                Description = asset.Description.First().Value,
                FileSize = asset.Files.First().Value.Details.Size
            };
        }

        private ContentfulManagementClient GetContentfulClient(string accessToken, string spaceId)
        {     
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
