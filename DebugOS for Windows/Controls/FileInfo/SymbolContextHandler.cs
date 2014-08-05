using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace DebugOS
{
    public sealed class SymbolContextHandler : ISmartContextHandler
    {
        private static readonly Regex symbolRegex = new Regex(@"<(\w[ \w:\(\)\*,&]*)(?:\s*[\+-]\s*(0x)?[\da-fA-F]+)?>");

        public IEnumerable<UIElement> GetContextualUI(string content, UIElement source)
        {
            if (content == null || Application.Debugger == null ||
                Application.Debugger.CurrentObjectFile == null)
            {
                yield break;
            }

            // Use a regular expression to search for a symbol string within the content
            MatchCollection matches = symbolRegex.Matches(content);

            foreach (Match match in matches)
            {
                SymbolEntry sym = Application.Debugger.CurrentObjectFile.SymbolTable.GetSymbol(
                    match.Groups[1].Value);

                if (sym == null) continue;
                else yield return new SymbolContextItem(sym);
            }
        }
    }
}
