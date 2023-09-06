using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Webhooks.Handlers
{
    public class BaseWebhookHandler : IWebhookEventHandler
    {
        private readonly string _entityName;
        private readonly string _actionName;

        protected BaseWebhookHandler(string entityName, string actionName)
        {
            _entityName = entityName;
            _actionName = actionName;
        }

        public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, 
            Dictionary<string, string> values)
        {
            var client = new ContentfulClient(authenticationCredentialsProvider);
            var topic = $"{_entityName}.{_actionName}";
            await client.CreateWebhook(new Webhook {
                Name = topic,
                Url = values["payloadUrl"], 
                Topics = new List<string> { topic } 
            });
        }

        public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, 
            Dictionary<string, string> values)
        {
            var client = new ContentfulClient(authenticationCredentialsProvider);
            var topic = $"{_entityName}.{_actionName}";
            var webhooks = client.GetWebhooksCollection().Result;
            var webhook = webhooks.First(w => w.Name == topic);
            await client.DeleteWebhook(webhook.SystemProperties.Id);
        }
    }
}
