namespace Apps.Contentful.Models.Requests.Base;

public class PatchRequest<T>
{
    public string Op { get; set; }
    
    public string Path { get; set; }
    
    public T Value { get; set; }
}