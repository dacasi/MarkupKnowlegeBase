using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Markup.Core
{
    public class MarkupToHtml
    {
        private Stack<string> _elements;
        private StringBuilder _builder;
        private StringBuilder _buffer;

        public MarkupToHtml()
        {
        }

        private void Init()
        {
            _elements = new Stack<string>();
            _builder = new StringBuilder();
            _buffer = new StringBuilder();
        }


        //https://en.wikipedia.org/wiki/Markdown
        public string Convert(string a_markup)
        {
            if (string.IsNullOrWhiteSpace(a_markup)) return string.Empty;

            Init();
            var lines = a_markup.Split(new char[] { '\n' }).
                Select(l => l.TrimStart()).
                ToList();
            for(int i = 0; i < lines.Count; i++)
            {
                ConvertLine(lines[i]);
            }
            if (_buffer.Length > 0)
                _builder.Append(_buffer.ToString());

            return _builder.ToString();
        }

        private void ConvertLine(string a_line)
        {
            var trimLine = a_line.Trim();
            if(trimLine.All(c => c == '#')){
                AppendElement("h1", _buffer.ToString());
                _buffer.Clear();
            }
            else if (a_line.StartsWith("##"))
            {
                AppendElement("h2", a_line.TrimStart(new char[] { '#', ' ' }));
            }
            else if (string.IsNullOrWhiteSpace(a_line))
            {
                //Paragraph
                AppendElement("p", _buffer.ToString());
                _buffer.Clear();
            }
            else if(trimLine.All(c => c == '-')){
                AppendElement("hr");
            }
            else
            {
                //NormalText
                _buffer.AppendLine(a_line);
                if (a_line.EndsWith("  "))
                    _buffer.Append("<br/>");
            }
        }

        private void AppendElement(string a_tag, string a_content)
        {
            StartElement(a_tag);
            _builder.Append(a_content);
            EndElement();
            _builder.AppendLine();
        }
        private void AppendElement(string a_tag)
        {
            _builder.Append($"</{a_tag}>");
        }

        private void StartElement(string a_tag)
        {
            _builder.Append($"<{a_tag}>");
            _elements.Push(a_tag);
        }

        private void EndElement()
        {
            var tag = _elements.Pop();
            _builder.Append("$</{tag}>");
        }
    }
}
