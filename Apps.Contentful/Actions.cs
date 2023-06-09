using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Contentful.Core.Configuration;
using Contentful.Core;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using RestSharp;
using Contentful.Core.Models;
using System.Dynamic;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get entry text content", Description = "Get entry text content by field id")]
        public GetTextContentResponse GetTextContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
            [ActionParameter] GetEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            
            return new GetTextContentResponse()
            {
                TextContent = fields[input.FieldId][input.Locale].ToString(),
            };
        }

        [Action("Set entry text content", Description = "Set entry text content by field id")]
        public void SetTextContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SetTextRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.CreateOrUpdateEntry(new Entry<dynamic>()
            {
                SystemProperties = new SystemProperties() { Id = input.EntryId },
                Fields = new Dictionary<string, Object>() { { input.FieldId, 
                        new Dictionary<string, string>() { { input.Locale, input.Text } } 
                } }
            }, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version).Wait();
        }

        [Action("Get all content types", Description = "Get all content types in space")]
        public GetAllContentTypesResponse GetAllContentTypes(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetContentTypesRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
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
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var asset = client.GetAsset(input.AssetId).Result;
            var file = asset.Files.First();
            return new GetAssetResponse()
            {
                Title = asset.Title.FirstOrDefault().Value,
                Description = asset.Description.FirstOrDefault().Value,
                Filename = file.Value.FileName,
                File = DownloadFileByUrl(file.Value.Url),
            };
        }

        private byte[] DownloadFileByUrl(string url)
        {
            var client = new RestClient();
            var request = new RestRequest($"https:{url}", Method.Get);
            return client.Get(request).RawBytes;
        }    
    }
}
