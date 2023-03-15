using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Contentful.Core.Configuration;
using Contentful.Core;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get content", Description = "Get content by id")]
        public GetEntryResponse GetContent(string deliveryApiKey, string previewApiKey, string managementApiKey, 
            AuthenticationCredentialsProvider authenticationCredentialsProvider, 
            [ActionParameter] GetEntryRequest input)
        {
            var client = GetContentfulPreviewClient(deliveryApiKey, previewApiKey, authenticationCredentialsProvider.Value);
            var testDto = client.GetEntry<TestDto>(input.EntryId).Result;
            return new GetEntryResponse()
            {
                TestDto = testDto
            };
        }

        [Action("Get all content types", Description = "Get all content types in space")]
        public GetAllContentTypesResponse GetAllContentTypes(string deliveryApiKey, string previewApiKey, string managementApiKey,
            AuthenticationCredentialsProvider authenticationCredentialsProvider)
        {
            var client = GetContentfulPreviewClient(deliveryApiKey, previewApiKey, authenticationCredentialsProvider.Value);
            var contentTypes = client.GetContentTypes().Result;
            
            return new GetAllContentTypesResponse
            {
                ContentTypes = contentTypes.Select(x => x.Name).ToList()
            };
        }

        [Action("Get asset", Description = "Get asset by Id")]
        public GetAssetResponse GetAssetById(string deliveryApiKey, string previewApiKey, string managementApiKey, 
            AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] GetAssetRequest input)
        {
            var client = GetContentfulPreviewClient(deliveryApiKey, previewApiKey, authenticationCredentialsProvider.Value);
            var asset = client.GetAsset(input.AssetId).Result;

            return new GetAssetResponse()
            {
                Title = asset.Title,
                Description = asset.Description,
                FileSize = asset.File.Details.Size
            };
        }

        /*
        [Action("Create content", Description = "Create content")]
        public GetEntryResponse CreateContent(string deliveryApiKey, string previewApiKey, string managementApiKey, 
            AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] CreateEntryRequest input)
        {
            var client = GetContentfulManagementClient(managementApiKey, authenticationCredentialsProvider.Value);
            var testDto = client.CreateEntry(input.Content, input.ContentTypeId).Result;
            return new GetEntryResponse()
            {
                TestDto = testDto
            };
        }
        */

        private ContentfulClient GetContentfulPreviewClient(string deliveryApiKey, string previewApiKey, string spaceId)
        {
            var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer *oauth_access_token*");
            var options = new ContentfulOptions
            {
                DeliveryApiKey = deliveryApiKey,
                PreviewApiKey = previewApiKey,
                SpaceId = spaceId
            };
            var client = new ContentfulClient(httpClient, options);
            return client;
        }

        private ContentfulManagementClient GetContentfulManagementClient(string deliveryApiKey, string spaceId)
        {
            var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer *oauth_access_token*");
            var client = new ContentfulManagementClient(httpClient, deliveryApiKey, spaceId);
            return client;
        }
    }
}
