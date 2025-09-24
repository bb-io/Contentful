using System.Net;
using Apps.Contentful.Actions;
using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers.EntryHandlers;
using Apps.Contentful.Webhooks.Models.Inputs;
using Apps.Contentful.Webhooks.Models.Payload;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Blackbird.Applications.SDK.Blueprints;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using WebhookRequest = Blackbird.Applications.Sdk.Common.Webhooks.WebhookRequest;

namespace Apps.Contentful.Webhooks;

[WebhookList("Entries")]
public class WebhookList(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{

    [Webhook("On entry created", typeof(EntryCreatedHandler), Description = "On entry created")]
    public Task<WebhookResponse<EntryEntity>> EntryCreated(WebhookRequest webhookRequest, 
        [WebhookParameter] OptionalTagListIdentifier tags, 
        [WebhookParameter] OptionalMultipleContentTypeIdentifier types,
        [WebhookParameter] OptionalFilterUsersIdentifier users
        )
        => HandleEntryWebhookResponse(webhookRequest, tags, types, new OptionalEntryIdentifier(), users);

    // It looks like this doesn't trigger anymore after a contentful update removing the save button. Needs to be verified.
    //[Webhook("On entry saved", typeof(EntrySavedHandler), Description = "On entry saved")]
    //public Task<WebhookResponse<FieldsChangedResponse>> EntrySaved(WebhookRequest webhookRequest,
    //    [WebhookParameter] OptionalEntryIdentifier optionalEntryIdentifier,
    //    [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier, 
    //    [WebhookParameter] OptionalTagListIdentifier tags, 
    //    [WebhookParameter] OptionalMultipleContentTypeIdentifier types)
    //    => HandleFieldsChangedResponse(webhookRequest, localeOptionalIdentifier, tags, types, optionalEntryIdentifier);

    [Webhook("On entry auto saved", typeof(EntryAutoSavedHandler), Description = "On entry auto saved")]
    public Task<WebhookResponse<FieldsChangedResponse>> EntryAutoSaved(WebhookRequest webhookRequest,
        [WebhookParameter] OptionalEntryIdentifier optionalEntryIdentifier,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier, 
        [WebhookParameter] OptionalTagListIdentifier tags, 
        [WebhookParameter] OptionalMultipleContentTypeIdentifier types)
        => HandleFieldsChangedResponse(webhookRequest, localeOptionalIdentifier, tags, types, optionalEntryIdentifier);

    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdated)]
    [Webhook("On entry published", typeof(EntryPublishedHandler), Description = "On entry published")]
    public Task<WebhookResponse<EntryEntity>> EntryPublished(WebhookRequest webhookRequest,
        [WebhookParameter] OptionalEntryIdentifier optionalEntryIdentifier,
        [WebhookParameter] OptionalTagListIdentifier tags, 
        [WebhookParameter] OptionalMultipleContentTypeIdentifier types,
        [WebhookParameter] OptionalFilterUsersIdentifier users,
        [WebhookParameter] LocaleOptionalIdentifier locale)
        => HandleEntryWebhookResponse(webhookRequest, tags, types, optionalEntryIdentifier, users, locale);

    [Webhook("On entry unpublished", typeof(EntryUnpublishedHandler), Description = "On entry unpublished")]
    public Task<WebhookResponse<EntityWebhookResponse>> EntryUnpublished(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On entry archived", typeof(EntryArchivedHandler), Description = "On entry archived")]
    public Task<WebhookResponse<EntityWebhookResponse>> EntryArchived(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On entry unarchived", typeof(EntryUnarchivedHandler), Description = "On entry unarchived")]
    public Task<WebhookResponse<EntityWebhookResponse>> EntryUnarchived(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On entry deleted", typeof(EntryDeletedHandler), Description = "On entry deleted")]
    public Task<WebhookResponse<EntityWebhookResponse>> EntryDeleted(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);


    private static Task<WebhookResponse<EntityWebhookResponse>> HandleWebhookResponse(WebhookRequest webhookRequest)
    {
        var payload = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));

        return Task.FromResult<WebhookResponse<EntityWebhookResponse>>(new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new() { Id = payload.Sys.Id }
        });
    }
    
    private async Task<WebhookResponse<EntryEntity>> HandleEntryWebhookResponse(WebhookRequest webhookRequest, 
        OptionalTagListIdentifier tagsInput, 
        OptionalMultipleContentTypeIdentifier types, 
        OptionalEntryIdentifier optionalEntryIdentifier,
        OptionalFilterUsersIdentifier users,
        LocaleOptionalIdentifier? locale  = null
        )
    {
        var payload = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));

        var env = string.IsNullOrWhiteSpace(tagsInput?.Environment) ? null : tagsInput!.Environment;


        var entryActions = new EntryActions(invocationContext, null!);
        var entry = await entryActions.GetEntry(new EntryIdentifier { EntryId = payload.Sys.Id, Environment = env }, 
            new LocaleOptionalIdentifier { Locale = "*" });

        if (types.ContentModels != null && !types.ContentModels.Contains(entry.ContentTypeId))
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (tagsInput.TagIds != null && !tagsInput.TagIds.All(x => entry.TagIds.Contains(x)))
        {
            return new() {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (tagsInput.AnyTagIds != null && !tagsInput.AnyTagIds.Any(x => entry.TagIds.Contains(x)))
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (tagsInput.ExcludeTags != null && tagsInput.ExcludeTags.Any(x => entry.TagIds.Contains(x)))
        {
            return new() {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (optionalEntryIdentifier.EntryId != null && entry.ContentId != optionalEntryIdentifier.EntryId)
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (users.UserIds != null && users.UserIds.Contains(entry.UpdatedBy))
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (!string.IsNullOrWhiteSpace(locale?.Locale))
        {
            var changedLocales = await GetChangedLocalesAsync(payload, env, entry);
            if (!changedLocales.Contains(locale.Locale!, StringComparer.OrdinalIgnoreCase))
            {
                return new()
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    Result = null,
                    ReceivedWebhookRequestType = WebhookRequestType.Preflight,
                };
            }
        }

        return new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = entry
        };
    }



    private async Task<WebhookResponse<FieldsChangedResponse>> HandleFieldsChangedResponse(
        WebhookRequest webhookRequest,
        LocaleOptionalIdentifier localeOptionalIdentifier,
        OptionalTagListIdentifier tagsInput,
        OptionalMultipleContentTypeIdentifier types,
        OptionalEntryIdentifier optionalEntryIdentifier
        )
    {
        var payload = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));

        var changes = new FieldsChangedResponse
        {
            EntryId = payload.Sys.Id,
            Fields = new List<FieldObject>()
        };

        foreach (var propertyField in payload.Fields.Properties())
        {
            foreach (var propertyLocale in (JObject)propertyField.Value)
            {
                if (!string.IsNullOrEmpty(localeOptionalIdentifier.Locale)) 
                    if (propertyLocale.Key != localeOptionalIdentifier.Locale)
                        continue;
                
                changes.Fields.Add(new FieldObject
                {
                    FieldId = propertyField.Name,
                    Locale = propertyLocale.Key,
                    FieldValue = propertyLocale.Value.ToString()
                });
            }
        }
        
        var env = string.IsNullOrWhiteSpace(tagsInput?.Environment) ? null : tagsInput!.Environment;
        var entryActions = new EntryActions(invocationContext, null!);
        var entry = await entryActions.GetEntry(new EntryIdentifier { EntryId = payload.Sys.Id, Environment = env }, 
            new LocaleOptionalIdentifier());
        
        if (types.ContentModels != null && !types.ContentModels.Contains(entry.ContentTypeId))
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }
        
        if (tagsInput.TagIds != null && !tagsInput.TagIds.All(x => entry.TagIds.Contains(x)))
        {
            return new() {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (tagsInput.AnyTagIds != null && !tagsInput.AnyTagIds.Any(x => entry.TagIds.Contains(x)))
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (tagsInput.ExcludeTags != null && tagsInput.ExcludeTags.Any(x => entry.TagIds.Contains(x)))
        {
            return new() {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        if (optionalEntryIdentifier.EntryId != null && entry.ContentId != optionalEntryIdentifier.EntryId)
        {
            return new()
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        return new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = changes
        };
    }

    private async Task<HashSet<string>> GetChangedLocalesAsync(
         GenericEntryPayload payload, string? environment, EntryEntity currentEntry)
    {
        var (hasAnySnapshot, previousFields) = await TryGetLatestSnapshotFieldsAsync(payload.Sys.Id, environment);
        if (!hasAnySnapshot)
            return ExtractLocalesFromEntry(currentEntry);

        var localizedFieldIds = await GetLocalizedFieldIdsAsync(currentEntry.ContentTypeId, environment);
        if (localizedFieldIds.Count == 0)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var currentFields = ToFieldsJObject(currentEntry);
        var changedLocales = DiffChangedLocalesLimited(previousFields, currentFields, localizedFieldIds);

        if (changedLocales.Count == 0)
            changedLocales = ExtractLocalesFromEntry(currentEntry);

        return changedLocales;
    }

    private async Task<(bool hasAnySnapshot, JObject previousFields)> TryGetLatestSnapshotFieldsAsync(
    string entryId, string? environment)
    {
        var client = new ContentfulRestClient(Creds,environment);

        var request = new ContentfulRestRequest($"entries/{entryId}/snapshots", Method.Get,Creds);
        var response = await client.ExecuteWithErrorHandling(request);

        var jo = JObject.Parse(response.Content!);
        var items = (JArray?)jo["items"] ?? new JArray();

        if (items.Count == 0)
            return (false, new JObject());

        var prev = items
            .OfType<JObject>()
            .OrderBy(i => (DateTime?)i["sys"]?["createdAt"])
            .Last();

        var fields = (JObject?)(prev["snapshot"]?["fields"]) ?? new JObject();
        return (true, fields);
    }

    private async Task<JObject> GetPreviousPublishedFieldsAsync(string entryId, string? environment, int currentPublishedVersion)
    {
        var rest = new ContentfulRestClient(Creds,environment);

        var req = new ContentfulRestRequest($"entries/{entryId}/snapshots", Method.Get, Creds);
        var resp = await rest.ExecuteWithErrorHandling(req);

        var jo = JObject.Parse(resp.Content!);
        var items = (JArray?)jo["items"] ?? new JArray();

        var prev = items
            .OfType<JObject>()
            .Select(i => new
            {
                obj = i,
                pv = (int?)i["snapshot"]?["sys"]?["publishedVersion"] ?? 0
            })
            .Where(x => x.pv < currentPublishedVersion)
            .OrderByDescending(x => x.pv)
            .FirstOrDefault()
            ?.obj;

        return (JObject?)(prev?["snapshot"]?["fields"]) ?? new JObject();
    }

    private async Task<HashSet<string>> GetLocalizedFieldIdsAsync(string contentTypeId, string? environment)
    {
        var rest = new ContentfulRestClient(Creds, environment);

        var request = new ContentfulRestRequest($"content_types/{contentTypeId}", Method.Get, Creds);
        var respons = await rest.ExecuteWithErrorHandling(request);

        var jo = JObject.Parse(respons.Content!);
        var fields = (JArray?)jo["fields"] ?? new JArray();

        return fields
            .OfType<JObject>()
            .Where(f => (f["localized"]?.Value<bool>() ?? false) && !string.IsNullOrWhiteSpace(f["id"]?.ToString()))
            .Select(f => f["id"]!.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static JObject ToFieldsJObject(EntryEntity entry)
    {
        var json = JsonConvert.SerializeObject(entry);
        var jo = JObject.Parse(json);
        return (JObject)(jo["fields"] ?? new JObject());
    }

    private static HashSet<string> ExtractLocalesFromEntry(EntryEntity entry)
    {
        var fields = ToFieldsJObject(entry);
        var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields.Properties())
        {
            if (field.Value is JObject perLocale)
            {
                foreach (var loc in perLocale.Properties().Select(p => p.Name))
                    locales.Add(loc);
            }
        }

        return locales;
    }

    private static HashSet<string> DiffChangedLocalesLimited(
        JObject prevFields, JObject currFields, HashSet<string> allowedFieldIds)
    {
        var changed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var fieldIds = prevFields.Properties().Select(p => p.Name)
            .Union(currFields.Properties().Select(p => p.Name))
            .Where(allowedFieldIds.Contains);

        foreach (var fieldId in fieldIds)
        {
            var prevPerLocale = prevFields[fieldId] as JObject ?? new JObject();
            var currPerLocale = currFields[fieldId] as JObject ?? new JObject();

            var locales = prevPerLocale.Properties().Select(p => p.Name)
                .Union(currPerLocale.Properties().Select(p => p.Name));

            foreach (var loc in locales)
            {
                var prevVal = Normalize(prevPerLocale[loc]);
                var currVal = Normalize(currPerLocale[loc]);

                if (!JToken.DeepEquals(prevVal, currVal))
                    changed.Add(loc);
            }
        }
        return changed;

        static JToken? Normalize(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            if (token is JValue v && v.Type == JTokenType.String)
                return new JValue(v.Value<string>()?.Trim());
            return token;
        }
    }
}