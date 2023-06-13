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
using Contentful.Core.Models.Management;
using File = Contentful.Core.Models.File;
using Newtonsoft.Json;

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
            var entry = client.GetEntry(input.EntryId).Result;
            JObject fields = (JObject)entry.Fields;
            fields[input.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>() { { input.Locale, input.Text } }));
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version).Wait();
        }

        [Action("Get entry media content", Description = "Get entry media content by field id")]
        public GetMediaContentResponse GetMediaContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            return new GetMediaContentResponse()
            {
                MediaId = fields[input.FieldId][input.Locale]["sys"]["id"].ToString(),
            };
        }

        [Action("Set entry media content", Description = "Set entry media content by field id")]
        public void SetMediaContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SetMediaRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var payload = new
            {
                sys = new
                {
                    type = "Link",
                    linkType = "Asset",
                    id = input.MediaId
                }
            };
            var entry = client.GetEntry(input.EntryId).Result;
            JObject fields = (JObject)entry.Fields;
            fields[input.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { input.Locale, payload }
            }));
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version).Wait();
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

        [Action("Create and upload asset", Description = "Create and upload asset")]
        public CreateAssetResponse CreateAsset(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateAssetRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var result = client.UploadFileAndCreateAsset(new ManagementAsset()
            {
                SystemProperties = new SystemProperties() { Id = Guid.NewGuid().ToString() },
                Title = new Dictionary<string, string>()
                            {
                                { input.Locale, input.Title }
                            },
                Description = new Dictionary<string, string>() { { input.Locale, input.Description } },
                Files = new Dictionary<string, File>()
                {
                    { input.Locale, new File(){ FileName = input.Filename, ContentType = "text/plain"} }
                },
            }, input.File).Result;

            return new CreateAssetResponse()
            {
                AssetId = result.SystemProperties.Id
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
