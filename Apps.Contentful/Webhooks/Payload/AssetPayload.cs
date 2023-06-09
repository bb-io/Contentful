﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Payload
{
    public class AssetPayload
    {
        public SysObject Sys { get; set; }

        public AssetFields Fields { get; set; }
    }

    public class AssetFields
    {
        public JObject File { get; set; }
    }

    public class AssetFileInfo
    {
        public string Locale { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
