using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class EntryTranslationTests : TestBase
{
    [TestMethod]
    public async Task Translate_standard_html_file()
    {
        var actions = new EntryActions(InvocationContext, FileManager);
        await actions.SetEntryLocalizableFieldsFromHtmlFile(
            new LocaleIdentifier { Locale = "de" },
            new FileRequest { File = new FileReference { Name = "contentful-pseudo.html"} },
            new UpdateEntryFromHtmlRequest { });

    }

    [TestMethod]
    public async Task Translate_from_xliff_file()
    {
        var actions = new EntryActions(InvocationContext, FileManager);
        await actions.SetEntryLocalizableFieldsFromHtmlFile(
            new LocaleIdentifier { Locale = "de" },
            new FileRequest { File = new FileReference { Name = "contentful-pseudo.xliff" } },
            new UpdateEntryFromHtmlRequest { });

    }
}
