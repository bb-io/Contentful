using Apps.Contentful.HtmlHelpers.Constants;
using Apps.Contentful.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Apps.Contentful.Services;

public class CustomSizeValidationService
{
    public IReadOnlyCollection<ContentProcessingError> Validate(string content, string locale, bool skipValidation)
    {
        var violations = GetViolations(content);
        if (!violations.Any())
        {
            return [];
        }

        if (!skipValidation)
        {
            throw new PluginMisconfigurationException(BuildExceptionMessage(violations, locale));
        }

        return violations.Select(violation => new ContentProcessingError
        {
            EntryId = violation.EntryId,
            ParentEntryId = null,
            ErrorMessage = $"Warning: {BuildViolationMessage(violation, locale)} Contentful may reject this field during upload."
        }).ToArray();
    }

    private static List<CustomSizeConstraintViolation> GetViolations(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var nodes = doc.DocumentNode.SelectNodes("//*[@data-blackbird-size and @data-contentful-field-id]");
        if (nodes == null)
        {
            return [];
        }

        var violations = new List<CustomSizeConstraintViolation>();
        foreach (var node in nodes)
        {
            var rawConstraint = node.GetAttributeValue("data-blackbird-size", string.Empty);
            if (!TryGetMaximumSize(rawConstraint, out var maximumLength))
            {
                continue;
            }

            var fieldValue = HttpUtility.HtmlDecode(node.InnerText ?? string.Empty);
            var actualLength = fieldValue.Length;
            if (actualLength <= maximumLength)
            {
                continue;
            }

            var fieldId = node.GetAttributeValue(ConvertConstants.FieldIdAttribute, string.Empty);
            var entryId = node.AncestorsAndSelf()
                .FirstOrDefault(x => x.Attributes[ConvertConstants.EntryIdAttribute] != null)?
                .GetAttributeValue(ConvertConstants.EntryIdAttribute, string.Empty) ?? string.Empty;

            violations.Add(new(entryId, fieldId, actualLength, maximumLength));
        }

        return violations;
    }

    private static bool TryGetMaximumSize(string rawConstraint, out int maximumLength)
    {
        maximumLength = 0;
        if (string.IsNullOrWhiteSpace(rawConstraint))
        {
            return false;
        }

        if (int.TryParse(rawConstraint, out maximumLength) && maximumLength > 0)
        {
            return true;
        }

        if (TryGetMaximumSizeFromJson(rawConstraint, out maximumLength))
        {
            return true;
        }

        var labeledMatch = Regex.Match(rawConstraint, @"(?i)(?:maximumsize|max)\D*(\d+)");
        if (labeledMatch.Success &&
            int.TryParse(labeledMatch.Groups[1].Value, out maximumLength) &&
            maximumLength > 0)
        {
            return true;
        }

        var rangeMatch = Regex.Match(rawConstraint, @"^\s*\d+\s*[-:;,]\s*(\d+)\s*$");
        if (rangeMatch.Success &&
            int.TryParse(rangeMatch.Groups[1].Value, out maximumLength) &&
            maximumLength > 0)
        {
            return true;
        }

        return false;
    }

    private static bool TryGetMaximumSizeFromJson(string rawConstraint, out int maximumLength)
    {
        maximumLength = 0;

        if (!rawConstraint.TrimStart().StartsWith('{') && !rawConstraint.TrimStart().StartsWith('['))
        {
            return false;
        }

        try
        {
            var token = JToken.Parse(rawConstraint);
            var maximumToken = FindFirstPropertyValue(token, "MaximumSize", "Max");

            return maximumToken != null &&
                   int.TryParse(maximumToken.ToString(), out maximumLength) &&
                   maximumLength > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JToken? FindFirstPropertyValue(JToken token, params string[] propertyNames)
    {
        if (token is JProperty property &&
            propertyNames.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return property.Value;
        }

        foreach (var child in token.Children())
        {
            var result = FindFirstPropertyValue(child, propertyNames);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static string BuildExceptionMessage(IEnumerable<CustomSizeConstraintViolation> violations, string locale)
    {
        var messages = violations.Select(violation => $"- {BuildViolationMessage(violation, locale)}");

        return string.Join(Environment.NewLine,
        [
            "The translated content violates custom size restrictions from the exported HTML.",
            "Fix the values below or set 'Skip custom validation step' to true if you want to continue and receive warnings instead.",
            ..messages
        ]);
    }

    private static string BuildViolationMessage(CustomSizeConstraintViolation violation, string locale)
        => $"Field '{violation.FieldId}' of entry '{violation.EntryId}' violates specified constraint for locale '{locale}'. Actual length: {violation.ActualLength}. Maximum allowed length: {violation.MaximumLength}.";

    private sealed record CustomSizeConstraintViolation(
        string EntryId,
        string FieldId,
        int ActualLength,
        int MaximumLength);
}
