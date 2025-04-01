using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Responses;

public class FileResponse
{
    public FileReference File { get; set; }

    public FileResponse()
    {
        File = default!;
    }

    public FileResponse(FileReference file)
    {
        File = file;
    }
}