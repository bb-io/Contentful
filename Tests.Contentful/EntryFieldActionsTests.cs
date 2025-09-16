using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Newtonsoft.Json;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class EntryFieldActionsTests : TestBase
{
    [TestMethod]
    public async Task GetNumberFieldContent_ValidRequest_ShouldReturnFieldValue()
    {
        var entryFieldActions = new EntryFieldActions(InvocationContext);
        var request = new EntryLocaleIdentifier
        {
            Environment = "master",
            EntryId = "5N76zvCw2PMHTE2rY6pnCo",
            Locale = "en-US"
        };

        var response = await entryFieldActions.GetNumberFieldContent(request, new()
        {
            FieldId = "communityProfileId"
        });
        
        Assert.IsNull(response);
        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
    }
}