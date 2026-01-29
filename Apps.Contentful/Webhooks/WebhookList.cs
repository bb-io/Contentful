using System.Net;
using Apps.Contentful.Actions;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [WebhookParameter(isSubscriptionDepends: true)] OptionalEntryIdentifier optionalEntryIdentifier,
        [WebhookParameter] OptionalTagListIdentifier tags,
        [WebhookParameter] OptionalMultipleContentTypeIdentifier types,
        [WebhookParameter] OptionalFilterUsersIdentifier users)
        => HandleEntryWebhookResponse(webhookRequest, tags, types, optionalEntryIdentifier, users);

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
        OptionalFilterUsersIdentifier users
        )
    {
        var payload = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString()!);

        if (payload is null)
            throw new InvalidCastException(nameof(webhookRequest.Body));

        var entryActions = new EntryActions(invocationContext, null!);
        var entry = await entryActions.GetEntry(new EntryIdentifier { EntryId = payload.Sys.Id, Environment = tagsInput.Environment },
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
            return new()
            {
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
            return new()
            {
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

        var entryActions = new EntryActions(invocationContext, null!);
        var entry = await entryActions.GetEntry(new EntryIdentifier { EntryId = payload.Sys.Id, Environment = tagsInput.Environment },
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
            return new()
            {
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
            return new()
            {
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
}