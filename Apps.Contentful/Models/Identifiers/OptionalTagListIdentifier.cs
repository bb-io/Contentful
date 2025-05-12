using Apps.Contentful.DataSourceHandlers.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers
{
    public class OptionalTagListIdentifier : EnvironmentIdentifier
    {
        [Display("Tag IDs (all must be present)",
            Description = "Filter the incoming events to only trigger when all of the following tags are present")]
        [DataSource(typeof(OptionalTagDataHandler))]
        public IEnumerable<string>? TagIds { get; set; }

        [Display("Tag IDs (at least one of them)",
            Description = "Filter the incoming events to only trigger if any of the following tags are present")]
        [DataSource(typeof(OptionalTagDataHandler))]
        public IEnumerable<string>? AnyTagIds { get; set; }

        [Display("Exclude tag IDs",
            Description = "Filter the incoming events to only trigger when all of the following tags are not present"), DataSource(typeof(OptionalTagDataHandler))]
        public IEnumerable<string>? ExcludeTags { get; set; }
    }
}