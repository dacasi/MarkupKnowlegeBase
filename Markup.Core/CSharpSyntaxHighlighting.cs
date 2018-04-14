using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Markdown.Core
{
    public class CSharpSyntaxHighlighting : ISyntaxHighlighting
    {
        private static readonly string[] _keyWords = new string[]
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "in (generic modifier)",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
            "var",
            "partial",
            "async",
            "await",
            "dynamic",
            "gobla",
            "let",
            "yield"
        };

        private bool _isString = false;
        private bool _isChar = false;
        private bool _isLineComment = false;
        private bool _isMultiLineComment = false;
        private ParserStep _parserStep;

        public string[] CodeIDs => new string[] { "csharp", "cs", "c#" };

        public string Highlight(string a_code)
        {
            _parserStep = new ParserStep();

            for(int i = 0; i < a_code.Length; i++)
            {
                _parserStep.C = a_code[i];
                _parserStep.NextChar = (i + 1 < a_code.Length) ? a_code[i + 1] : ' ';

                if (_isLineComment) { ParseLineComment(); }
                else if (_isMultiLineComment)
                {
                    if (ParseMultilineComment())
                        i++; //add the end of the comment, jump one char.
                }
                else
                {
                    ParseCode();
                }
                _parserStep.LastChar = _parserStep.C;
            }
            FlushBuffer();

            return _parserStep.Builder.ToString();
        }

        private void ParseCode()
        {
            var builder = _parserStep.Builder;
            var buffer = _parserStep.Buffer;
            var c = _parserStep.C;
            var lastChar = _parserStep.LastChar;

            switch (c)
            {
                case '/':
                    if (_parserStep.NextChar == '/')
                    {
                        FlushBuffer();
                        buffer.Append("<span class=\"csComment\">/");
                        _isLineComment = true;
                    }
                    else if (_parserStep.NextChar == '*')
                    {
                        FlushBuffer();
                        buffer.Append("<span class=\"csComment\">/");
                        _isMultiLineComment = true;
                    }
                    else
                        buffer.Append(c);
                    break;

                case ';': FlushBufferAndAppend(c); break;
                case ',': FlushBufferAndAppend(c); break;
                case ')': FlushBufferAndAppend(c); break;
                case '}': FlushBufferAndAppend(c); break;
                case '(': FlushBufferAndAppend(c); break;
                case '{': FlushBufferAndAppend(c); break;
                case '[': FlushBufferAndAppend(c); break;
                case ']': FlushBufferAndAppend(c); break;
                case '<': FlushBufferAndAppend("&lt;"); break;
                case '>': FlushBufferAndAppend("&gt;"); break;
                case '\n': FlushBufferAndAppend("<br />"); break;
                case ' ': FlushBufferAndAppend("&nbsp;"); break;
                case '\t': FlushBufferAndAppend("&nbsp;&nbsp;&nbsp;&nbsp;"); break;

                case '"':
                    if (lastChar != '\\')
                    {
                        if (!_isString)
                        {
                            buffer.Append("<span class=\"csString\">&quot;");
                            _isString = true;
                        }
                        else
                        {
                            buffer.Append("&quot;</span>");
                            _isString = false;
                        }
                    }
                    break;

                case '\'':
                    if (lastChar != '\\')
                    {
                        if (!_isChar)
                        {
                            buffer.Append("<span class=\"csString\">&#39;");
                            _isChar = true;
                        }
                        else
                        {
                            buffer.Append("&#39;</span>");
                            _isChar = false;
                        }
                    }
                    break;

                default:
                    buffer.Append(c);
                    break;
            }
        }

        private void ParseLineComment()
        {
            switch (_parserStep.C)
            {
                case '\n':
                    _parserStep.Buffer.Append("</span><br />");
                    _isLineComment = false;
                    break;

                default:
                    _parserStep.Buffer.Append(_parserStep.C);
                    break;
            }
        }

        private bool ParseMultilineComment()
        {
            var builder = _parserStep.Builder;
            var buffer = _parserStep.Buffer;
            var c = _parserStep.C;

            switch (c)
            {
                case '*':
                    if (_parserStep.NextChar == '/')
                    {
                        buffer.Append("*/</span>");
                        _isMultiLineComment = false;
                        return true;
                    }
                    else  {  buffer.Append(c);  }
                    break;

                case '\n': FlushBufferAndAppend("<br />"); break;
                case ' ': FlushBufferAndAppend("&nbsp;"); break;
                case '\t': FlushBufferAndAppend("&nbsp;&nbsp;&nbsp;&nbsp;"); break;

                default:
                    buffer.Append(c);
                    break;
            }
            return false;
        }

        private void FlushBufferAndAppend(char a_append)
        {
            FlushBuffer();
            _parserStep.Builder.Append(a_append);
        }

        private void FlushBufferAndAppend(string a_append)
        {
            FlushBuffer();
            _parserStep.Builder.Append(a_append);
        }

        private void FlushBuffer()
        {
            if (_parserStep.Buffer.Length > 0)
            {
                var text = _parserStep.Buffer.ToString().TrimEnd();
                if (!_isString && !_isChar && _keyWords.Any(w => w.Equals(text, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _parserStep.Builder.Append($"<span class=\"csKeyword\">{text}</span>");
                }
                else
                {
                    _parserStep.Builder.Append(text);
                }
                _parserStep.Buffer.Clear();
            }
        }

        public string GetCSS()
        {
            return "span.csString { color: DarkSalmon ; } " + Environment.NewLine +
                    "span.csKeyword { color: CornflowerBlue; } " + Environment.NewLine +
                    "span.csComment { color: ForestGreen ; }";
        }

    }
}
