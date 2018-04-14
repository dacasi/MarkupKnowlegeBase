using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Core
{
    public class MarkdownHtmlWriterSettings : MarkdownSettings
    {
        public string Title { get; set; }
        public string Language { get; set; }

        public MarkdownHtmlWriterSettings() : base()
        {
            this.Language = "en";
        }
    }
}
