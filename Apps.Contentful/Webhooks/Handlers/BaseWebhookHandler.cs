using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Webhooks.Handlers
{
    public class BaseWebhookHandler : IWebhookEventHandler
    {
        private readonly string _entityName;
        private readonly string _actionName;
        private readonly SpaceIdentifier _space;

        protected BaseWebhookHandler(string entityName, string actionName, [WebhookParameter] SpaceIdentifier space)
        {
            _entityName = entityName;
            _actionName = actionName;
            _space = space;
        }

        public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, 
            Dictionary<string, string> values)
        {
            var client = new ContentfulClient(authenticationCredentialsProvider, _space.Id);
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
            var client = new ContentfulClient(authenticationCredentialsProvider, _space.Id);
            var topic = $"{_entityName}.{_actionName}";
            var webhooks = client.GetWebhooksCollection().Result;
            var webhook = webhooks.First(w => w.Name == topic);
            await client.DeleteWebhook(webhook.SystemProperties.Id);
        }
    }
}
