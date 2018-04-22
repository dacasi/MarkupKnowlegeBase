using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markdown.Core
{
    public class MarkdownToHtml
    {
        private Stack<string> _elements;
        private StringBuilder _builder;
        private StringBuilder _buffer;
        private bool _listActive;
        private MarkdownSettings _settings;
        private Dictionary<Guid, string> _replace;

        public MarkdownToHtml(MarkdownSettings a_settings = null)
        {
            _settings = a_settings ?? new MarkdownSettings();
        }

        private void Init()
        {
            _elements = new Stack<string>();
            _builder = new StringBuilder();
            _buffer = new StringBuilder();
            _listActive = false;
            _replace = new Dictionary<Guid, string>();
        }

        //https://en.wikipedia.org/wiki/Markdown
        public string Convert(string a_markdown)
        {
            if (string.IsNullOrWhiteSpace(a_markdown)) return string.Empty;

            Init();
            var text = PrepareText(a_markdown);
            var lines = text.Split(new char[] { '\n' }).
                Select(l => l.TrimStart().TrimEnd(new char[] { '\r' })).
                ToList();
            for(int i = 0; i < lines.Count; i++)
            {
                ConvertLine(lines[i]);
            }
            if (_buffer.Length > 0)
                _builder.Append(_buffer.ToString());

            var result = _builder.ToString();
            foreach(var id in _replace.Keys)
            {
                result = result.Replace(id.ToString(), _replace[id]);
            }

            return result;
        }

        private void ConvertLine(string a_line)
        {
            var trimLine = a_line.Trim();

            a_line = PrepareLine(a_line);

            if (trimLine.Any() && trimLine.All(c => c == '='))
            {
                AppendElement("h1", _buffer.ToString().Trim());
                _buffer.Clear();
            }
            else if (a_line.StartsWith("#"))
            {
                var tag = string.Empty;
                if (a_line.StartsWith("###")) { tag = "h3"; }
                else if (a_line.StartsWith("##")) { tag = "h2"; }
                else if(a_line.StartsWith("#")) { tag = "h1"; }

                if (!string.IsNullOrWhiteSpace(tag)) 
                    AppendElement(tag, a_line.TrimStart(new char[] { '#', ' ' }));

            }
            else if (string.IsNullOrWhiteSpace(a_line))
            {
                if (_listActive)
                {
                    EndElement();
                    _listActive = false;
                }
                else
                {
                    //Paragraph
                    var content = _buffer.ToString().Trim();
                    if (content.Length > 0)
                    {
                        AppendElement("p", content);
                    }
                    _buffer.Clear();
                }
            }
            else if (trimLine.All(c => c == '-'))
            {
                AppendElement("hr");
            }
            else if (trimLine.StartsWith("*"))
            {
                if (!_listActive)
                {
                    StartElement("ul");
                    _listActive = true;
                }
                _builder.AppendLine($"<li>{a_line.TrimStart(new char[] { '*', ' ' })}</li>");
            }
            else if (Regex.IsMatch(trimLine, @"[0-9]+?\.+?"))
            {
                if (!_listActive)
                {
                    StartElement("ol");
                    _listActive = true;
                }
                _builder.AppendLine($"<li>{a_line.Substring(a_line.IndexOf('.') + 1)}</li>");
            }
            else
            {
                //NormalText
                _buffer.AppendLine(a_line);
            }
        }

        private string PrepareText(string a_text)
        {
            a_text = HightlightCodeParts(a_text);
            a_text = Replace(a_text, @"\*\*.+?\*\*", '*', "b");
            a_text = Replace(a_text, @"_.+?_", '_', "em");
            a_text = Replace(a_text, @"`.+?`", '`', "code");

            return a_text;
        }

        private string HightlightCodeParts(string a_text)
        {
            if (_settings == null || _settings.SyntaxHighlighter == null || !_settings.SyntaxHighlighter.Any()) return a_text;

            foreach (Match match in Regex.Matches(a_text, @"```([\w\d]+)(.+)```", RegexOptions.Singleline))
            {
                var codeID = match.Groups[1].Value;
                var hightlighter = _settings.SyntaxHighlighter.FirstOrDefault(h => h.CodeIDs.Contains(codeID, StringComparer.InvariantCultureIgnoreCase));
                if (hightlighter != null)
                {
                    var hightlightedCode = hightlighter.Highlight(match.Groups[2].Value.TrimStart());
                    var id = Guid.NewGuid();
                    _replace.Add(id, $"<code>{hightlightedCode}</code>");
                    a_text = a_text.Replace(match.Value, $"{id.ToString()}");
                }
            }
            return a_text;
        }

        private string PrepareLine(string a_line)
        {
            //Images
            foreach (Match match in Regex.Matches(a_line, @"!\[(.+?)\]\((.+?)\)", RegexOptions.IgnoreCase))
            {
                a_line = a_line.Replace(match.Value, $"<img alt=\"{match.Groups[1].Value}\" src=\"{match.Groups[2].Value}\"/>");
            }

            //Links
            foreach (Match match in Regex.Matches(a_line, @"\[(.+?)\]\((.+?)\)", RegexOptions.IgnoreCase))
            {
                a_line = a_line.Replace(match.Value, $"<a href=\"{match.Groups[2].Value}\">{match.Groups[1].Value}</a>");
            }

            if(a_line.EndsWith("  "))
            {
                a_line = a_line.TrimEnd() + "<br />";
            }

            return a_line;
        }

        private string Replace(string a_text, string a_regex, char a_trim, string a_tag)
        {
            foreach (Match match in Regex.Matches(a_text, a_regex, RegexOptions.Singleline))
            {
                a_text = a_text.Replace(match.Value, $" <{a_tag}>{match.Value.Trim(new char[] { a_trim })}</{a_tag}>");
            }
            return a_text;
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
            _builder.Append($"<{a_tag} />");
        }

        private void StartElement(string a_tag)
        {
            _builder.Append($"<{a_tag}>");
            _elements.Push(a_tag);
        }

        private void EndElement()
        {
            var tag = _elements.Pop();
            _builder.Append(string.Format("</{0}>", tag));
        }
    }
}
