using System.Text;

namespace Apps.Contentful.Utils
{
    public static class FileNameSanitizer
    {
        public static string Sanitize(string? name, string fallback = "file", int maxLength = 120)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = fallback;

            var invalid = Path.GetInvalidFileNameChars();

            var sb = new StringBuilder(name.Length);
            foreach (var ch in name)
            {
                if (char.IsControl(ch) || Array.IndexOf(invalid, ch) >= 0)
                    sb.Append('_');
                else
                    sb.Append(ch);
            }
            var result = sb.ToString().Trim();

            result = result.TrimEnd('.', ' ');

            if (string.IsNullOrWhiteSpace(result))
                result = fallback;

            if (result.Length > maxLength)
                result = result.Substring(0, maxLength).TrimEnd('.', ' ');

            return result;
        }
    }
}
