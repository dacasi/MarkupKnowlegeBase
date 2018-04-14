using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Core
{
    public interface ISyntaxHighlighting
    {
        string[] CodeIDs { get; }
        string Highlight(string a_code);
        string GetCSS();
    }
}
