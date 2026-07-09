using Newtonsoft.Json;

namespace Apps.Contentful.Dtos.Raw;

public class UserItem
{
    [JsonProperty("sys")]
    public UserSystemProperties SystemProperties { get; set; } = new();

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }
}

public class UserSystemProperties
{
    public string Id { get; set; } = string.Empty;
}
