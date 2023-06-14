using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntrySavedHandler : BaseWebhookHandler
    {
        public EntrySavedHandler() : base("Entry", "save")
        {
        }
    }
}
