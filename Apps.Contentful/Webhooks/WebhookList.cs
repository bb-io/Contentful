using Apps.Contentful.Dtos;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers;
using Apps.Contentful.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Apps.Contentful.Webhooks
{
    [WebhookList]
    public class WebhookList 
    {
        #region EntryWebhooks

        [Webhook("On entry created", typeof(EntryCreatedHandler), Description = "On entry created")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryCreation(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if(data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry saved", typeof(EntrySavedHandler), Description = "On entry saved")]
        public async Task<WebhookResponse<FieldsChangedResponse>> EntrySaved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            var changes = new FieldsChangedResponse()
            {
                EntryId = data.Sys.Id,
                Fields = new List<FieldObject>()
            };
            foreach (var propertyField in data.Fields.Properties())
            {
                foreach (var propertyLocale in (JObject)propertyField.Value)
                {
                    changes.Fields.Add(new FieldObject()
                    {
                        FieldId = propertyField.Name,
                        Locale = propertyLocale.Key,
                        FieldValue = propertyLocale.Value.ToString()
                    });
                }
            }
            return new WebhookResponse<FieldsChangedResponse>
            {
                HttpResponseMessage = null,
                Result = changes
            };
        }

        [Webhook("On entry auto saved", typeof(EntryAutosavedHandler), Description = "On entry auto saved")]
        public async Task<WebhookResponse<FieldsChangedResponse>> EntryAutoSaved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            var changes = new FieldsChangedResponse() 
            { 
                EntryId = data.Sys.Id,
                Fields = new List<FieldObject>()
            };
            foreach (var propertyField in data.Fields.Properties())
            {
                foreach (var propertyLocale in (JObject) propertyField.Value)
                {
                    changes.Fields.Add(new FieldObject()
                    {
                        FieldId = propertyField.Name,
                        Locale = propertyLocale.Key,
                        FieldValue = propertyLocale.Value.ToString()
                    });
                }
            }
            return new WebhookResponse<FieldsChangedResponse>
            {
                HttpResponseMessage = null,
                Result = changes
            };
        }

        [Webhook("On entry published", typeof(EntryPublishedHandler), Description = "On entry published")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryPublished(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry unpublished", typeof(EntryUnpublishedHandler), Description = "On entry unpublished")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryUnpublished(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry archieved", typeof(EntryArchievedHandler), Description = "On entry archieved")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryArchieved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry unarchieved", typeof(EntryUnarchievedHandler), Description = "On entry unarchieved")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryUnarchieved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry deleted", typeof(EntryDeletedHandler), Description = "On entry deleted")]
        public async Task<WebhookResponse<AddNewEntryResponse>> EntryDeleted(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<GenericEntryPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<AddNewEntryResponse>
            {
                HttpResponseMessage = null,
                Result = new AddNewEntryResponse() { EntryId = data.Sys.Id }
            };
        }
        #endregion
    }
}
