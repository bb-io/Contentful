using Apps.Contentful.Utils;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Contentful;

[TestClass]
public class ReferencedEntryContentTypeFilterTests
{
    [TestMethod]
    public void Allows_WithoutAnyContentTypeIds_ShouldAllowEveryContentType()
    {
        var filter = new ReferencedEntryContentTypeFilter(null, null);

        IsTrue(filter.Allows("page"));
        IsTrue(filter.Allows("component"));
    }

    [TestMethod]
    public void Allows_WithIncludedContentTypeIds_ShouldAllowOnlyListedContentTypes()
    {
        var filter = new ReferencedEntryContentTypeFilter(null, ["component", "section"]);

        IsTrue(filter.Allows("component"));
        IsTrue(filter.Allows("section"));
        IsFalse(filter.Allows("page"));
        IsFalse(filter.Allows("Component"));
    }

    [TestMethod]
    public void Allows_WithExcludedContentTypeIds_ShouldRejectListedContentTypes()
    {
        var filter = new ReferencedEntryContentTypeFilter(["author"], []);

        IsFalse(filter.Allows("author"));
        IsTrue(filter.Allows("component"));
    }

    [TestMethod]
    public void Allows_WithContentTypeBothIncludedAndExcluded_ShouldReject()
    {
        var filter = new ReferencedEntryContentTypeFilter(["component"], ["component", "section"]);

        IsFalse(filter.Allows("component"));
        IsTrue(filter.Allows("section"));
    }
}
