using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugOS
{
    public delegate void ResultClickCallback(SearchResult sender);


    public class SearchResult
    {
        public string Title { get; private set; }
        public Bitmap Icon { get; private set; }

        public ResultClickCallback ClickCallback { get; private set; }

        public SearchResult(string title, Bitmap icon, ResultClickCallback onClick = null)
        {
            this.Title         = title;
            this.Icon          = icon;
            this.ClickCallback = onClick;
        }
    }

    public interface ISearchCategory
    {
        string Header { get; }

        IEnumerable<SearchResult> Reset();
        IEnumerable<SearchResult> GetResults(string searchString);
    }
}
