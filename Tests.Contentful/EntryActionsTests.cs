using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Filters.Coders;
using Newtonsoft.Json;
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
        var listEntriesRequest = new ListEntriesRequest { Environment = "dev", SearchTerm = "The perfect partner" };

        var entriesResponse = await entryActions.ListEntries(listEntriesRequest);

        foreach (var entry in entriesResponse.Entries)
        {
            Console.WriteLine($"{entry.ContentId}");
        }
    }

    [TestMethod]
    public async Task ListEntries_WithValidPublishedDateFilters_ShouldReturnFilteredEntries()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest
        {
            PublishedAfter = new DateTime(2025, 10, 01),
            PublishedBefore = new DateTime(2025, 10, 04)
        };

        // Act
        var result = await entryActions.ListEntries(listEntriesRequest);

        // Assert
        foreach (var entry in result.Entries)
        {
            Console.WriteLine($"{entry.ContentId}");
        }
        IsNotNull(result);
    }

    [TestMethod]
    public async Task ListEntries_WithInvalidPublishedDateFilters_ShouldFailWithException()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest
        {
            PublishedAfter = new DateTime(2025, 10, 04),
            PublishedBefore = new DateTime(2025, 10, 01)
        };

        // Act & Assert
        await ThrowsExceptionAsync<PluginMisconfigurationException>(async () => await entryActions.ListEntries(listEntriesRequest));
    }

    [TestMethod]
    public async Task ListEntries_WithValidFirstPublishedDateFilters_ShouldReturnFilteredEntries()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest
        {
            FirstPublishedAfter = new DateTime(2025, 08, 31),
            FirstPublishedBefore = new DateTime(2025, 09, 02)
        };

        // Act
        var result = await entryActions.ListEntries(listEntriesRequest);

        // Assert
        foreach (var entry in result.Entries)
        {
            Console.WriteLine($"{entry.ContentId}");
        }
        IsNotNull(result);
    }

    [TestMethod]
    public async Task ListEntries_WithValidMixedDateFilters_ShouldReturnFilteredEntries()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest
        {
            FirstPublishedAfter = new DateTime(2025, 08, 31),
            PublishedBefore = new DateTime(2025, 09, 06)
        };

        // Act
        var result = await entryActions.ListEntries(listEntriesRequest);

        // Assert
        foreach (var entry in result.Entries)
        {
            Console.WriteLine($"{entry.ContentId}");
        }
        IsNotNull(result);
    }

    [TestMethod]
    public async Task GetEntryLocalizableFieldsAsHtmlFile_WithoutReferenceEntries_ShouldGenerateHtmlFile()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput()
        {
            Environment = "dev",
            ContentId = "70rcIvfhhD9VjLlMTA3YPz",
            Locale = "en-US"
        };
        var request = new GetEntryAsHtmlRequest();

        var fileResponse = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, request);

        IsFalse(string.IsNullOrEmpty(fileResponse.Content.ToString()));
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithHyperlinkEntries_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "Example_en-US.html",
                ContentType = "text/html"
            }
        };

        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithReferenceEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "First reference entry_en-US.html",
                ContentType = "text/html"
            },
            DontUpdateReferenceFields = true
        };

        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithMarkdownEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "Markdown entry #1_en-US.html",
                ContentType = "text/html"
            },
            DontUpdateReferenceFields = true
        };

        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithImageAltsTranslated_ShouldNotFail()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            ContentId = "5746dLKTkEZjOQX21HX2KI",
            Environment = "master",
            Locale = "nl",
            Content = new() { Name = "The Loire Valley_en-US.html (3).xlf" }
        };

        // Act
        var result = await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        IsNotNull(result);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_MxliffFile_ShouldFailWithException()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Locale = "nl",
            Content = new() { Name = "empty.mxliff" }
        };

        // Act & Assert
        await ThrowsExceptionAsync<PluginMisconfigurationException>(
            async () => await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier)
        );
    }

    [TestMethod]
    public async Task GetEntry_ValidEntryWithoutLocale_ShouldReturnEntryWithTitle()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new EntryIdentifier
        {
            Environment = "dev",
            EntryId = "5wFvto8Zhatz451gTDEpvP"
        };
        var localeIdentifier = new LocaleOptionalIdentifier();

        var entry = await entryActions.GetEntry(entryIdentifier, localeIdentifier);

        IsNotNull(entry);
        IsNotNull(entry.Title);
        Console.WriteLine($"Entry title: {entry.Title}");
    }

    [TestMethod]
    public async Task AddTagEntry_ShouldReturnEntryWithTag()
    {
        var entryActions = new TagActions(InvocationContext);
        var entryIdentifier = new EntryTagIdentifier
        {
            TagId = "23232",
            EntryId = "2kgmrDpJKTSqk3prI14WjX"
        };

        await entryActions.AddEntryTag(entryIdentifier);


        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task DownloadEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = "en-US",
            ContentId = "5746dLKTkEZjOQX21HX2KI",

        };
        var entry = new GetEntryAsHtmlRequest
        {
            IgnoredFieldIds = new List<string>
           {
                "slug"
           },
            GetHyperlinkContent = true,
            IgnoredContentTypeIds = new List<string>
            {
                "page"
            },
            GetEmbeddedBlockContent = true,
            GetEmbeddedInlineContent = true,
            GetNonLocalizationReferenceContent = true
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task DownloadEntry_Has_Blacklake_required_fields()
    {
        var lang = "en-US";
        var contentId = "5746dLKTkEZjOQX21HX2KI";

        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = lang,
            ContentId = contentId,

        };
        var entry = new GetEntryAsHtmlRequest
        {
            GetReferenceContent = true,
            GetEmbeddedInlineContent = true,
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        var contentString = FileManager.ReadOutputAsString(response.Content);
        var codedContent = (new HtmlContentCoder()).Deserialize(contentString, response.Content.Name);

        foreach (var unit in codedContent.TextUnits.Where(x => x.Key is null))
        {
            Console.WriteLine(unit.GetCodedText());
        }

        Console.WriteLine(contentString);
        Assert.AreEqual(lang, codedContent.Language);
        Assert.AreEqual(contentId, codedContent.SystemReference.ContentId);
        Assert.AreEqual($"https://app.contentful.com/spaces/12ktqqmw656e/entries/{contentId}", codedContent.SystemReference.AdminUrl);
        Assert.AreEqual("Contentful", codedContent.SystemReference.SystemName);
        Assert.AreEqual("https://www.contentful.com", codedContent.SystemReference.SystemRef);
        Assert.IsNotNull(codedContent.SystemReference.ContentName);

        Assert.IsNotNull(codedContent.Provenance.Review.Person);
        Console.WriteLine(codedContent.Provenance.Review.Person);
        Assert.AreEqual("Contentful", codedContent.Provenance.Review.Tool);
        Assert.AreEqual("https://www.contentful.com", codedContent.Provenance.Review.ToolReference);

        Assert.IsTrue(codedContent.TextUnits.All(x => x.Key is not null));
    }

    [TestMethod]
    public async Task DownloadEntry_Has_Size_Restrictions()
    {
        var lang = "en-US";
        var contentId = "7pB35K9cZjsZrBmnUjwjLa";

        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = lang,
            ContentId = contentId,

        };
        var entry = new GetEntryAsHtmlRequest
        {
            GetReferenceContent = true,
            GetEmbeddedInlineContent = true,
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        Console.WriteLine(response.Content.Name);
        var contentString = FileManager.ReadOutputAsString(response.Content);
        var codedContent = (new HtmlContentCoder()).Deserialize(contentString, response.Content.Name);

        Console.WriteLine(contentString);

        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 256));
        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 60));
        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 160));
    }

    [TestMethod]
    public async Task GetReferenceEntries_WithoutFieldIds_ShouldReturnAllReferencedEntries()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var request = new GetReferenceEntriesRequest
        {
            EntryId = "1973QRvX9m84FWpFpC7ZnH"
        };

        // Act
        var response = await entryActions.GetReferenceEntries(request);

        // Assert
        IsNotNull(response);
        IsNotNull(response.ReferencedEntries);
        IsNotNull(response.ReferencedEntryIds);

        Console.WriteLine($"Found {response.ReferencedEntries.Count()} referenced entries:");
        foreach (var entry in response.ReferencedEntries)
        {
            Console.WriteLine($"- Entry ID: {entry.ContentId}, Content Type: {entry.ContentTypeId}");
        }

        Console.WriteLine($"\nReferenced Entry IDs:");
        foreach (var entryId in response.ReferencedEntryIds)
        {
            Console.WriteLine($"- {entryId}");
        }

        // Verify that both collections have the same count
        AreEqual(response.ReferencedEntries.Count(), response.ReferencedEntryIds.Count());
    }

    [TestMethod]
    public async Task GetEntriesLinkingToEntry_ShouldWork()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entry = new EntryIdentifier { EntryId = "1973QRvX9m84FWpFpC7ZnH" };
        var contentModel = new ContentModelOptionalIdentifier();

        // Act
        var response = await entryActions.GetEntriesLinkingToEntry(entry, contentModel);

        // Assert
        IsTrue(response.Entries.Any());
        IsTrue(response.EntriesIds.Any());
        AreNotEqual(response.FirstEntryId, string.Empty);
        IsTrue(response.TotalCount > 0);

        Console.WriteLine(JsonConvert.SerializeObject(response.EntriesIds, Formatting.Indented));
    }

    [TestMethod]
    public async Task GetEntriesLinkingToEntry_WithModelFilter_ShouldWork()
    {
        // Arrange
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entry = new EntryIdentifier { EntryId = "1973QRvX9m84FWpFpC7ZnH" };
        var contentModel = new ContentModelOptionalIdentifier { ContentModelId = "pageWrapper" };

        // Act
        var response = await entryActions.GetEntriesLinkingToEntry(entry, contentModel);

        // Assert
        IsTrue(response.Entries.Any());
        IsTrue(response.EntriesIds.Any());
        AreNotEqual(response.FirstEntryId, string.Empty);
        IsTrue(response.TotalCount > 0);

        Console.WriteLine(JsonConvert.SerializeObject(response.EntriesIds, Formatting.Indented));
    }
}