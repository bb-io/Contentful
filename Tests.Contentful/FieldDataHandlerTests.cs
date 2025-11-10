using Apps.Contentful.DataSourceHandlers;
using Newtonsoft.Json;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class FieldDataHandlerTests : TestBase
{
    [TestMethod]
    public async Task GetDataAsync_ValidRequest_ReturnsFields()
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
    public async Task GetDataAsync_FromModel_ReturnsFields()
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
}