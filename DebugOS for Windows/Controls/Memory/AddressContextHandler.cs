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

            var addresses = new List<Address>();

            // Use a regular expression to search for a hex string within the content
            MatchCollection matches = hexRegex.Matches(content);

            foreach (Match match in matches)
            {
                var address = new Address((long)Utils.ParseHex64(match.Groups[1].Value));
                addresses.Add(address);

                yield return new AddressContextItem(address);
            }

            // If no matches, brute-force
            if (matches.Count == 0)
            {
                AddressContextItem output = null;
                try
                {
                    var address = new Address((long)Utils.ParseHex64(content));
                    addresses.Add(address);

                    output = new AddressContextItem(address);
                }
                catch { }

                if (output != null) yield return output;
            }

            // Look for matching symbols
            if (addresses.Count != 0 && Application.Debugger != null &&
                Application.Debugger.CurrentObjectFile != null)
            {
                foreach (var sym in Application.Debugger.CurrentObjectFile.SymbolTable)
                {
                    foreach (Address address in addresses)
                    {
                        if (sym.Value == address.Value && sym.Value != 0)
                        {
                            yield return new SymbolContextItem(sym);
                        }
                    }
                }
            }
        }
    }
}
