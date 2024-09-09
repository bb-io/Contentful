using System.Web;
using Contentful.Core.Models;
using HtmlAgilityPack;

namespace Apps.Contentful.HtmlHelpers;

public class HtmlToRichTextConverter
{
    public Document ToRichText(string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var contentfulDocument = new Document();
        contentfulDocument.NodeType = "document";
        contentfulDocument.Data = new GenericStructureData();
        contentfulDocument.Content = new List<IContent>();
        ParseHtmlToContentful(htmlDocument.DocumentNode, contentfulDocument.Content);
        return contentfulDocument;
    }
    
    private void ParseHtmlToContentful(HtmlNode node, List<IContent> contentList)
    {
        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.NodeType == HtmlNodeType.Element)
            {
                IContent content = null;

                switch (childNode.Name)
                {
                    case "h1":
                        content = CreateHeading(childNode, 1);
                        break;
                    case "h2":
                        content = CreateHeading(childNode, 2);
                        break;
                    case "h3":
                        content = CreateHeading(childNode, 3);
                        break;
                    case "h4":
                        content = CreateHeading(childNode, 4);
                        break;
                    case "h5":
                        content = CreateHeading(childNode, 5);
                        break;
                    case "h6":
                        content = CreateHeading(childNode, 6);
                        break;
                    case "p":
                        content = CreateParagraph(childNode);
                        break;
                    case "span":
                        content = new Text()
                        {
                            NodeType = "text",
                            Marks = new(),
                            Data = new(),
                            Value = string.Empty
                        };
                        break;
                    case "ul":
                        content = CreateUnorderedList(childNode);
                        break;
                    case "ol":
                        content = CreateOrderedList(childNode);
                        break;
                    case "blockquote":
                        content = CreateBlockQuote(childNode);
                        break;
                    case "hr":
                        content = CreateHorizontalRuler();
                        break;
                    case "table":
                        content = CreateTable(childNode);
                        break;
                    case "a":
                        content = CreateHyperlink(childNode);
                        break;
                    default:
                        ParseHtmlToContentful(childNode, contentList);
                        break;
                }

                if (content != null)
                    contentList.Add(content);
            }
            else if (childNode.NodeType == HtmlNodeType.Text)
            {
                var text = childNode.InnerText;
                if(string.IsNullOrWhiteSpace(text))
                    continue;
                
                var marks = new List<string>();
                GetMarksFromHtmlNode(childNode, marks);

                var textNode = new Text
                {
                    NodeType = "text",
                    Value = HttpUtility.HtmlDecode(text),
                    Data = new GenericStructureData(),
                    Marks = new List<Mark>(marks.Select(mark => new Mark { Type = mark }))
                };
                contentList.Add(textNode);
            }
        }
    }

    private IContent CreateHeading(HtmlNode node, int level)
    {
        switch (level)
        {
            case 1:
                var heading1 = new Heading1
                {
                    NodeType = "heading-1",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading1.Content);
                return heading1;
            case 2:
                var heading2 = new Heading2
                {
                    NodeType = "heading-2",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading2.Content);
                return heading2;
            case 3:
                var heading3 = new Heading3
                {
                    NodeType = "heading-3",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading3.Content);
                return heading3;
            case 4:
                var heading4 = new Heading1
                {
                    NodeType = "heading-4",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading4.Content);
                return heading4;
            case 5:
                var heading5 = new Heading5
                {
                    NodeType = "heading-5",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading5.Content);
                return heading5;
            case 6:
                var heading6 = new Heading6
                {
                    NodeType = "heading-6",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };
                ParseHtmlToContentful(node, heading6.Content);
                return heading6;
            default:
                throw new Exception();
        }
    }

    private Paragraph CreateParagraph(HtmlNode node)
    {
        var paragraph = new Paragraph
        {
            NodeType = "paragraph",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };

        ParseHtmlToContentful(node, paragraph.Content);
        if (!node.ChildNodes.Any())
            paragraph.Content.Add(new Text()
            {
                NodeType = "text",
                Marks = new(),
                Data = new(),
                Value = string.Empty
            });

        return paragraph;
    }

    private List CreateOrderedList(HtmlNode node)
    {
        var list = new List
        {
            NodeType = "ordered-list",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };

        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.Name == "li")
            {
                var listItem = new ListItem
                {
                    NodeType = "list-item",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };

                ParseHtmlToContentful(childNode, listItem.Content);
                list.Content.Add(listItem);
            }
        }

        return list;
    }
    
    private List CreateUnorderedList(HtmlNode node)
    {
        var list = new List
        {
            NodeType = "unordered-list",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };

        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.Name == "li")
            {
                var listItem = new ListItem
                {
                    NodeType = "list-item",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };

                ParseHtmlToContentful(childNode, listItem.Content);
                list.Content.Add(listItem);
            }
        }

        return list;
    }

    private Quote CreateBlockQuote(HtmlNode node)
    {
        var blockQuote = new Quote
        {
            NodeType = "blockquote",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };

        ParseHtmlToContentful(node, blockQuote.Content);
        return blockQuote;
    }

    private HorizontalRuler CreateHorizontalRuler() => 
        new()
        {
            NodeType = "hr",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };

    private Table CreateTable(HtmlNode node)
    {
        var table = new Table
        {
            NodeType = "table",
            Data = new GenericStructureData(),
            Content = new List<IContent>()
        };
        
        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.Name == "tr")
            {
                var tableRow = new TableRow
                {
                    NodeType = "table-row",
                    Data = new GenericStructureData(),
                    Content = new List<IContent>()
                };

                foreach (var cellNode in childNode.ChildNodes)
                {
                    if (cellNode.Name == "td")
                    {
                        var tableCell = new TableCell
                        {
                            NodeType = "table-cell",
                            Data = new TableCellData(),
                            Content = new List<IContent>()
                        };

                        ParseHtmlToContentful(cellNode, tableCell.Content);
                        tableRow.Content.Add(tableCell);
                    }

                    else if (cellNode.Name == "th")
                    {
                        var tableCell = new TableCell
                        {
                            NodeType = "table-header-cell",
                            Data = new TableCellData(),
                            Content = new List<IContent>()
                        };

                        ParseHtmlToContentful(cellNode, tableCell.Content);
                        tableRow.Content.Add(tableCell);
                    }
                }

                table.Content.Add(tableRow);
            }
        }

        return table;
    }

    private IContent CreateHyperlink(HtmlNode node)
    {
        var uri = node.GetAttributeValue("href", "");
        var id = node.GetAttributeValue("id", "");

        switch (id)
        {
            case var value when value.StartsWith("asset-hyperlink_") || value.StartsWith("embedded-asset-block_"):
                var nodeType = value.Split("_")[0];
                var assetId = value.Split("_")[^1];
                var assetHyperlink = new AssetHyperlink
                {
                    NodeType = nodeType,
                    Content = new List<IContent>(),
                    Data = new AssetHyperlinkData
                    {
                        Target = new Asset
                        {
                            SystemProperties = new SystemProperties
                            {
                                Id = assetId,
                                Type = "Link",
                                LinkType = "Asset"
                            }
                        }
                    }
                };
                
                if (nodeType == "asset-hyperlink")
                    ParseHtmlToContentful(node, assetHyperlink.Content);
                
                return assetHyperlink;
            
            case var value when value.StartsWith("entry-hyperlink_") || value.StartsWith("embedded-entry-block_")
                                                                     || value.StartsWith("embedded-entry-inline_"):
                nodeType = value.Split("_")[0];
                var entryId = value.Split("_")[^1];
                var entryHyperlink = new EntryStructure
                {
                    NodeType = nodeType,
                    Content = new List<IContent>(),
                    Data = new EntryStructureData
                    {
                        Target = new Asset
                        {
                            SystemProperties = new SystemProperties
                            {
                                Id = entryId,
                                Type = "Link",
                                LinkType = "Entry"
                            }
                        }
                    }
                };
                
                if (nodeType == "entry-hyperlink")
                    ParseHtmlToContentful(node, entryHyperlink.Content);
                
                return entryHyperlink;
            
            default:
                var hyperlink = new Hyperlink
                {
                    NodeType = "hyperlink",
                    Content = new List<IContent>(),
                    Data = new HyperlinkData { Uri = uri }
                };
                ParseHtmlToContentful(node, hyperlink.Content);
                return hyperlink;
        }
    }

    private void GetMarksFromHtmlNode(HtmlNode node, List<string> marks)
    {
        var parent = node.ParentNode;
        switch (parent.Name)
        {
            case "b":
                marks.Add("bold");
                GetMarksFromHtmlNode(parent, marks);
                break;
            case "i":
                marks.Add("italic");
                GetMarksFromHtmlNode(parent, marks);
                break;
            case "u":
                marks.Add("underline");
                GetMarksFromHtmlNode(parent, marks);
                break;
            case "code":
                marks.Add("code");
                GetMarksFromHtmlNode(parent, marks);
                break;
            case "sup":
                marks.Add("superscript");
                GetMarksFromHtmlNode(parent, marks);
                break;
            case "sub":
                marks.Add("subscript");
                GetMarksFromHtmlNode(parent, marks);
                break;
            default:
                return;
        }
    }
}