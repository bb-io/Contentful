using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Responses
{
    public class GetAssetResponse
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Filename { get; set; }

        public byte[] File { get; set; }
    }
}
