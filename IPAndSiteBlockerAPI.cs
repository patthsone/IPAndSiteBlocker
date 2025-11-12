using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace IPAndSiteBlockerAPI
{
    public static class BlockChecker
    {
        private static readonly Regex UrlRegex = new(@"\b(?:https?|ftp)://[^\s/$.?#].[^\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex IpRegex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
        private static readonly Regex DomainRegex = new(@"\b(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly List<string> CommonDomains = new List<string>
        {
            ".com", ".net", ".org", ".info", ".biz", ".us", ".ru",
            ".online", ".su", ".co", ".io", ".me", ".tv", ".edu",
            ".xyz", ".site", ".tech", ".dev", ".app", ".cloud"
        };

        public static List<string> Whitelist { get; set; } = new List<string>();

        public static bool IsBlocked(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            var urlMatches = UrlRegex.Matches(message);
            foreach (Match match in urlMatches)
            {
                if (!IsWhitelisted(match.Value))
                {
                    return true;
                }
            }

            var ipMatches = IpRegex.Matches(message);
            foreach (Match match in ipMatches)
            {
                if (!IsWhitelisted(match.Value))
                {
                    return true;
                }
            }

            var domainMatches = DomainRegex.Matches(message);
            foreach (Match match in domainMatches)
            {
                if (!IsWhitelisted(match.Value))
                {
                    return true;
                }
            }

            foreach (var domain in CommonDomains)
            {
                if (message.IndexOf(domain, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var nakedDomainMatches = Regex.Matches(message, $@"\b\w+{Regex.Escape(domain)}\b", RegexOptions.IgnoreCase);
                    foreach (Match match in nakedDomainMatches)
                    {
                        if (!IsWhitelisted(match.Value))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Player";

            name = UrlRegex.Replace(name, "");

            name = IpRegex.Replace(name, "");

            name = DomainRegex.Replace(name, "");

            foreach (var domain in CommonDomains)
                name = Regex.Replace(name, $@"\b\w+{Regex.Escape(domain)}\b", "", RegexOptions.IgnoreCase);

            name = name.Trim();

            if (string.IsNullOrEmpty(name) || name.Length < 2)
                name = "Player" + Random.Shared.Next(1000, 9999);

            return name;
        }

        public static bool IsWhitelisted(string message)
        {
            if (string.IsNullOrEmpty(message) || Whitelist.Count == 0)
                return false;

            string lowerCaseMessage = message.ToLowerInvariant();

            return Whitelist.Any(whitelistedItem =>
                lowerCaseMessage.Equals(whitelistedItem.ToLowerInvariant()) ||
                lowerCaseMessage.Contains(whitelistedItem.ToLowerInvariant()));
        }
    }
}