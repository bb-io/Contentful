using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Webhooks.Handlers
{
    public class BaseWebhookHandler : IWebhookEventHandler
    {
        private string EntityName;
        private string ActionName;
        private string _spaceId;

        public BaseWebhookHandler(string entityName, string actionName, [WebhookParameter] string spaceId)
        {
            EntityName = entityName;
            ActionName = actionName;
            _spaceId = spaceId;
        }

        public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, Dictionary<string, string> values)
        {
            var client = new ContentfulClient(authenticationCredentialsProvider, _spaceId);
            var topic = $"{EntityName}.{ActionName}";
            await client.CreateWebhook(new Webhook() {
                Name = topic,
                Url = values["payloadUrl"], 
                Topics = new List<string>() { topic } 
            });
        }

        public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, Dictionary<string, string> values)
        {
            var client = new ContentfulClient(authenticationCredentialsProvider, _spaceId);
            var topic = $"{EntityName}.{ActionName}";
            var webhooks = client.GetWebhooksCollection().Result;
            var webhook = webhooks.First(w => w.Name == topic);
            await client.DeleteWebhook(webhook.SystemProperties.Id);
        }
    }
}
