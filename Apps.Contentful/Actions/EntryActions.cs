﻿using System.Net.Mime;
using System.Text;
using Apps.Contentful.Api;
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
using System.Web;
using Apps.Contentful.Models.Exceptions;
using Apps.Contentful.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Xliff.Utils.Serializers.Xliff2;
using Blackbird.Xliff.Utils.Serializers.Html;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    #region Text/Rich text fields

    [Action("Get first not empty entry's text/rich text field", Description =
        "Get the first not empty text content of the field of the specified entry. " +
        "Field can be short text, long text or rich text. In all cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetFirstNotEmptyTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] GetFirstNotEmptyTextFieldContentRequest request)
    {
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

    [Action("Get entry's text/rich text field", Description =
        "Get the text content of the field of the specified entry. " +
        "Field can be short text, long text or rich text. In all " +
        "cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetTextFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
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

        return new GetTextContentResponse { TextContent = textContent };
    }

    [Action("Get text/rich text field as HTML file",
        Description =
            "Get the text content of the field of the specified entry as HTML file. Field can be short text, long text or rich text. In all cases HTML file is returned.")]
    public async Task<FileResponse> GetTextFieldContentAsHtmlFile(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][entryIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
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
        {
            htmlContent = $"<p>{field}</p>";
        }
        else
        {
            throw new PluginMisconfigurationException(
                "The specified field must be of the short text, long text or rich text type.");
        }

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
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
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

    [Action("Get IDs from HTML file", Description = "Extract entry and field IDs from HTML file.")]
    public async Task<GetIdsFromHtmlResponse> GetIdsFromHtmlFile([ActionParameter] GetIdsFromFileRequest input)
    {
        var file = await fileManagementClient.DownloadAsync(input.File);
        var html = Encoding.UTF8.GetString(await file.GetByteData());
        var (entryId, fieldId, locale) = ExtractIdsFromHtml(html);

        if (string.IsNullOrEmpty(entryId))
        {
            throw new PluginApplicationException("Couldn't find the 'blackbird-entry-id' value in the HTML file. " +
                                                 "Please make sure the file is not empty and was generated by our 'Contentful' app. " +
                                                 "Otherwise, please contact the support team.");
        }

        var linkedIds = GetLinkedEntryIdsFromFile(html);
        return new GetIdsFromHtmlResponse
        {
            EntryId = entryId,
            FieldId = fieldId ?? string.Empty,
            Locale = locale ?? string.Empty,
            LinkedEntryIds = linkedIds.EntryIds,
            LinkedAssetIds = linkedIds.AssetIds
        };
    }

    [Action("Set text/rich text field from HTML file", Description =
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

        var entryId = entryIdentifier.EntryId ?? extractedEntryId
            ?? throw new PluginMisconfigurationException("Entry ID is empty. Please provide a valid ID");
        var fieldId = fieldIdentifier.FieldId ?? extractedFieldId
            ?? throw new PluginMisconfigurationException("Field ID is empty. Please provide a valid ID");

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () => await client.GetEntry(entryId));
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
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
            throw new PluginMisconfigurationException(
                "The specified field must be of the short text, long text or rich text type.");
        }

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    #endregion

    #region Number fields

    [Action("Get entry's number field", Description = "Get entry's number field value by field ID.")]
    public async Task<GetNumberContentResponse> GetNumberFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
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
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = number;
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    #endregion

    #region Boolean fields

    [Action("Get entry's boolean field", Description = "Get entry's boolean field by field ID.")]
    public async Task<GetBoolContentResponse> GetBoolFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
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
        [ActionParameter] [Display("Boolean")] bool boolean)
    {
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][entryIdentifier.Locale] = boolean;
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    #endregion

    #region Media fields

    [Action("Get entry's media content", Description = "Get entry's media content by field ID.")]
    public async Task<EntryMediaContentResponse> GetMediaFieldContent(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
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
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(assetEntryIdentifier.EntryId));
        var fields = (JObject)entry.Fields;

        fields[fieldIdentifier.FieldId] ??= new JObject();
        fields[fieldIdentifier.FieldId][assetEntryIdentifier.Locale] =
            JObject.Parse(JsonConvert.SerializeObject(payload));
        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version));
    }

    #endregion

    #region Entries

    [Action("Search entries", Description = "Search for all entries. Optionally filter by content model and tags.")]
    public async Task<ListEntriesResponse> ListEntries([ActionParameter] ListEntriesRequest request)
    {
        var client = new ContentfulClient(Creds, request.Environment);

        var queryString = HttpUtility.ParseQueryString(string.Empty);

        if (request.ContentModelId != null)
        {
            queryString.Add("content_type", request.ContentModelId);
        }

        if (request.UpdatedFrom.HasValue)
        {
            queryString.Add("sys.updatedAt[gte]", request.UpdatedFrom.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
        }

        if (request.UpdatedTo.HasValue)
        {
            queryString.Add("sys.updatedAt[lte]", request.UpdatedTo.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            queryString.Add("query", request.SearchTerm);
        }
            


        IEnumerable<Entry<object>> entries =
            await client.Paginate<Entry<object>>(
                async (query) => await client.GetEntriesCollection<Entry<object>>(query), "?" + queryString);
        
        if (request.Published.HasValue && request.Published.Value)
        {            
            entries = entries.Where(e => e.SystemProperties.PublishedVersion != null && 
                                         e.SystemProperties.Version == e.SystemProperties.PublishedVersion + 1);
        }
        
        if (request.Changed.HasValue && request.Changed.Value)
        {
            entries = entries.Where(e => e.SystemProperties.PublishedVersion != null && 
                                         e.SystemProperties.Version >= e.SystemProperties.PublishedVersion + 2);
        }        
        
        if (request.Draft.HasValue && request.Draft.Value)
        {
            entries = entries.Where(e => e.SystemProperties.PublishedVersion.HasValue == false);
        }

        if (request.Tags is not null && request.Tags.Any())
        {
            entries = entries.Where(e => e.Metadata.Tags.Any(t => request.Tags.Contains(t.Sys.Id)));
        }

        if (request.ExcludeTags is not null && request.ExcludeTags.Any())
        {
            entries = entries.Where(e => e.Metadata.Tags.All(t => !request.ExcludeTags.Contains(t.Sys.Id)));
        }

        var entriesResponse = entries.Select(e => new EntryEntity(e)).ToArray();
        return new ListEntriesResponse(entriesResponse, entriesResponse.Length);
    }

    [Action("Get entry", Description = "Get details of a specific entry")]
    public async Task<EntryWithTitleEntity> GetEntry([ActionParameter] EntryIdentifier input, 
        [ActionParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
    {
        if (string.IsNullOrEmpty(input.EntryId))
        {
            throw new PluginMisconfigurationException("Entry ID is null or empty. Please add a valid entry ID");
        }
        
        var client = new ContentfulClient(Creds, input.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () => await client.GetEntry(input.EntryId));
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType =
            await client.ExecuteWithErrorHandling(async () => await client.GetContentType(contentTypeId));
        entry.SystemProperties.ContentType = contentType;

        return new(entry, localeOptionalIdentifier.Locale);
    }

    [Action("Find entry by field value", Description = "Search for first matching entry.")]
    public async Task<EntryEntity> FindEntry([ActionParameter] FindEntryRequest input)
    {
        var client = new ContentfulClient(Creds, input.Environment);

        var queryString = HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("content_type", input.ContentModelId);

        queryString.Add($"fields.{input.Field}", input.Value);

        
        IEnumerable<Entry<object>> entries =
            await client.Paginate<Entry<object>>(
                async (query) => await client.GetEntriesCollection<Entry<object>>(query), "?" + queryString);

        var entriesResponse = entries.Select(e => new EntryEntity(e)).ToArray();

        return entriesResponse.FirstOrDefault();
    }

    [Action("Add new entry", Description = "Add new entry with specified content model.")]
    public async Task<EntryIdentifier> AddNewEntry([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds, contentModelIdentifier.Environment);
        var result = await client.ExecuteWithErrorHandling(async () =>
            await client.CreateEntry(new Entry<dynamic>(), contentModelIdentifier.ContentModelId));
        return new EntryIdentifier
        {
            EntryId = result.SystemProperties.Id
        };
    }

    [Action("Delete entry", Description = "Delete specified entry.")]
    public async Task DeleteEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        ContentfulClientExtensions.ThrowIfNullOrEmpty(entryIdentifier.EntryId, nameof(entryIdentifier.EntryId));
        
        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        await client.ExecuteWithErrorHandling(async () =>
            await client.DeleteEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version));
    }

    [Action("Publish entry", Description = "Publish specified entry.")]
    public async Task PublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        ContentfulClientExtensions.ThrowIfNullOrEmpty(entryIdentifier.EntryId, nameof(entryIdentifier.EntryId));

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        await client.ExecuteWithErrorHandling(async () =>
            await client.PublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version));
    }

    [Action("Unpublish entry", Description = "Unpublish specified entry.")]
    public async Task UnpublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        ContentfulClientExtensions.ThrowIfNullOrEmpty(entryIdentifier.EntryId, nameof(entryIdentifier.EntryId));

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId));
        await client.ExecuteWithErrorHandling(async () =>
            await client.UnpublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version));
    }

    [Action("Get locales", Description = "Get the default locale and all other locales in convenient codes form.")]
    public async Task<GetLocalesResponse> GetLocales([ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var locales = await client.ExecuteWithErrorHandling(async () => 
            await client.GetLocalesCollection());
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
        var entry = await client.ExecuteWithErrorHandling(async () => await client.GetEntry(entryIdentifier.EntryId));

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
        var entry = await client.ExecuteWithErrorHandling(async () => 
            await client.GetEntry(entryIdentifier.EntryId));
        var contentModel = await client.ExecuteWithErrorHandling(async () => 
            await client.GetContentType(entry.SystemProperties.ContentType.SystemProperties.Id));
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

    [Action("Get entry as HTML file", Description =
        "Get all localizable fields of specified entry " +
        "as an HTML file.")]
    public async Task<FileResponse> GetEntryLocalizableFieldsAsHtmlFile(
        [ActionParameter] EntryLocaleIdentifier entryIdentifier,
        [ActionParameter] GetEntryAsHtmlRequest input)
    {
        if (string.IsNullOrEmpty(entryIdentifier.EntryId))
        {
            throw new PluginMisconfigurationException("Entry ID is null or empty. Please add a valid entry ID");
        }
        if (entryIdentifier.EntryId.Contains("?")) 
        {
            entryIdentifier.EntryId = entryIdentifier.EntryId.Remove(entryIdentifier.EntryId.IndexOf('?'));
        }

        var client = new ContentfulClient(Creds, entryIdentifier.Environment);
        var spaceId = Creds.Get("spaceId").Value;

        var locales = await client.ExecuteWithErrorHandling(async () => await client.GetLocalesCollection());
        if (locales.All(x => x.Code != entryIdentifier.Locale))
        {
            var allLocales = string.Join(", ", locales.Select(x => x.Code));
            throw new PluginMisconfigurationException($"Locale {entryIdentifier.Locale} not found. Available locales: {allLocales}");
        }

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
            input.IgnoredFieldIds ?? new List<string>(),
            input.IgnoredContentTypeIds?.ToList() ?? new List<string>(),
            input.ExcludeTags?.ToList(),
            entryIdentifier.EntryId,
            input.MaxDepth);

        var htmlConverter = new EntryToHtmlConverter(InvocationContext, entryIdentifier.Environment);
        var resultHtml = htmlConverter.ToHtml(entriesContent, entryIdentifier.Locale, spaceId);

        var originalEntry = await GetEntry(new() { EntryId = entryIdentifier.EntryId, Environment = entryIdentifier.Environment}, 
            new LocaleOptionalIdentifier { Locale = entryIdentifier.Locale });
        
        var fileNameFirstPart = string.IsNullOrEmpty(originalEntry.Title) ? entryIdentifier.EntryId : originalEntry.Title;
        var file = await fileManagementClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(resultHtml)),
            MediaTypeNames.Text.Html, $"{fileNameFirstPart}_{entryIdentifier.Locale}.html");
        return new()
        {
            File = file
        };
    }

    [Action("Update entry from HTML file", Description =
        "Update all localizable fields of specified entry " +
        "from an HTML file.")]
    public async Task SetEntryLocalizableFieldsFromHtmlFile(
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] FileRequest input,
        [ActionParameter] UpdateEntryFromHtmlRequest updateEntryFromHtmlRequest)
    {
        var client = new ContentfulClient(Creds, localeIdentifier.Environment);

        var locales = await client.ExecuteWithErrorHandling(async () =>await client.GetLocalesCollection());
        if (locales.All(x => x.Code != localeIdentifier.Locale))
        {
            var allLocales = string.Join(", ", locales.Select(x => x.Code));
            throw new PluginMisconfigurationException(
                $"Locale {localeIdentifier.Locale} not found. Please specify a valid locale. " +
                $"Available locales: {allLocales}");
        }

        var file = await fileManagementClient.DownloadAsync(input.File);
        var html = Encoding.UTF8.GetString(await file.GetByteData());

        // XLIFF 2 upgrade
        if (Xliff2Serializer.IsXliff2(html))
        {
            html = HtmlSerializer.Serialize(Xliff2Serializer.Deserialize(html)).FirstOrDefault();
            if (html == null) throw new PluginMisconfigurationException("XLIFF did not contain any files");
        }

        var entriesToUpdate = EntryToJsonConverter.GetEntriesInfo(html);

        foreach (var entryToUpdate in entriesToUpdate)
        {
            var entry = await client.ExecuteWithErrorHandling(() => client.GetEntry(entryToUpdate.EntryId));
            var contentType = await client.ExecuteWithErrorHandling(() =>
                client.GetContentType(entry.SystemProperties.ContentType.SystemProperties.Id));

            try
            {
                var oldEntryFields = (entry.Fields as JToken)!.DeepClone();
                EntryToJsonConverter.ToJson(entry, entryToUpdate.HtmlNode, localeIdentifier.Locale, contentType, updateEntryFromHtmlRequest.DontUpdateReferenceFields ?? false);

                if (JToken.DeepEquals(oldEntryFields.Escape(), (entry.Fields as JObject)!.Escape()))
                {
                    continue;
                }
                
                await client.ExecuteWithErrorHandling(async () =>
                {
                    var entryVersion = await client.GetEntry(entryToUpdate.EntryId);
                    return await client.CreateOrUpdateEntry(entry, version: entryVersion.SystemProperties.Version);
                });
            }
            catch (FieldConversionException ex)
            {
                throw new PluginMisconfigurationException(
                    $"Error updating entry '{entry.SystemProperties.Id}': {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("archived"))
                {
                    continue;
                }

                if (ex.Message.Contains("Version mismatch error") 
                    || ex.Message.Contains("The resource could not be found")
                    || ex.Message.Contains("Internal server") 
                    || ex.Message.Contains("Validation error"))
                {
                    throw new PluginApplicationException($"Converting entry to JSON failed. " +
                                                         $"Entry ID: {entry.SystemProperties.Id}; " +
                                                         $"Error: {ex.Message}");
                }

                throw new(
                    $"Converting entry to JSON failed. Entry ID: {entry.SystemProperties.Id};  Exception: {ex}; Locale: {localeIdentifier.Locale}; Entry: {JsonConvert.SerializeObject(entry)}; HTML: {entryToUpdate.HtmlNode.OuterHtml};");
            }
        }
    }

    #endregion

    #region Utils

    private async Task<List<EntryContentDto>> GetLinkedEntriesContent(string entryId, string locale,
        ContentfulClient client,
        List<EntryContentDto> resultList, bool getReferenceContent, bool ignoreReferenceLocalization, bool hyperlinks,
        bool inline, bool blocks, IEnumerable<string> ignoredFieldIds, List<string> ignoredContentTypeIds,
        List<string>? excludeTags, string rootEntryId, int? maxDepth = null, int currentDepth = 0)
    {
        if (maxDepth.HasValue && currentDepth >= maxDepth.Value)
            return resultList;

        if (resultList.Any(x => x.Id == entryId))
            return resultList;

        var entryContent = await client.ExecuteWithErrorHandling(() =>
            GetEntryContent(entryId, client, ignoredFieldIds, ignoredContentTypeIds, excludeTags, rootEntryId, 
                ignoreReferenceLocalization));

        if (entryContent != null)
        {
            var linkedIds = GetLinkedEntryIds(entryContent, locale, getReferenceContent, hyperlinks, inline, blocks).Distinct()
                .ToList();

            resultList.Add(entryContent);
            foreach (var linkedEntryId in linkedIds)
                await GetLinkedEntriesContent(linkedEntryId, locale, client, resultList, getReferenceContent,
                    ignoreReferenceLocalization, hyperlinks, inline, blocks, ignoredFieldIds, ignoredContentTypeIds,
                    excludeTags, rootEntryId, maxDepth, currentDepth + 1);

            return resultList;
        }

        return resultList;
    }

    private LinkedIdsEntity GetLinkedEntryIdsFromFile(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var entryIds = new List<string>();
        var assetIds = new List<string>();

        var entryIdNodes = doc.DocumentNode.SelectNodes("//div[@data-contentful-link-id]");
        if (entryIdNodes != null)
        {
            foreach (var node in entryIdNodes)
            {
                var id = node.GetAttributeValue("data-contentful-link-id", string.Empty);
                var type = node.GetAttributeValue("data-contentful-link-type", string.Empty);

                if (type == "Asset")
                {
                    assetIds.Add(id);
                }
                else if (!string.IsNullOrEmpty(id))
                {
                    entryIds.Add(id);
                }
            }
        }

        var linkIdNodes = doc.DocumentNode.SelectNodes("//div[@data-contentful-link-ids]");
        if (linkIdNodes != null)
        {
            foreach (var node in linkIdNodes)
            {
                var ids = node.GetAttributeValue("data-contentful-link-ids", string.Empty);
                if (!string.IsNullOrEmpty(ids))
                {
                    foreach (var id in ids.Split(','))
                    {
                        if (assetIds.Contains(id.Trim()))
                            continue;

                        entryIds.Add(id.Trim());
                    }
                }
            }
        }

        // Handle embedded-entry-inline links
        var inlineLinkNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@id, 'embedded-entry-inline_')]");
        if (inlineLinkNodes != null)
        {
            const string prefix = "embedded-entry-inline_";
            foreach (var node in inlineLinkNodes)
            {
                var idAttr = node.GetAttributeValue("id", string.Empty);
                if (!string.IsNullOrEmpty(idAttr) && idAttr.StartsWith(prefix))
                {
                    var extractedId = idAttr.Substring(prefix.Length);
                    if (!string.IsNullOrEmpty(extractedId))
                    {
                        entryIds.Add(extractedId);
                    }
                }
            }
        }

        // Handle entry-hyperlink elements
        var hyperlinkNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@id, 'entry-hyperlink_')]");
        if (hyperlinkNodes != null)
        {
            const string prefix = "entry-hyperlink_";
            foreach (var node in hyperlinkNodes)
            {
                var idAttr = node.GetAttributeValue("id", string.Empty);
                if (!string.IsNullOrEmpty(idAttr) && idAttr.StartsWith(prefix))
                {
                    var extractedId = idAttr.Substring(prefix.Length);
                    if (!string.IsNullOrEmpty(extractedId))
                    {
                        entryIds.Add(extractedId);
                    }
                }
            }
        }

        // Handle embedded-entry-block elements
        var blockLinkNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@id, 'embedded-entry-block_')]");
        if (blockLinkNodes != null)
        {
            const string prefix = "embedded-entry-block_";
            foreach (var node in blockLinkNodes)
            {
                var idAttr = node.GetAttributeValue("id", string.Empty);
                if (!string.IsNullOrEmpty(idAttr) && idAttr.StartsWith(prefix))
                {
                    var extractedId = idAttr.Substring(prefix.Length);
                    if (!string.IsNullOrEmpty(extractedId))
                    {
                        entryIds.Add(extractedId);
                    }
                }
            }
        }

        return new LinkedIdsEntity(entryIds.Distinct().ToList(), assetIds.Distinct().ToList());
    }

    private IEnumerable<string> GetLinkedEntryIds(EntryContentDto entryContent, string locale, bool getReferenceContent,
        bool hyperlinks, bool inline, bool blocks)
    {
        try
        {
            var result = new List<string>();
            var contentTypeFields = entryContent.ContentTypeFields;

            if (getReferenceContent)
            {
                var linkFieldIds = contentTypeFields
                    .Where(f => f.LinkType is "Entry")
                    .Select(f => 
                    {
                        var fieldToken = entryContent.EntryFields[f.Id];
                        if (fieldToken == null) 
                            return null;

                        var localeToken = fieldToken[locale];
                        if (localeToken == null || localeToken.Type != JTokenType.Object)
                            return null;

                        var sys = localeToken["sys"];
                        if (sys == null) 
                            return null;

                        return sys["id"]?.ToString();
                    })
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
                                     Enumerable.Empty<JToken>()).Where(x =>
                        x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
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
                                     Enumerable.Empty<JToken>()).Where(x =>
                        x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
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
                                     Enumerable.Empty<JToken>()).Where(x =>
                        x.Parent?["data"]?["target"]?["sys"]?["linkType"]?.Value<string>() == "Entry")
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

    private async Task<EntryContentDto?> GetEntryContent(string entryId,
        ContentfulClient client,
        IEnumerable<string> ignoredFieldIds,
        IEnumerable<string> ignoredContentTypeIds,
        IEnumerable<string>? excludeTags,
        string rootEntryId,
        bool ignoreLocalizationForLinks = false,
        bool ignoreLocalizationFields = false)
    {
        var entry = await client.GetEntryWithErrorHandling(entryId);

        if (rootEntryId != entry.SystemProperties.Id &&
            ignoredContentTypeIds.Contains(entry.SystemProperties.ContentType.SystemProperties.Id))
        {
            return null;
        }

        if (excludeTags is not null && excludeTags.Any(excludedTag => 
                entry.Metadata.Tags.Any(entryTag => entryTag.Sys.Id == excludedTag)))
        {
            if (rootEntryId == entryId)
            {
                var excludedTags = string.Join(',', excludeTags);
                throw new PluginMisconfigurationException($"The root entry ({rootEntryId}) has one or more tags that are included in the excluded tags you provided: {excludedTags}. " + 
                                                          $"Most likely, you don't want to translate this entry. If you do, please modify either the excluded tags or the entry tags.");
            }
            
            return null;
        }

        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.ExecuteWithErrorHandling(async () =>await client.GetContentType(contentTypeId));

        if (ignoreLocalizationForLinks)
        {
            return new(entryId, entry.Fields,
                contentType.Fields
                    .Where(x => x.Localized || x.Type == "Link" || (x.Type == "Array" && x.Items?.Type == "Link"))
                    .Where(x => !ignoredFieldIds.Contains(x.Id)).ToArray());
        }

        if (!ignoreLocalizationFields)
        {
            return new(entryId, entry.Fields,
                contentType.Fields.Where(x => x.Localized).Where(x => !ignoredFieldIds.Contains(x.Id)).ToArray());
        }

        return new(entryId, entry.Fields, contentType.Fields.Where(x => !ignoredFieldIds.Contains(x.Id)).ToArray());
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