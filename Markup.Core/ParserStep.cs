using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Core
{
    public class ParserStep
    {
        public StringBuilder Buffer { get; set; }
        public StringBuilder Builder { get; set; }
        public char C { get; set; }
        public char NextChar { get; set; }
        public char LastChar { get; set; }

        public ParserStep()
        {
            Buffer = new StringBuilder();
            Builder = new StringBuilder();
            NextChar = ' ';
            LastChar = ' ';
            C = ' ';
        }
    }
}
