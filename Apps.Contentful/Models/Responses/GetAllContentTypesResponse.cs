using Apps.Contentful.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Responses
{
    public class GetAllContentTypesResponse
    {
        public IEnumerable<ContentTypeDto> ContentTypes { get; set; }
    }
}
