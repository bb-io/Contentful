using System.Net.Mime;
using System.Text;
using Apps.Contentful.Api;
using Apps.Contentful.Dtos.Raw;
using Apps.Contentful.Extensions;
using Apps.Contentful.HtmlHelpers;
using Apps.Contentful.Models;
using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Contentful.Core.Models;
using Newtonsoft.Json;
using Contentful.Core.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    #region Text/Rich text fields

    [Action("Get entry's text/rich text field", Description =
        "Get the text content of the field of the specified entry. " +
        "Field can be short text, long text or rich text. In all " +
        "cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId]?[entryIdentifier.Locale];

        if (field is null)
            return new()
            {
                TextContent = null
            };

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
        else if (fieldType == "Text" || fieldType == "Symbol")
            textContent = field.ToString();
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type.");

        return new GetTextContentResponse { TextContent = textContent };
    }

    [Action("Get entry's text/rich text field as HTML file",
        Description =
            "Get the text content of the field of the specified entry as HTML file. Field can be short text, long text or rich text. In all cases HTML file is returned.")]
    public async Task<FileResponse> GetTextFieldContentAsHtmlFile(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][entryIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        string htmlContent;

        if (fieldType == "RichText")
        {
            var content = (JArray)field["content"];
            var spaceId = Creds.First(p => p.KeyName == "spaceId").Value;
            var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
            htmlContent = richTextToHtmlConverter.ToHtml();
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
            htmlContent = $"<p>{field}</p>";
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type.");

        string html = $@"
        <html>
        <head>
            <meta name='blackbird-field-id' content='{fieldIdentifier.FieldId}'>
            <meta name='blackbird-entry-id' content='{entryIdentifier.EntryId}'>
            <meta name='blackbird-locale' content='{entryIdentifier.Locale}'>
        </head>
        <body>{htmlContent}</body>
        </html>";

        var file = await fileManagementClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(html)),
            MediaTypeNames.Text.Html,
            $"{entryIdentifier.EntryId}_{fieldIdentifier.FieldId}_{entryIdentifier.Locale}.html");
        return new()
        {
            File = file
        };
    }

    [Action("Set entry's text/rich text field", Description =
        "Set content of the field of the specified entry. Field " +
        "can be short text, long text or rich text.")]
    public async Task SetTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] [Display("Text")] string text)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;

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
            fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = text;
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type.");

        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    [Action("Get IDs from HTML file", Description = "Extract entry and field IDs from HTML file.")]
    public async Task<GetIdsFromHtmlResponse> GetIdsFromHtmlFile([ActionParameter] GetIdsFromFileRequest input)
    {
        var file = await fileManagementClient.DownloadAsync(input.File);
        var html = Encoding.UTF8.GetString(await file.GetByteData());
        var (entryId, fieldId, locale) = ExtractIdsFromHtml(html);

        if (string.IsNullOrEmpty(entryId))
            throw new Exception("Entry ID not found in the HTML file.");

        var client = new ContentfulClient(Creds, input.Environment);
        var entryContent = await GetEntryContent(entryId, client, new List<string>());
        var linkedIds = GetLinkedEntryIds(entryContent, input.Locale, true, true, true, true).Distinct();

        return new GetIdsFromHtmlResponse
        {
            EntryId = entryId ?? string.Empty,
            FieldId = fieldId ?? string.Empty,
            Locale = locale ?? string.Empty,
            LinkedEntryIds = linkedIds.ToList(),
        };
    }

    [Action("Set entry's text/rich text field from HTML file", Description =
        "Set content of the field of the specified " +
        "entry from HTML file. Field can be short " +
        "text, long text or rich text. For short/long " +
        "text only the text extracted from HTML is " +
        "put in the field. For rich text all HTML " +
        "structure is preserved.")]
    public async Task SetTextFieldContentFromHtml(
        [ActionParameter] EntryLocaleOptionalIdentifier entryIdentifier,
        [ActionParameter] FieldOptionalIdentifier fieldIdentifier,
        [ActionParameter] FileRequest input)
    {
        var file = await fileManagementClient.DownloadAsync(input.File);
        var html = Encoding.UTF8.GetString(await file.GetByteData());

        var (extractedEntryId, extractedFieldId, locale) = ExtractIdsFromHtml(html);

        var entryId = entryIdentifier.EntryId ?? extractedEntryId ?? throw new Exception("Entry ID is required.");
        var fieldId = fieldIdentifier.FieldId ?? extractedFieldId ?? throw new Exception("Field ID is required.");

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldId).Type;

        if (fieldType == "RichText")
        {
            var htmlToRichTextConverter = new HtmlToRichTextConverter();
            var richText = htmlToRichTextConverter.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
            fields[fieldId][entryIdentifier.Locale] =
                JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var text = string.Join("",
                htmlDocument.DocumentNode.SelectNodes("//text()").Select(node => node.InnerText));
            fields[fieldId][entryIdentifier.Locale] = text;
        }
        else
        {
            throw new Exception("The specified field must be of the short text, long text or rich text type.");
        }

        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    #endregion

    #region Number fields

    [Action("Get entry's number field", Description = "Get entry's number field value by field ID.")]
    public async Task<GetNumberContentResponse> GetNumberFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetNumberContentResponse
        {
            NumberContent = fields[fieldIdentifier.FieldId][entryIdentifier.Locale].ToInt()
        };
    }

    [Action("Set entry's number field", Description = "Set entry's number field value by field ID.")]
    public async Task SetNumberFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] [Display("Number")] int number)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = number;
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    #endregion

    #region Boolean fields

    [Action("Get entry's boolean field", Description = "Get entry's boolean field by field ID.")]
    public async Task<GetBoolContentResponse> GetBoolFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetBoolContentResponse
        {
            BooleanContent = fields[fieldIdentifier.FieldId][entryIdentifier.Locale].ToObject<bool>()
        };
    }

    [Action("Set entry's boolean field", Description = "Set entry's boolean field by field ID.")]
    public async Task SetBoolFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] [Display("Boolean")] bool boolean)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = boolean;
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    #endregion

    #region Media fields

    [Action("Get entry's media content", Description = "Get entry's media content by field ID.")]
    public async Task<EntryMediaContentResponse> GetMediaFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;

        var assetId = fields[fieldIdentifier.FieldId][entryIdentifier.Locale]["sys"]["id"].ToString();
        var asset = await client.GetAsset(assetId);

        if (!asset.Files.TryGetValue(entryIdentifier.Locale, out var fileData))
        {
            throw new("No asset with the provided locale found");
        }

        return new()
        {
            AssetId = assetId,
            File = new(new HttpRequestMessage(HttpMethod.Get, $"https:{fileData.Url}"), fileData.FileName,
                fileData.ContentType)
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
        var entry = await client.GetEntry(assetEntryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;

        fields[fieldIdentifier.FieldId] ??= new JObject();
        fields[fieldIdentifier.FieldId][assetEntryIdentifier.Locale] =
            JObject.Parse(JsonConvert.SerializeObject(payload));
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    #endregion

    #region Entries

    [Action("Search entries", Description = "Search for all entries. Optionally filter by content model and tags.")]
    public async Task<ListEntriesResponse> ListEntries([ActionParameter] ListEntriesRequest request)
    {
        var client = new ContentfulClient(Creds, request.Environment);
        var queryString = request.ContentModelId == null 
            ? string.Empty 
            : $"?content_type={request.ContentModelId}";
        var entries =
            await client.Paginate<Entry<object>>(
                async (query) => await client.GetEntriesCollection<Entry<object>>(query), queryString);
        
        if (request.Tags is not null && request.Tags.Any())
        {
            entries = entries.Where(e => e.Metadata.Tags.Any(t => request.Tags.Contains(t.Sys.Id)));
        }
        
        var entriesResponse = entries.Select(e => new EntryEntity(e)).ToArray();
        return new ListEntriesResponse(entriesResponse, entriesResponse.Length);
    }
    
    [Action("Get entry", Description = "Get details of a specific entry")]
    public async Task<EntryEntity> GetEntry([ActionParameter] EntryIdentifier input)
    {
        var client = new ContentfulClient(Creds, input.Environment);

        var entry = await client.GetEntry(input.EntryId);
        return new(entry);
    }

    [Action("Add new entry", Description = "Add new entry with specified content model.")]
    public async Task<EntryIdentifier> AddNewEntry([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds, contentModelIdentifier.Environment);
        var result = await client.CreateEntry(new Entry<dynamic>(), contentModelIdentifier.ContentModelId);
        return new EntryIdentifier
        {
            EntryId = result.SystemProperties.Id
        };
    }

    [Action("Delete entry", Description = "Delete specified entry.")]
    public async Task DeleteEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.DeleteEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Publish entry", Description = "Publish specified entry.")]
    public async Task PublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.PublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Unpublish entry", Description = "Unpublish specified entry.")]
    public async Task UnpublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.UnpublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Get locales", Description = "Get the default locale and all other locales in convenient codes form.")]
    public async Task<GetLocalesResponse> GetLocales([ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var locales = await client.GetLocalesCollection();
        var defaultLocale = locales.FirstOrDefault(x => x.Default)?.Code;
        var otherLocales = locales.Where(x => !x.Default).Select(x => x.Code).ToList();

        return new GetLocalesResponse
        {
            DefaultLocale = defaultLocale,
            OtherLocales = otherLocales
        };
    }

    [Action("Search missing locales for a field", Description = "Search for a list of missing locales for a field.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForField(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            entryIdentifier.Environment);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var availableLocales = (await client.GetLocalesCollection()).Select(l => l.Code);
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

        return new ListLocalesResponse { Locales = missingLocales };
    }

    [Action("Search missing locales for entry",
        Description = "Search for a list of missing locales for specified entry.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForEntry(
        [ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            entryIdentifier.Environment);
        var availableLocales = (await client.GetLocalesCollection()).Select(l => l.Code).ToArray();
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var contentModel = await client.GetContentType(entry.SystemProperties.ContentType.SystemProperties.Id);
        var contentModelLocalizableFields = contentModel.Fields.Where(f => f.Localized);
        var entryFields = (JObject)entry.Fields;
        var missingLocales = new HashSet<string>();

        foreach (var field in contentModelLocalizableFields)
        {
            if (!entryFields.TryGetValue(field.Id, out var entryField))
                continue;

            foreach (var locale in availableLocales)
            {
                if (!((JObject)entryField).TryGetValue(locale, out _))
                    missingLocales.Add(locale);
            }
        }

        return new ListLocalesResponse { Locales = missingLocales };
    }

    [Action("Get entry's localizable fields as HTML file", Description =
        "Get all localizable fields of specified entry " +
        "as HTML file.")]
    public async Task<FileResponse> GetEntryLocalizableFieldsAsHtmlFile(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] GetEntryAsHtmlRequest input)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var spaceId = Creds.Get("spaceId").Value;

        var entriesContent = await GetLinkedEntriesContent(
            entryIdentifier.EntryId, 
            entryIdentifier.Locale, 
            client, 
            new(), 
            input.GetReferenceContent ?? false, 
            input.GetNonLocalizationReferenceContent ?? false, 
            input.GetHyperlinkContent ?? false, 
            input.GetEmbeddedInlineContent ?? false, 
            input.GetEmbeddedBlockContent ?? false,
            input.IgnoredFieldIds ?? new List<string>());

        var resultHtml = EntryToHtmlConverter.ToHtml(entriesContent, entryIdentifier.Locale, spaceId);

        var file = await fileManagementClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(resultHtml)),
            MediaTypeNames.Text.Html, $"{entryIdentifier.EntryId}_{entryIdentifier.Locale}.html");
        return new()
        {
            File = file
        };
    }

    [Action("Set entry's localizable fields from HTML file", Description =
        "Set all localizable fields of specified entry " +
        "from HTML file.")]
    public async Task SetEntryLocalizableFieldsFromHtmlFile(
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] FileRequest input)
    {
        var client = new ContentfulClient(Creds, localeIdentifier.Environment);

        var file = await fileManagementClient.DownloadAsync(input.File);
        var html = Encoding.UTF8.GetString(await file.GetByteData());

        var entriesToUpdate = EntryToJsonConverter.GetEntriesInfo(html);

        foreach (var entryToUpdate in entriesToUpdate)
        {
            var entry = await client.GetEntry(entryToUpdate.EntryId);

            try
            {
                var oldEntryFields = (entry.Fields as JToken)!.DeepClone();
                EntryToJsonConverter.ToJson(entry, entryToUpdate.HtmlNode, localeIdentifier.Locale);

                if (JToken.DeepEquals(oldEntryFields.Escape(), (entry.Fields as JObject)!.Escape()))
                    continue;

                await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
            }
            catch (Exception ex)
            {
                throw new(
                    $"Converting entry to HTML failed. Locale: {localeIdentifier.Locale}; Entry: {JsonConvert.SerializeObject(entry)}; HTML: {entryToUpdate.HtmlNode.OuterHtml};Exception: {ex}");
            }
        }
    }

    #endregion

    #region Utils

    private async Task<List<EntryContentDto>> GetLinkedEntriesContent(string entryId, string locale,
        ContentfulClient client,
        List<EntryContentDto> resultList, bool references, bool ignoreReferenceLocalization, bool hyperlinks, bool inline, bool blocks, IEnumerable<string> ignoredFieldIds)
    {
        if (resultList.Any(x => x.Id == entryId))
            return resultList;

        var entryContent = await GetEntryContent(entryId, client, ignoredFieldIds, ignoreReferenceLocalization);
        var linkedIds = GetLinkedEntryIds(entryContent, locale, references, hyperlinks, inline, blocks).Distinct().ToList();

        resultList.Add(entryContent);
        foreach (var linkedEntryId in linkedIds)
            await GetLinkedEntriesContent(linkedEntryId, locale, client, resultList, references, ignoreReferenceLocalization, hyperlinks, inline, blocks, ignoredFieldIds);

        return resultList;
    }

    private IEnumerable<string> GetLinkedEntryIds(EntryContentDto entryContent, string locale, bool references, bool hyperlinks, bool inline, bool blocks)
    {
        try
        {
            var result = new List<string>();
            var contentTypeFields = entryContent.ContentTypeFields;

            if (references)
            {
                var linkFieldIds = contentTypeFields
                    .Where(f => f.LinkType == "Entry")
                    .Select(x => entryContent.EntryFields[x.Id]?[locale]?["sys"]?["id"]?.ToString()!)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                var linkArrayFieldIds = contentTypeFields
                    .Where(x => x.Items?.LinkType == "Entry")
                    .SelectMany(x =>
                        entryContent.EntryFields[x.Id]?[locale]?.Select(x => x["sys"]?["id"]?.ToString()!) ??
                        Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                result = result.Concat(linkFieldIds).Concat(linkArrayFieldIds).ToList();
            }

            if (hyperlinks)
            {
                var richTextFieldEntryHyperlinkIds = contentTypeFields
                    .Where(x => x.Type is "RichText")
                    .SelectMany(x => (entryContent.EntryFields[x.Id]?[locale] as JObject)?.Descendants().Where(y =>
                                         y is JProperty { Name: "nodeType" } jProperty &&
                                         jProperty.Value.ToString() == "entry-hyperlink") ??
                                     Enumerable.Empty<JToken>()).Where(x => x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
                                     .Select(x => x.Parent?["data"]?["target"]?["sys"]?["id"]?.Value<string>()).ToList();

                result = result.Concat(richTextFieldEntryHyperlinkIds).ToList();
            }

            if (inline)
            {
                var richTextFieldInlineEntryIds = contentTypeFields
                    .Where(x => x.Type is "RichText")
                    .SelectMany(x => (entryContent.EntryFields[x.Id]?[locale] as JObject)?.Descendants().Where(y =>
                                         y is JProperty { Name: "nodeType" } jProperty &&
                                         jProperty.Value.ToString() == "embedded-entry-inline") ??
                                     Enumerable.Empty<JToken>()).Where(x => x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
                                     .Select(x => x.Parent?["data"]?["target"]?["sys"]?["id"]?.Value<string>()).ToList();

                result = result.Concat(richTextFieldInlineEntryIds).ToList();
            }

            if (blocks)
            {
                var richTextFieldBlockEntryIds = contentTypeFields
                    .Where(x => x.Type is "RichText")
                    .SelectMany(x => (entryContent.EntryFields[x.Id]?[locale] as JObject)?.Descendants().Where(y =>
                                         y is JProperty { Name: "nodeType" } jProperty &&
                                         jProperty.Value.ToString() == "embedded-entry-block") ??
                                     Enumerable.Empty<JToken>()).Where(x => x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
                                     .Select(x => x.Parent?["data"]?["target"]?["sys"]?["id"]?.Value<string>()).ToList();

                result = result.Concat(richTextFieldBlockEntryIds).ToList();
            }

            return result.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error parsing Contentful model for locale {locale} | {JsonConvert.SerializeObject(entryContent)}");
        }
    }

    private async Task<EntryContentDto> GetEntryContent(string entryId, ContentfulClient client, IEnumerable<string> ignoredFieldIds, bool ignoreLocalizationForLinks = false)
    {
        var entry = await client.GetEntry(entryId);

        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);

        if (ignoreLocalizationForLinks)
        {
            return new(entryId, entry.Fields, contentType.Fields.Where(x => x.Localized || x.Type == "Link" || (x.Type == "Array" && x.Items?.Type == "Link")).Where(x => !ignoredFieldIds.Contains(x.Id)).ToArray());
        }

        return new(entryId, entry.Fields, contentType.Fields.Where(x => x.Localized).Where(x => !ignoredFieldIds.Contains(x.Id)).ToArray());
    }

    private (string? entryId, string? fieldId, string? locale) ExtractIdsFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var entryId = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-entry-id']")
            ?.GetAttributeValue("content", null);
        var fieldId = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-field-id']")
            ?.GetAttributeValue("content", null);
        var locale = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-locale']")
            ?.GetAttributeValue("content", null);

        return (entryId, fieldId, locale);
    }

    #endregion
}