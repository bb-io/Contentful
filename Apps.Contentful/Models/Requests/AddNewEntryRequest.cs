using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Requests
{
    public class AddNewEntryRequest
    {
        public string SpaceId { get; set; }
        public string ContentModelId { get; set; }
    }
}
