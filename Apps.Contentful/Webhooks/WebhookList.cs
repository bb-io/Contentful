using Apps.Contentful.Dtos;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers;
using Apps.Contentful.Webhooks.Handlers.EntryHandlers;
using Apps.Contentful.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Threading.Channels;

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

        #region AssetWebhooks

        [Webhook("On asset created", typeof(AssetCreatedHandler), Description = "On asset created")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetCreation(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset saved", typeof(AssetSavedHandler), Description = "On asset saved")]
        public async Task<WebhookResponse<AssetChangedResponse>> AssetSaved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            var changes = new AssetChangedResponse()
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
                HttpResponseMessage = null,
                Result = changes
            };
        }

        [Webhook("On asset auto saved", typeof(AssetAutosavedHandler), Description = "On asset auto saved")]
        public async Task<WebhookResponse<AssetChangedResponse>> AssetAutosaved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            var changes = new AssetChangedResponse()
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
                HttpResponseMessage = null,
                Result = changes
            };
        }

        [Webhook("On asset published", typeof(AssetPublishedHandler), Description = "On asset published")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetPublished(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset unpublished", typeof(AssetUnpublishedHandler), Description = "On asset unpublished")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetUnpublished(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset archieved", typeof(AssetArchievedHandler), Description = "On asset archieved")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetArchieved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset unarchieved", typeof(AssetUnarchievedHandler), Description = "On asset unarchieved")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetUnarchieved(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }

        [Webhook("On asset deleted", typeof(AssetDeletedHandler), Description = "On asset deleted")]
        public async Task<WebhookResponse<CreateAssetResponse>> AssetDeleted(WebhookRequest webhookRequest)
        {
            var data = JsonConvert.DeserializeObject<AssetPayload>(webhookRequest.Body.ToString());
            if (data is null)
            {
                throw new InvalidCastException(nameof(webhookRequest.Body));
            }
            return new WebhookResponse<CreateAssetResponse>
            {
                HttpResponseMessage = null,
                Result = new CreateAssetResponse() { AssetId = data.Sys.Id }
            };
        }
        #endregion
    }
}
