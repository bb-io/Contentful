using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Handlers
{
    public class EntryUnpublishedHandler : BaseWebhookHandler
    {
        public EntryUnpublishedHandler() : base("Entry", "unpublish")
        {
        }
    }
}
