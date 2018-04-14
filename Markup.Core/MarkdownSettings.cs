using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Markdown.Core
{
    public class MarkdownSettings
    {
        public List<ISyntaxHighlighting> SyntaxHighlighter { get; set; } 

        public MarkdownSettings()
        {
            this.SyntaxHighlighter = new ISyntaxHighlighting[]
            {
                new CSharpSyntaxHighlighting()
            }.ToList();
        }
    }
}
