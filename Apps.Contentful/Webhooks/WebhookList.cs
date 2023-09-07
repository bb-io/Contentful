using System.Net;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers.AssetHandlers;
using Apps.Contentful.Webhooks.Handlers.EntryHandlers;
using Apps.Contentful.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Webhooks
{
    [WebhookList]
    public class WebhookList 
    {
        #region EntryWebhooks

        [Webhook("On entry created", typeof(EntryCreatedHandler), Description = "On entry created")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryCreated(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry saved", typeof(EntrySavedHandler), Description = "On entry saved")]
        public async Task<WebhookResponse<FieldsChangedResponse>> EntrySaved(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
           
            var changes = new FieldsChangedResponse
            {
                EntryId = data.Sys.Id,
                Fields = new List<FieldObject>()
            };
            
            foreach (var propertyField in data.Fields.Properties())
            {
                foreach (var propertyLocale in (JObject)propertyField.Value)
                {
                    changes.Fields.Add(new FieldObject
                    {
                        FieldId = propertyField.Name,
                        Locale = propertyLocale.Key,
                        FieldValue = propertyLocale.Value.ToString()
                    });
                }
            }
            
            return new WebhookResponse<FieldsChangedResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = changes
            };
        }

        [Webhook("On entry auto saved", typeof(EntryAutoSavedHandler), Description = "On entry auto saved")]
        public async Task<WebhookResponse<FieldsChangedResponse>> EntryAutoSaved(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);

            var changes = new FieldsChangedResponse
            { 
                EntryId = data.Sys.Id,
                Fields = new List<FieldObject>()
            };
            
            foreach (var propertyField in data.Fields.Properties())
            {
                foreach (var propertyLocale in (JObject) propertyField.Value)
                {
                    changes.Fields.Add(new FieldObject
                    {
                        FieldId = propertyField.Name,
                        Locale = propertyLocale.Key,
                        FieldValue = propertyLocale.Value.ToString()
                    });
                }
            }
            
            return new WebhookResponse<FieldsChangedResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = changes
            };
        }

        [Webhook("On entry published", typeof(EntryPublishedHandler), Description = "On entry published")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryPublished(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry unpublished", typeof(EntryUnpublishedHandler), Description = "On entry unpublished")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryUnpublished(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry archived", typeof(EntryArchivedHandler), Description = "On entry archived")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryArchived(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry unarchived", typeof(EntryUnarchivedHandler), Description = "On entry unarchived")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryUnarchived(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }

        [Webhook("On entry deleted", typeof(EntryDeletedHandler), Description = "On entry deleted")]
        public async Task<WebhookResponse<EntryIdentifier>> EntryDeleted(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<GenericEntryPayload>(webhookRequest);
            return new WebhookResponse<EntryIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new EntryIdentifier { EntryId = data.Sys.Id }
            };
        }
        #endregion

        #region AssetWebhooks

        [Webhook("On asset created", typeof(AssetCreatedHandler), Description = "On asset created")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetCreated(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset saved", typeof(AssetSavedHandler), Description = "On asset saved")]
        public async Task<WebhookResponse<AssetChangedResponse>> AssetSaved(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            
            var changes = new AssetChangedResponse
            {
                AssetId = data.Sys.Id,
                FilesInfo = new List<AssetFileInfo>()
            };
            
            foreach (var propertyLocale in data.Fields.File.Properties())
            {
                var change = propertyLocale.Value.ToObject<AssetFileInfo>();
                change.Locale = propertyLocale.Name;
                changes.FilesInfo.Add(change);
            }
            
            return new WebhookResponse<AssetChangedResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = changes
            };
        }

        [Webhook("On asset auto saved", typeof(AssetAutoSavedHandler), Description = "On asset auto saved")]
        public async Task<WebhookResponse<AssetChangedResponse>> AssetAutoSaved(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            
            var changes = new AssetChangedResponse
            {
                AssetId = data.Sys.Id,
                FilesInfo = new List<AssetFileInfo>()
            };
            
            foreach (var propertyLocale in data.Fields.File.Properties())
            {
                var change = propertyLocale.Value.ToObject<AssetFileInfo>();
                change.Locale = propertyLocale.Name;
                changes.FilesInfo.Add(change);
            }
            
            return new WebhookResponse<AssetChangedResponse>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = changes
            };
        }

        [Webhook("On asset published", typeof(AssetPublishedHandler), Description = "On asset published")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetPublished(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset unpublished", typeof(AssetUnpublishedHandler), Description = "On asset unpublished")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetUnpublished(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset archived", typeof(AssetArchivedHandler), Description = "On asset archived")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetArchived(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset unarchived", typeof(AssetUnarchivedHandler), Description = "On asset unarchived")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetUnarchived(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset deleted", typeof(AssetDeletedHandler), Description = "On asset deleted")]
        public async Task<WebhookResponse<AssetIdentifier>> AssetDeleted(WebhookRequest webhookRequest)
        {
            var data = DeserializePayload<AssetPayload>(webhookRequest);
            return new WebhookResponse<AssetIdentifier>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                Result = new AssetIdentifier { AssetId = data.Sys.Id }
            };
        }
        #endregion

        private static T DeserializePayload<T>(WebhookRequest webhookRequest)
        {
            var payload = JsonConvert.DeserializeObject<T>(webhookRequest.Body.ToString());
            
            if (payload is null)
                throw new InvalidCastException(nameof(webhookRequest.Body));

            return payload;
        }
    }
}
