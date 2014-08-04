using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace DebugOS
{
    public sealed class AddressContextHandler : ISmartContextHandler
    {
        private static readonly Regex hexRegex = new Regex("(?:\\W|^)0x([0-9a-fA-F]+)(?:\\W|$)");

        public IEnumerable<UIElement> GetContextualUI(string content, UIElement source)
        {
            if (content == null) yield break;

            // Use a regular expression to search for a hex string within the content
            MatchCollection matches = hexRegex.Matches(content);

            foreach (Match match in matches)
            {
                yield return new AddressContextItem(
                    new Address((long)Utils.ParseHex64(match.Groups[1].Value)));
            }

            // If no matches, brute-force
            if (matches.Count == 0) 
            {
                UIElement output = null;
                try
                {
                    output = new AddressContextItem(
                        new Address((long)Utils.ParseHex64(content)));
                }
                catch { }

                if (output != null) yield return output;
                else                yield break;
            }
        }
    }
}
