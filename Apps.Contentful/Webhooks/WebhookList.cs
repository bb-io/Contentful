using System.Net;
using Apps.Contentful.Actions;
using Apps.Contentful.Api;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers.AssetHandlers;
using Apps.Contentful.Webhooks.Handlers.EntryHandlers;
using Apps.Contentful.Webhooks.Models.Payload;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebhookRequest = Blackbird.Applications.Sdk.Common.Webhooks.WebhookRequest;

namespace Apps.Contentful.Webhooks;

[WebhookList]
public class WebhookList(InvocationContext invocationContext)
{
    #region EntryWebhooks

    [Webhook("On entry created", typeof(EntryCreatedHandler), Description = "On entry created")]
    public Task<WebhookResponse<EntryEntity>> EntryCreated(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleEntryWebhookResponse(webhookRequest, localeOptionalIdentifier);

    [Webhook("On entry saved", typeof(EntrySavedHandler), Description = "On entry saved")]
    public Task<WebhookResponse<FieldsChangedResponse>> EntrySaved(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleFieldsChangedResponse(webhookRequest, localeOptionalIdentifier);

    [Webhook("On entry auto saved", typeof(EntryAutoSavedHandler), Description = "On entry auto saved")]
    public Task<WebhookResponse<FieldsChangedResponse>> EntryAutoSaved(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleFieldsChangedResponse(webhookRequest, localeOptionalIdentifier);

    [Webhook("On entry published", typeof(EntryPublishedHandler), Description = "On entry published")]
    public Task<WebhookResponse<EntryEntity>> EntryPublished(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleEntryWebhookResponse(webhookRequest, localeOptionalIdentifier);

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

    #endregion

    #region AssetWebhooks

    [Webhook("On asset created", typeof(AssetCreatedHandler), Description = "On asset created")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetCreated(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On asset saved", typeof(AssetSavedHandler), Description = "On asset saved")]
    public Task<WebhookResponse<AssetChangedResponse>> AssetSaved(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleAssetChangedResponse(webhookRequest, localeOptionalIdentifier);

    [Webhook("On asset auto saved", typeof(AssetAutoSavedHandler), Description = "On asset auto saved")]
    public Task<WebhookResponse<AssetChangedResponse>> AssetAutoSaved(WebhookRequest webhookRequest,
        [WebhookParameter] LocaleOptionalIdentifier localeOptionalIdentifier)
        => HandleAssetChangedResponse(webhookRequest, localeOptionalIdentifier);

    [Webhook("On asset published", typeof(AssetPublishedHandler), Description = "On asset published")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetPublished(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On asset unpublished", typeof(AssetUnpublishedHandler), Description = "On asset unpublished")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetUnpublished(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On asset archived", typeof(AssetArchivedHandler), Description = "On asset archived")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetArchived(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On asset unarchived", typeof(AssetUnarchivedHandler), Description = "On asset unarchived")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetUnarchived(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    [Webhook("On asset deleted", typeof(AssetDeletedHandler), Description = "On asset deleted")]
    public Task<WebhookResponse<EntityWebhookResponse>> AssetDeleted(WebhookRequest webhookRequest)
        => HandleWebhookResponse(webhookRequest);

    #endregion

    #region Utils

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
    
    private async Task<WebhookResponse<EntryEntity>> HandleEntryWebhookResponse(
        WebhookRequest webhookRequest,
        LocaleOptionalIdentifier localeOptionalIdentifier)
    {
        var payload = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));
        
        var entryActions = new EntryActions(invocationContext, null!);
        var entry = await entryActions.GetEntry(new EntryIdentifier { EntryId = payload.Sys.Id });

        if (!string.IsNullOrEmpty(localeOptionalIdentifier.Locale))
        {
            if (entry.Locale == localeOptionalIdentifier.Locale)
            {
                return new()
                {
                    HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                    Result = entry
                };
            }

            return new WebhookResponse<EntryEntity>()
            {
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        }
        
        return new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = entry
        };
    }
    
    private static Task<WebhookResponse<FieldsChangedResponse>> HandleFieldsChangedResponse(
        WebhookRequest webhookRequest,
        LocaleOptionalIdentifier localeOptionalIdentifier)
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

        return Task.FromResult<WebhookResponse<FieldsChangedResponse>>(new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = changes
        });
    }

    private static Task<WebhookResponse<AssetChangedResponse>> HandleAssetChangedResponse(
        WebhookRequest webhookRequest,
        LocaleOptionalIdentifier localeOptionalIdentifier)
    {
        var payload = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));

        var changes = new AssetChangedResponse
        {
            AssetId = payload.Sys.Id,
            FilesInfo = new List<AssetFileInfo>()
        };

        foreach (var propertyLocale in payload.Fields.File.Properties())
        {
            if (!string.IsNullOrEmpty(localeOptionalIdentifier.Locale))
            {
                if (propertyLocale.Name != localeOptionalIdentifier.Locale)
                {
                    continue;
                }
            }
            
            var change = propertyLocale.Value.ToObject<AssetFileInfo>();
            change.Locale = propertyLocale.Name;
            changes.FilesInfo.Add(change);
        }

        return Task.FromResult<WebhookResponse<AssetChangedResponse>>(new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = changes
        });
    }

    #endregion
}