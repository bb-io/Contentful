using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetArchievedHandler : BaseWebhookHandler
    {
        public AssetArchievedHandler() : base("Asset", "archive")
        {
        }
    }
}
