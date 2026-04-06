using Apps.Contentful.Services;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using System.Text;
using Tests.Contentful.Base;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Contentful;

[TestClass]
public class CustomSizeValidationServiceTests : TestBase
{
    [TestMethod]
    public async Task Validate_WithExceededCustomSize_ShouldThrowDetailedException()
    {
        var content = await ReadInputFile("MyRepairs.Online_en-US_fr-FR.html");
        var service = new CustomSizeValidationService();

        var exception = ThrowsException<PluginMisconfigurationException>(() =>
            service.Validate(content, "fr-FR", false));

        StringAssert.Contains(exception.Message, "Field 'description'");
        StringAssert.Contains(exception.Message, "entry '3uACdsR62YrTMkBY3T6geU'");
        StringAssert.Contains(exception.Message, "Maximum allowed length: 256");
    }

    [TestMethod]
    public async Task Validate_WithExceededCustomSizeAndSkipEnabled_ShouldReturnWarnings()
    {
        var content = await ReadInputFile("MyRepairs.Online_en-US_fr-FR.html");
        var service = new CustomSizeValidationService();

        var warnings = service.Validate(content, "fr-FR", true);

        AreEqual(1, warnings.Count);
        StringAssert.Contains(warnings.Single().ErrorMessage, "Warning:");
        StringAssert.Contains(warnings.Single().ErrorMessage, "Field 'description'");
    }

    private async Task<string> ReadInputFile(string fileName)
    {
        using var stream = await FileManager.DownloadAsync(new FileReference
        {
            Name = fileName,
            ContentType = "text/html"
        });

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
