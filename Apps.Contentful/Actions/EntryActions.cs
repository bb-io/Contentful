using System.Net.Mime;
using System.Text;
using Apps.Contentful.HtmlHelpers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Newtonsoft.Json;
using Contentful.Core.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json.Serialization;
using File = Blackbird.Applications.Sdk.Common.Files.File;
using HtmlRenderer = Apps.Contentful.HtmlHelpers.HtmlRenderer;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public EntryActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    #region Text/Rich text fields

    [Action("Get entry's text/rich text field", Description = "Get the text content of the field of the specified entry. " +
                                                              "Field can be plain text or rich text. In both cases plain " +
                                                              "text is returned.")]
    public async Task<GetTextContentResponse> GetTextFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][localeIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
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
        else if (fieldType == "Text")
            textContent = field.ToString();
        else
            throw new Exception("The specified field must be of the text or rich text type."); 

        return new GetTextContentResponse { TextContent = textContent };
    }
    
    [Action("Get entry's text/rich text field as HTML file", Description = "Get the text content of the field of the " +
                                                                           "specified entry as HTML file. Field can be " +
                                                                           "plain text or rich text. In both cases HTML " +
                                                                           "file is returned.")]
    public async Task<FileResponse> GetTextFieldContentAsHtmlFile(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][localeIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        string html;

        if (fieldType == "RichText")
        {
            var content = (JArray)field["content"];
            var spaceId = Creds.First(p => p.KeyName == "spaceId").Value;
            var htmlRenderer = new HtmlRenderer(content, spaceId);
            html = htmlRenderer.ToHtml();
        }
        else if (fieldType == "Text")
            html = $"<p>{field}</p>";
        else
            throw new Exception("The specified field must be of the text or rich text type."); 

        html = $"<html><body>{html}</body></html>";

        return new FileResponse
        {
            File = new File(Encoding.UTF8.GetBytes(html))
            {
                Name = $"{entryIdentifier.EntryId}_{fieldIdentifier.FieldId}_{localeIdentifier.Locale}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }

    [Action("Set entry's text/rich text field", Description = "Set content of the field of the specified entry. Field " +
                                                              "can be plain text or rich text.")]
    public async Task SetTextFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Text")] string text)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;

        if (fieldType == "RichText")
        {
            var html = $"<p>{text}</p>";
            var documentRenderer = new RichTextRenderer();
            var richText = documentRenderer.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, Document>
                { { localeIdentifier.Locale, richText } }, serializerSettings));
        }
        else if (fieldType == "Text") 
            fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>
                { { localeIdentifier.Locale, text } }));
        else
            throw new Exception("The specified field must be of the text or rich text type.");
        
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    [Action("Set entry's text/rich text field from HTML file", Description = "Set content of the field of the specified " +
                                                                             "entry from HTML file. Field can be plain " +
                                                                             "text or rich text. For plain text only the " +
                                                                             "text extracted from HTML is put in the field. " +
                                                                             "For rich text all HTML structure is preserved.")]
    public async Task SetTextFieldContentFromHtml(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] FileRequest input)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        var html = Encoding.UTF8.GetString(input.File.Bytes);

        if (fieldType == "RichText")
        {
            var documentRenderer = new RichTextRenderer();
            var richText = documentRenderer.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, Document>
                { { localeIdentifier.Locale, richText } }, serializerSettings));
            
        }
        else if (fieldType == "Text")
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var text = string.Join("", htmlDocument.DocumentNode.SelectNodes("//text()").Select(node => node.InnerText));
            fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>
                { { localeIdentifier.Locale, text } }));
        }
        else
            throw new Exception("The specified field must be of the text or rich text type."); 

        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    #endregion

    #region Number fields

    [Action("Get entry's number field", Description = "Get entry's number field value by field ID.")]
    public async Task<GetNumberContentResponse> GetNumberFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetNumberContentResponse
        {
            NumberContent = fields[fieldIdentifier.FieldId][localeIdentifier.Locale].ToInt()
        };
    }
    
    [Action("Set entry's number field", Description = "Set entry's number field value by field ID.")]
    public async Task SetNumberFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Number")] int number)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, int>
            { { localeIdentifier.Locale, number } }));
        await client.CreateOrUpdateEntry(entry,
            version: client.GetEntry(entryIdentifier.EntryId).Result.SystemProperties.Version);
    }
    
    #endregion

    #region Boolean fields

    [Action("Get entry's boolean field", Description = "Get entry's boolean field by field ID.")]
    public async Task<GetBoolContentResponse> GetBoolFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetBoolContentResponse
        {
            BooleanContent = fields[fieldIdentifier.FieldId][localeIdentifier.Locale].ToObject<bool>()
        };
    }

    [Action("Set entry's boolean field", Description = "Set entry's boolean field by field ID.")]
    public async Task SetBoolFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Boolean")] bool boolean)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, bool>
            { { localeIdentifier.Locale, boolean } }));
        await client.CreateOrUpdateEntry(entry,
            version: client.GetEntry(entryIdentifier.EntryId).Result.SystemProperties.Version);
    }
    
    #endregion

    #region Media fields

    [Action("Get entry's media content", Description = "Get entry's media content by field ID.")]
    public async Task<AssetIdentifier> GetMediaFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new AssetIdentifier
        {
            AssetId = fields[fieldIdentifier.FieldId][localeIdentifier.Locale]["sys"]["id"].ToString()
        };
    }

    [Action("Set entry's media field", Description = "Set entry's media field by field ID.")]
    public async Task SetMediaFieldContent(
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
                id = assetIdentifier.AssetId
            }
        };
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId] = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, object>
            { { localeIdentifier.Locale, payload } }));
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    #endregion

    #region Entries

    [Action("List entries", Description = "List all entries. If a content model is specified, only entries that have " +
                                          "this content model are listed.")]
    public async Task<ListEntriesResponse> ListEntries([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var queryString = $"?content_type={contentModelIdentifier.ContentModelId}";
        var entries = await client.GetEntriesCollection<Entry<dynamic>>(queryString);
        return new ListEntriesResponse
        {
            Entries = entries.Select(e => new EntryIdentifier { EntryId = e.SystemProperties.Id } )
        };
    }
    
    [Action("Add new entry", Description = "Add new entry with specified content model.")]
    public async Task<EntryIdentifier> AddNewEntry([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var result = await client.CreateEntry(new Entry<dynamic>(), contentModelIdentifier.ContentModelId);
        return new EntryIdentifier
        {
            EntryId = result.SystemProperties.Id
        };
    }

    [Action("Delete entry", Description = "Delete specified entry.")]
    public async Task DeleteEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.DeleteEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Publish entry", Description = "Publish specified entry.")]
    public async Task PublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.PublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Unpublish entry", Description = "Unpublish specified entry.")]
    public async Task UnpublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.UnpublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }
    
    #endregion
}