using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Payload
{
    public class GenericEntryPayload
    {
        public SysObject Sys { get; set; }
        public JObject Fields { get; set; }
    }

    public class SysObject
    {
        public string Id { get; set; }
    }
}
