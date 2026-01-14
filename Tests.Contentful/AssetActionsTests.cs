using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class AssetActionsTests : TestBase
{
    [TestMethod]
    public async Task GetAssetAsHtml_ValidAsset_ShouldReturnHtmlFile()
    {
        // Arrange
        var assetActions = new AssetActions(InvocationContext, FileManager);
        var assetIdentifier = new AssetLocaleIdentifier
        {
            Environment = "dev",
            AssetId = "7k2NErWkmulHH2do3O5sHh",
            Locale = "en-US"
        };

        // Act
        var fileResponse = await assetActions.GetAssetAsHtml(assetIdentifier);

        // Assert
        Assert.IsNotNull(fileResponse);
        Assert.IsNotNull(fileResponse.File);
        Assert.IsFalse(string.IsNullOrEmpty(fileResponse.File.Name));
        Assert.AreEqual("text/html", fileResponse.File.ContentType);
        
        Console.WriteLine($"Generated HTML file: {fileResponse.File.Name}");
    }

    [TestMethod]
    public async Task GetAsset_ValidAsset_ShouldReturn()
    {
        // Arrange
        var assetActions = new AssetActions(InvocationContext, FileManager);
        var assetIdentifier = new AssetLocaleIdentifier
        {
            //Environment = "dev",
            AssetId = "0657846a-dac4-4d58-a23b-c649cf5a05f6",
            Locale = "en-US"
        };

        // Act
        var fileResponse = await assetActions.GetAssetById(assetIdentifier);

        // Assert
        Assert.IsNotNull(fileResponse);

        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(fileResponse));
    }

    [TestMethod]
    public async Task UpdateAssetFromHtml_ValidAsset_ShouldNotFail()
    {
        // Arrange
        var assetActions = new AssetActions(InvocationContext, FileManager);
        var fileRequest = new UpdateAssetFromHtmlRequest
        {
            File = new()
            {
                Name = "Green bird.html",
                ContentType = "text/html"
            },
            Locale = "nl"
        };
        
        // Act & Assert - If no exception is thrown, the test passes
        await assetActions.UpdateAssetFromHtml(fileRequest);
        
        Console.WriteLine("Asset update from HTML completed successfully");
    }
}
