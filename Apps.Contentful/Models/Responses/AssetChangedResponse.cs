using Apps.Contentful.Webhooks.Payload;
using Contentful.Core.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Responses
{
    public class AssetChangedResponse
    {
        public string AssetId { get; set; }

        public List<AssetFileInfo> FilesInfo { get; set; }
    }
}
