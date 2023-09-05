using System.Text;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using RestSharp;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using File = Contentful.Core.Models.File;
using Newtonsoft.Json;
using Contentful.Core.Extensions;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get text content of entry's field", Description = "Get the text content of the field of the specified " +
                                                                   "entry. Field can be plain text or rich text. In " +
                                                                   "both cases plain text is returned.")]
        public async Task<GetTextContentResponse> GetFieldTextContent(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);

            var entry = await client.GetEntry(input.EntryId);
            var field = ((JObject)entry.Fields)[input.FieldId][input.Locale];
            string textContent;
            
            var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
            var contentType = client.GetContentType(contentTypeId).Result;
            var fieldType = contentType.Fields.First(f => f.Id == input.FieldId).Type;

            if (fieldType == "RichText")
            {
                var content = (JArray)field["content"];
                var result = new StringBuilder();
                foreach (var item in content)
                {
                    var text = string.Join("", item["content"].Select(c => c["value"]));
                    result.Append(text);
                }

                textContent = result.ToString();
            }

            else
                textContent = field.ToString();

            return new GetTextContentResponse { TextContent = textContent };
        }

        [Action("Set entry text content", Description = "Set entry text content by field id")]
        public void SetTextContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SetTextRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var entry = client.GetEntry(input.EntryId).Result;
            JObject fields = (JObject)entry.Fields;
            fields[input.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>()
                { { input.Locale, input.Text } }));
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Get entry number content", Description = "Get entry number content by field id")]
        public GetNumberContentResponse GetNumberContent(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            return new GetNumberContentResponse()
            {
                NumberContent = fields[input.FieldId][input.Locale].ToInt(),
            };
        }

        [Action("Set entry number content", Description = "Set entry number content by field id")]
        public void SetNumberContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SetNumberRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var entry = client.GetEntry(input.EntryId).Result;
            JObject fields = (JObject)entry.Fields;
            fields[input.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, int>()
                { { input.Locale, input.Number } }));
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Get entry boolean content", Description = "Get entry boolean content by field id")]
        public GetBoolContentResponse GetBoolContent(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var fields = (JObject)(client.GetEntry(input.EntryId).Result.Fields);
            return new GetBoolContentResponse()
            {
                BooleanContent = fields[input.FieldId][input.Locale].ToObject<bool>(),
            };
        }

        [Action("Set entry boolean content", Description = "Set entry boolean content by field id")]
        public void SetBoolContent(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SetBoolRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var entry = client.GetEntry(input.EntryId).Result;
            JObject fields = (JObject)entry.Fields;
            fields[input.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, bool>()
                { { input.Locale, input.Boolean } }));
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Get entry media content", Description = "Get entry media content by field id")]
        public GetMediaContentResponse GetMediaContent(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
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
            client.CreateOrUpdateEntry(entry, version: client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Get all content types", Description = "Get all content types in space")]
        public GetAllContentTypesResponse GetAllContentTypes(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
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
        public GetAssetResponse GetAssetById(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetAssetRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var asset = client.GetAsset(input.AssetId).Result;
            var fileData = asset.Files?[input.Locale];

            var fileContent = DownloadFileByUrl(fileData);
            return new GetAssetResponse()
            {
                Title = asset.Title?[input.Locale],
                Description = asset.Description?[input.Locale],
                File = fileContent,
            };
        }

        [Action("Create and upload asset", Description = "Create and upload asset")]
        public CreateAssetResponse CreateAsset(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
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
                    {
                        input.Locale,
                        new File() { FileName = input.Filename ?? input.File.Name, ContentType = "text/plain" }
                    }
                },
            }, input.File.Bytes).Result;

            return new CreateAssetResponse()
            {
                AssetId = result.SystemProperties.Id
            };
        }

        [Action("Update asset file", Description = "Update asset file")]
        public async void UpdateAssetFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UpdateAssetFileRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var oldAsset = await client.GetAsset(input.AssetId);
            var uploadReference = client.UploadFile(input.File.Bytes).Result;
            uploadReference.SystemProperties.CreatedAt = null;
            uploadReference.SystemProperties.CreatedBy = null;
            uploadReference.SystemProperties.Space = null;
            uploadReference.SystemProperties.LinkType = "Upload";

            oldAsset.Files.Add(input.Locale,
                new File()
                {
                    FileName = input.Filename ?? input.File.Name, ContentType = "text/plain",
                    UploadReference = uploadReference
                });
            await client.CreateOrUpdateAsset(new ManagementAsset()
            {
                SystemProperties = new SystemProperties() { Id = input.AssetId },
                Title = oldAsset.Title,
                Description = oldAsset.Description,
                Files = oldAsset.Files
            }, version: oldAsset.SystemProperties.Version);
            await client.ProcessAsset(input.AssetId, (int)oldAsset.SystemProperties.Version, input.Locale);
        }

        [Action("Add new entry", Description = "Add new entry by content model id")]
        public AddNewEntryResponse AddNewEntry(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] AddNewEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var result = client.CreateEntry(new Entry<dynamic>(), input.ContentModelId).Result;
            return new AddNewEntryResponse()
            {
                EntryId = result.SystemProperties.Id
            };
        }

        [Action("Delete entry", Description = "Delete entry by id")]
        public void DeleteEntry(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.DeleteEntry(input.EntryId, (int)client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Publish entry", Description = "Publish entry by id")]
        public void PublishEntry(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] PublishEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.PublishEntry(input.EntryId, (int)client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Unpublish entry", Description = "Unpublish entry by id")]
        public void UnpublishEntry(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] PublishEntryRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.UnpublishEntry(input.EntryId, (int)client.GetEntry(input.EntryId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Publish asset", Description = "Publish asset by id")]
        public void PublishAsset(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] PublishAssetRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.PublishAsset(input.AssetId, (int)client.GetAsset(input.AssetId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Unpublish asset", Description = "Unpublish asset by id")]
        public void UnpublishAsset(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] PublishAssetRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            client.UnpublishAsset(input.AssetId, (int)client.GetAsset(input.AssetId).Result.SystemProperties.Version)
                .Wait();
        }

        [Action("Is asset locale present", Description = "Is asset locale present")]
        public IsAssetLocalePresentResponse IsAssetLocalePresent(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] IsAssetLocalePresentRequest input)
        {
            var client = new ContentfulClient(authenticationCredentialsProviders, input.SpaceId);
            var asset = client.GetAsset(input.AssetId).Result;
            if (asset.Files.TryGetValue(input.Locale, out var file))
            {
                return new IsAssetLocalePresentResponse() { IsAssetLocalePresent = 1 };
            }

            return new IsAssetLocalePresentResponse() { IsAssetLocalePresent = 0 };
        }

        private Blackbird.Applications.Sdk.Common.Files.File? DownloadFileByUrl(File? file)
        {
            if (file is null)
                return null;

            var client = new RestClient();
            var request = new RestRequest($"https:{file.Url}");
            
            return new Blackbird.Applications.Sdk.Common.Files.File(client.Get(request).RawBytes)
            {
                Name = file.FileName,
                ContentType = file.ContentType
            };;
        }

        // private async Task<string> GetContentType(string contentTypeId, string spaceId)
        // {
        //     var client = new RestClient(new RestClientOptions { BaseUrl = new Uri("https://api.contentful.com") });
        //     var request = new RestRequest($"/spaces/{spaceId}/content_types/{contentTypeId}");
        //     
        // }
    }
}