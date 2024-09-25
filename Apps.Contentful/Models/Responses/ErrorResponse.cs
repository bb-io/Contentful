using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Models.Responses;

public class ErrorResponse
{
    public string Message { get; set; }
    
    public JObject Details { get; set; }
}

