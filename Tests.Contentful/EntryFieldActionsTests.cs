using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class EntryFieldActionsTests : TestBase
{
    [TestMethod]
    public async Task SetJsonFieldContent_WithInvalidJson_ThrowsMisconfigurationException()
    {
        var action = new EntryFieldActions(new InvocationContext
        {
            AuthenticationCredentialsProviders = []
        });

        var exception = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            action.SetJsonFieldContent(
                new EntryLocaleIdentifier { EntryId = "entry-id", Locale = "en-US" },
                new JsonFieldIdentifier { FieldId = "localizationStatus" },
                "{ invalid json }"));

        StringAssert.Contains(exception.Message, "JSON content is invalid");
    }

    [TestMethod]
    public async Task SetJsonFieldContent_WithEmptyJson_ThrowsMisconfigurationException()
    {
        var action = new EntryFieldActions(new InvocationContext
        {
            AuthenticationCredentialsProviders = []
        });

        var exception = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            action.SetJsonFieldContent(
                new EntryLocaleIdentifier { EntryId = "entry-id", Locale = "en-US" },
                new JsonFieldIdentifier { FieldId = "localizationStatus" },
                " "));

        StringAssert.Contains(exception.Message, "JSON content must be provided");
    }

    [TestMethod]
    public async Task GetNumberFieldContent_ValidRequest_ShouldReturnFieldValue()
    {
        var entryFieldActions = new EntryFieldActions(InvocationContext);
        var request = new EntryLocaleIdentifier
        {
            Environment = "master",
            EntryId = "5N76zvCw2PMHTE2rY6pnCo",
            Locale = "nl"
        };

        var response = await entryFieldActions.GetNumberFieldContent(request, new()
        {
            FieldId = "communityProfileId"
        });
        
        Assert.IsNull(response);
        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
    }

    [TestMethod]
    public async Task GetTextFieldContent_ValidRequest_ShouldReturnFieldValue()
    {
        // Arrange
        var action = new EntryFieldActions(InvocationContext);
        var entry = new EntryLocaleIdentifier 
        { 
            EntryId = "5746dLKTkEZjOQX21HX2KI", 
            Locale = "de",
        };
        var field = new FieldIdentifier { FieldId = "title" };

        // Act
        var response = await action.GetTextFieldContent(entry, field);
            
        // Assert
        Assert.IsNotNull(response);
        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
    }

    [TestMethod]
    public async Task GetTextFieldContent_WithNullLocale_ThrowsMisconfigException()
    {
        // Arrange
        var action = new EntryFieldActions(InvocationContext);
        var entry = new EntryLocaleIdentifier
        {
            EntryId = "5746dLKTkEZjOQX21HX2KI",
            Locale = null,
        };
        var field = new FieldIdentifier { FieldId = "title" };

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(
            async () => await action.GetTextFieldContent(entry, field)
        );

        // Assert
        StringAssert.Contains(ex.Message, "Locale must be provided. Please check your input and try again");
    }

    [TestMethod]
    public async Task GetTextFieldContent_WithNullEntryId_ThrowsMisconfigException()
    {
        // Arrange
        var action = new EntryFieldActions(InvocationContext);
        var entry = new EntryLocaleIdentifier
        {
            EntryId = null,
            Locale = "de",
        };
        var field = new FieldIdentifier { FieldId = "title" };

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(
            async () => await action.GetTextFieldContent(entry, field)
        );

        // Assert
        StringAssert.Contains(ex.Message, "Entry ID must be provided. Please check your input and try again");
    }
}
