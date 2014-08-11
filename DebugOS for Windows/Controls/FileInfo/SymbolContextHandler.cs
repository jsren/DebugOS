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
            if (content == null || Application.Debugger == null)
            {
                yield break;
            }

            // Use a regular expression to search for a symbol string within the content
            MatchCollection matches = symbolRegex.Matches(content);

            foreach (Match match in matches)
            {
                SymbolEntry sym  = null;
                CodeUnit    unit = null;

                foreach (ObjectCodeFile file in Application.Session.LoadedImages)
                {
                    if ((sym = file.SymbolTable.GetSymbol(match.Groups[1].Value)) != null)
                    {
                        unit = file.GetCode(sym.Name);
                        break;
                    }
                }
                if (sym != null) yield return new SymbolContextItem(unit, sym);
            }
        }
    }
}
