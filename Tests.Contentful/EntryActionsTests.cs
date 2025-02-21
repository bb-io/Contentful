using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.Contentful.Base;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Contentful;

[TestClass]
public class EntryActionsTests : TestBase
{
    [TestMethod]
    public async Task ListEntries_NotExistingEnvironment_ShouldFailWithException()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest { Environment = "temp_empty_for_test" };
        var listEntriesTask = entryActions.ListEntries(listEntriesRequest);

        await ThrowsExceptionAsync<PluginApplicationException>(() => listEntriesTask);
    }
    
    [TestMethod]
    public async Task ListEntries_ValidEnvironment_ShouldReturnAllEntries()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest { Environment = "dev" };
        
        var entriesResponse = await entryActions.ListEntries(listEntriesRequest);

        foreach (var entry in entriesResponse.Entries)
        {
            Console.WriteLine($"{entry.Id}");
        }
    }

    [TestMethod]
    public async Task GetEntryLocalizableFieldsAsHtmlFile_WithoutReferenceEntries_ShouldGenerateHtmlFile()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new EntryLocaleIdentifier()
        {
            Environment = "dev",
            EntryId = "5wFvto8Zhatz451gTDEpvP",
            Locale = "en-US"
        };
        var request = new GetEntryAsHtmlRequest();
        
        var fileResponse = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, request);
        
        IsFalse(string.IsNullOrEmpty(fileResponse.File.ToString()));
    }
    
    
    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithoutReferenceEntries_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new LocaleIdentifier()
        {
            Environment = "dev",
            Locale = "nl"
        };
        var fileRequest = new FileRequest
        {
            File = new()
            {
                Name = "5wFvto8Zhatz451gTDEpvP_en-US.html",
                ContentType = "text/html"
            }
        };
        
        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier, fileRequest);
    }
}