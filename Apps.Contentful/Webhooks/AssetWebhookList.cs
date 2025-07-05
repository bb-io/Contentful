using System.Net;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers.AssetHandlers;
using Apps.Contentful.Webhooks.Models.Payload;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using WebhookRequest = Blackbird.Applications.Sdk.Common.Webhooks.WebhookRequest;

namespace Apps.Contentful.Webhooks;

[WebhookList("Assets")]
public class AssetWebhookList(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
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
}