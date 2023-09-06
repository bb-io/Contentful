using System.Text;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Identifiers;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Newtonsoft.Json;
using Contentful.Core.Extensions;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public EntryActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Get text content of entry's field", Description = "Get the text content of the field of the specified " +
                                                               "entry. Field can be plain text or rich text. In " +
                                                               "both cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetFieldTextContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);

        var entry = await client.GetEntry(entryIdentifier.Id);
        var field = ((JObject)entry.Fields)[fieldIdentifier.Id][localeIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.Id).Type;
        string textContent;

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
    public async Task SetTextContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Text")] string text)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.Id] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>
            { { localeIdentifier.Locale, text } }));
        await client.CreateOrUpdateEntry(entry,
            version: client.GetEntry(entryIdentifier.Id).Result.SystemProperties.Version);
    }

    [Action("Get entry number content", Description = "Get entry number content by field id")]
    public async Task<GetNumberContentResponse> GetNumberContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        return new GetNumberContentResponse
        {
            NumberContent = fields[fieldIdentifier.Id][localeIdentifier.Locale].ToInt()
        };
    }

    [Action("Set entry number content", Description = "Set entry number content by field id")]
    public async Task SetNumberContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Number")] int number)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.Id] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, int>
            { { localeIdentifier.Locale, number } }));
        await client.CreateOrUpdateEntry(entry,
            version: client.GetEntry(entryIdentifier.Id).Result.SystemProperties.Version);
    }

    [Action("Get entry boolean content", Description = "Get entry boolean content by field id")]
    public async Task<GetBoolContentResponse> GetBoolContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        return new GetBoolContentResponse
        {
            BooleanContent = fields[fieldIdentifier.Id][localeIdentifier.Locale].ToObject<bool>()
        };
    }

    [Action("Set entry boolean content", Description = "Set entry boolean content by field id")]
    public async Task SetBoolContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Boolean")] bool boolean)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.Id] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, bool>
            { { localeIdentifier.Locale, boolean } }));
        await client.CreateOrUpdateEntry(entry,
            version: client.GetEntry(entryIdentifier.Id).Result.SystemProperties.Version);
    }

    [Action("Get entry media content", Description = "Get entry media content by field id")]
    public async Task<AssetIdentifier> GetMediaContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        return new AssetIdentifier
        {
            Id = fields[fieldIdentifier.Id][localeIdentifier.Locale]["sys"]["id"].ToString()
        };
    }

    [Action("Set entry media content", Description = "Set entry media content by field id")]
    public async Task SetMediaContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var payload = new
        {
            sys = new
            {
                type = "Link",
                linkType = "Asset",
                id = assetIdentifier.Id
            }
        };
        var entry = await client.GetEntry(entryIdentifier.Id);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.Id] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, object>
            { { localeIdentifier.Locale, payload } }));
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    [Action("Add new entry", Description = "Add new entry by content model id")]
    public async Task<EntryIdentifier> AddNewEntry([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var result = await client.CreateEntry(new Entry<dynamic>(), contentModelIdentifier.Id);
        return new EntryIdentifier
        {
            Id = result.SystemProperties.Id
        };
    }

    [Action("Delete entry", Description = "Delete entry by id")]
    public async Task DeleteEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        await client.DeleteEntry(entryIdentifier.Id, version: (int)entry.SystemProperties.Version);
    }

    [Action("Publish entry", Description = "Publish entry by id")]
    public async Task PublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        await client.PublishEntry(entryIdentifier.Id, version: (int)entry.SystemProperties.Version);
    }

    [Action("Unpublish entry", Description = "Unpublish entry by id")]
    public async Task UnpublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.Id);
        await client.UnpublishEntry(entryIdentifier.Id, version: (int)entry.SystemProperties.Version);
    }
}