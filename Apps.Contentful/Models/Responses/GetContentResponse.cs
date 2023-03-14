using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Responses
{
    public class GetContentResponse
    {
        public string Name { get; set; }

        public string UserUrl { get; set; }

        public int PublicRepositoriesNumber { get; set; }
    }
}
