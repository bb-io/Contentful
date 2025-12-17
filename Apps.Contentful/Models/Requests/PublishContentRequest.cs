using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class PublishContentRequest : EnvironmentIdentifier
{
    public FileReference Content { get; set; }
}
