using Apps.Contentful.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Requests
{
    public class CreateEntryRequest
    {
        public string ContentTypeId { get; set; }

        public TestDto Content { get; set; }
    }
}
