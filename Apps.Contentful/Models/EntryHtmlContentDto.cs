using HtmlAgilityPack;

namespace Apps.Contentful.Models;

public record EntryHtmlContentDto(string EntryId, HtmlNode HtmlNode);