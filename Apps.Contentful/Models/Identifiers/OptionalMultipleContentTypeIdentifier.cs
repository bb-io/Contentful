using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.DataSourceHandlers.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Identifiers
{
    public class OptionalMultipleContentTypeIdentifier
    {
        [Display("Content types",
            Description = "Filter the incoming events to only trigger when it is of any of the following content types")]
        [DataSource(typeof(ContentModelDataSourceHandler))]
        public IEnumerable<string>? ContentModels { get; set; }
    }
}
