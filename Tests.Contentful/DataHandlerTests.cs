using Newtonsoft.Json;
using Tests.Contentful.Base;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.DataSourceHandlers;

namespace Tests.Contentful;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task FieldDataHandler_ReturnsFields()
    {
        var dataHandler = new FieldDataHandler(InvocationContext, new()
        {
            EntryId = "5N76zvCw2PMHTE2rY6pnCo",
            Locale = "en-US",
            Environment = "master"
        });
        
        var result = await dataHandler.GetDataAsync(new(), CancellationToken.None);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any());
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [TestMethod]
    public async Task FieldFromModelDataHandler_ReturnsFields()
    {
        var dataHandler = new FieldFromModelDataHandler(InvocationContext, new()
        {
            ContentModelId = "vitaliiTest",
            Environment = "master"
        });
        
        var result = await dataHandler.GetDataAsync(new(), CancellationToken.None);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any());
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [TestMethod]
    public async Task LocaleDataSourceHandler_ReturnsLocales()
    {
        // Arrange
        var entry = new EnvironmentIdentifier { Environment = "master" };
        var handler = new LocaleDataSourceHandler(InvocationContext, entry);

        // Act
        var result = await handler.GetDataAsync(new(), default);

        // Assert
        Assert.IsNotNull(result);
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }
}