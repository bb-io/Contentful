using System.Text;
using Apps.Contentful.Api;
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
using Newtonsoft.Json.Serialization;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Apps.Contentful.DataSourceHandlers;

namespace Apps.Contentful.Actions;

[ActionList("Entry fields")]
public class EntryFieldActions(InvocationContext invocationContext) : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    [Action("Get first not empty entry's text field", Description =
       "Get the first not empty text content of the field of the specified entry. " +
       "Field can be short text, long text or rich text. In all cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetFirstNotEmptyTextFieldContent(
       [ActionParameter] EntryLocaleIdentifier entryIdentifier,
       [ActionParameter] GetFirstNotEmptyTextFieldContentRequest request)
    {
        entryIdentifier.Validate();

        foreach (var fieldId in request.FieldIds)
        {
            try
            {
                var result = await GetTextFieldContent(entryIdentifier, new FieldIdentifier { FieldId = fieldId });
                if (!string.IsNullOrEmpty(result.TextContent))
                    return result;
            }
            catch (Exception)
            {
                continue;
            }
        }

        return new GetTextContentResponse { TextContent = null };
    }

    [Action("Get entry's text field", Description =
        "Get the text content of the field of the specified entry. " +
        "Field can be short text, long text or rich text. In all " +
        "cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId]?[entryIdentifier.Locale];

        if (field is null)
            return new()
            {
                TextContent = null
            };

        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
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
        else if (fieldType == "Text" || fieldType == "Symbol")
        {
            textContent = field.ToString();
        }
        else
        {
            throw new PluginMisconfigurationException(
                "The specified field must be of the short text, long text or rich text type.");
        }

        return new GetTextContentResponse { TextContent = textContent, MissingLocales = await GetMissingLocales(entryIdentifier, fieldIdentifier, entry) };
    }

    [Action("Set entry's text field", Description =
        "Set content of the field of the specified entry. Field " +
        "can be short text, long text or rich text.")]
    public async Task SetTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter][Display("Text")] string text)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));

        if (entry.Fields == null)
        {
            throw new PluginApplicationException($"Entry with ID {entryIdentifier.EntryId} has no fields.");
        }

        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));

        if (contentType.Fields == null)
        {
            throw new PluginApplicationException($"Content type {contentTypeId} has no fields.");
        }

        var field = contentType.Fields.FirstOrDefault(f => f.Id == fieldIdentifier.FieldId);

        if (field == null)
        {
            throw new PluginApplicationException($"Field with ID {fieldIdentifier.FieldId} not found in content type {contentTypeId}.");
        }
        var fieldType = field.Type;

        if (fields[fieldIdentifier.FieldId] == null)
        {
            fields[fieldIdentifier.FieldId] = new JObject();
        }

        if (fieldType == "RichText")
        {
            var html = $"<p>{text}</p>";
            var htmlToRichTextConverter = new HtmlToRichTextConverter();
            var richText = htmlToRichTextConverter.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            fields[fieldIdentifier.FieldId][entryIdentifier.Locale] =
                JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
        {
            fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = text;
        }
        else
        {
            throw new PluginMisconfigurationException(
                "The specified field must be of the short text, long text or rich text type.");
        }

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    [Action("Get entry's array field", Description =
    "Get the array content of the specified field of the specified entry. " +
    "Field must be of type Array. Returns the array values as a list of strings.")]
    public async Task<GetArrayContentResponse> GetArrayFieldContent(
    [ActionParameter] EntryLocaleIdentifier entryIdentifier,
    [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId]?[entryIdentifier.Locale];

        if (field is null)
            return new()
            {
                ArrayContent = null
            };

        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;

        if (fieldType != "Array")
        {
            throw new PluginMisconfigurationException(
                "The specified field must be of type Array.");
        }

        var arrayContent = new List<string>();
        if (field is JArray jArray)
        {
            arrayContent = jArray.Select(item => item.ToString()).ToList();
        }

        return new GetArrayContentResponse
        {
            ArrayContent = arrayContent,
            MissingLocales = await GetMissingLocales(entryIdentifier, fieldIdentifier, entry)
        };
    }

    [Action("Get entry's number field", Description = "Get entry's number field value by field ID.")]
    public async Task<GetNumberContentResponse> GetNumberFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        if (fields.TryGetValue(fieldIdentifier.FieldId, out JToken fieldToken) == false
           || fieldToken[entryIdentifier.Locale] == null)
        {
            var availableFields = fields.Properties().Select(f => f.Name);
            throw new PluginApplicationException($"Field '{fieldIdentifier.FieldId}' or locale '{entryIdentifier.Locale}' not found in entry {entryIdentifier.EntryId}. " +
                                                 $"Available fields: {string.Join(", ", availableFields)}. Please check your input and try again");
        }

        return new GetNumberContentResponse
        {
            NumberContent = fields[fieldIdentifier.FieldId][entryIdentifier.Locale].Value<double>()
        };
    }

    [Action("Set entry's number field", Description = "Set entry's number field value by field ID.")]
    public async Task SetNumberFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter][Display("Number")] int number)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = number;
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    [Action("Get entry's boolean field", Description = "Get entry's boolean field by field ID.")]
    public async Task<GetBoolContentResponse> GetBoolFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;

        if (fields == null
        || !fields.TryGetValue(fieldIdentifier.FieldId, out JToken fieldToken)
        || fieldToken[entryIdentifier.Locale] == null)
        {
            throw new PluginApplicationException($"Field '{fieldIdentifier.FieldId}' or locale '{entryIdentifier.Locale}' not found in entry {entryIdentifier.EntryId}. Please check your input and try again");
        }

        bool booleanValue = fieldToken[entryIdentifier.Locale].ToObject<bool>();
        return new GetBoolContentResponse { BooleanContent = booleanValue };
    }

    [Action("Set entry's boolean field", Description = "Set entry's boolean field by field ID.")]
    public async Task SetBoolFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter][Display("Boolean")] bool boolean)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = boolean;
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    [Action("Get entry's media field", Description = "Get entry's media content by field ID.")]
    public async Task<EntryMediaContentResponse> GetMediaFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        entryIdentifier.Validate();

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;

        var assetId = fields[fieldIdentifier.FieldId][entryIdentifier.Locale]["sys"]["id"].ToString();
        var asset = await client.ExecuteWithErrorHandling(async () => await client.GetAsset(assetId));

        if (!asset.Files.TryGetValue(entryIdentifier.Locale, out var fileData))
        {
            throw new PluginMisconfigurationException("No asset with the provided locale found.");
        }

        return new()
        {
            AssetId = assetId,
            File = new(new HttpRequestMessage(HttpMethod.Get, $"https:{fileData.Url}"), fileData.FileName,
                fileData.ContentType),
            MissingLocales = await GetMissingLocales(entryIdentifier, fieldIdentifier, entry),
        };
    }

    [Action("Set entry's media field", Description = "Set entry's media field by field ID.")]
    public async Task SetMediaFieldContent(
        [ActionParameter] AssetEntryLocaleIdentifier assetEntryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, assetEntryIdentifier.Environment);
        var payload = new
        {
            sys = new
            {
                type = "Link",
                linkType = "Asset",
                id = assetEntryIdentifier.AssetId
            }
        };
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(assetEntryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;

        fields[fieldIdentifier.FieldId] ??= new JObject();
        fields[fieldIdentifier.FieldId][assetEntryIdentifier.Locale] =
            JObject.Parse(JsonConvert.SerializeObject(payload));
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    [Action("Set entry's reference field", Description = "Set entry's reference field by field ID.")]
    public async Task SetReferenceFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryLocaleIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter, Display("Referenced entry ID"), DataSource(typeof(EntryLocaleDataSourceHandler))] string referencedEntryId)
    {
        if (string.IsNullOrEmpty(referencedEntryId))
        {
            throw new PluginMisconfigurationException($"Referenced entry ID is null or empty. Please check your input and try again");
        }
        
        var client = new ContentfulClient(Creds, entryLocaleIdentifier.Environment);
        var payload = new
        {
            sys = new
            {
                type = "Link",
                linkType = "Entry",
                id = referencedEntryId
            }
        };

        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryLocaleIdentifier.EntryId));
        var fields = (JObject)entry.Fields;

        fields[fieldIdentifier.FieldId] ??= new JObject();
        fields[fieldIdentifier.FieldId][entryLocaleIdentifier.Locale] = JObject.Parse(JsonConvert.SerializeObject(payload));

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    private async Task<List<string>> GetMissingLocales(EntryLocaleIdentifier entryIdentifier, FieldIdentifier fieldIdentifier, Entry<dynamic> entry)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, entryIdentifier.Environment);
        var locales = await client.ExecuteWithErrorHandling(async () => await client.GetLocalesCollection());

        var availableLocales = locales.Select(l => l.Code);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId];
        IEnumerable<string> missingLocales;

        if (field == null)
            missingLocales = availableLocales;
        else
        {
            var fieldDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(field.ToString());
            var presentLocales = fieldDictionary.Select(f => f.Key);
            missingLocales = availableLocales.Except(presentLocales);
        }

        return missingLocales.ToList();
    }

}

