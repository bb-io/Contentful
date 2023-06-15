using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Requests
{
    public class UpdateAssetFileRequest
    {
        public string AssetId { get; set; }

        public string SpaceId { get; set; }

        public string Locale { get; set; }

        public string Filename { get; set; }

        public byte[] File { get; set; }
    }
}
