using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace Markdown.Core
{
    public class MarkdownHtmlWriter : IDisposable
    {
        private MarkdownHtmlWriterSettings _settings;
        private XmlWriter _writer;
        private StringBuilder _builder;
        private bool _headWritten = false;

		public MarkdownHtmlWriter(MarkdownHtmlWriterSettings a_settings = null)
        {
            _settings = a_settings ?? new MarkdownHtmlWriterSettings();
            _builder = new StringBuilder();
            _writer = XmlWriter.Create(_builder, GetXmlWriterSettings());
        }

		private XmlWriterSettings GetXmlWriterSettings()
        {
            return new XmlWriterSettings()
            {
                Indent = true,
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				ConformanceLevel = ConformanceLevel.Fragment
            };
        }

		public void AppendMarkdown(string a_markDown)
        {
            if (!_headWritten)
            {
                WriteHead();
                _headWritten = true;
            }
            var markdownToHtml = new MarkdownToHtml(_settings);
            var html = markdownToHtml.Convert(a_markDown);
            _writer.WriteRaw(html);
        }

		public string GetHtml()
        {
            Dispose();
            return _builder.ToString();
        }

		private void WriteHead()
        {
            _writer.WriteRaw("<!DOCTYPE html>" + Environment.NewLine);
            _writer.WriteStartElement("html");
            _writer.WriteAttributeString("lang", _settings.Language);
            _writer.WriteStartElement("head");

            _writer.WriteStartElement("meta");
            _writer.WriteAttributeString("charset", "utf-8");
            _writer.WriteEndElement();

            if(!string.IsNullOrWhiteSpace(_settings.Title))
                _writer.WriteElementString("title", _settings.Title);

            _writer.WriteStartElement("style");
            _writer.WriteRaw(GenerateCSS());
            _writer.WriteEndElement();

            _writer.WriteEndElement();
            _writer.WriteStartElement("body");
        }

		private string GenerateCSS()
        {
            var builder = new StringBuilder();

            var css = GenerateDefaultCSS();
            if (!string.IsNullOrWhiteSpace(css))
                builder.AppendLine(css);

            if (_settings != null && _settings.SyntaxHighlighter.Any())
            {
                foreach (var syntaxhighlighter in _settings.SyntaxHighlighter)
                {
                    builder.AppendLine(syntaxhighlighter.GetCSS());
                }
            }
            return builder.ToString();
        }

		private string GenerateDefaultCSS()
        {
            var builder = new StringBuilder();
            builder.AppendLine("code { display:inline-block; border: 1px solid #adb7bd; background-color: #1E1E1E; padding:10px;}");
            builder.AppendLine("body { color: #adb7bd; font-family: 'Lucida Sans', Arial, sans-serif; font-size: 16px; background-color: #1C2329; }");
            builder.AppendLine("h1 { color: #ffffff; font-family: 'Lato', sans-serif; font-size: 32px; font-weight: 300; line-height: 58px; margin: 0 0 16px; border-bottom: solid 1px #adb7bd }");
            builder.AppendLine("h2 { color: #ffffff; font-family: 'Lato', sans-serif; font-size:28px; font-weight: 300; line-height: 58px; margin: 0 0 14px; }");
            builder.AppendLine("h3 { color: #ffffff; font-family: 'Lato', sans-serif; font-size:24px; font-weight: 300; line-height: 58px; margin: 0 0 12px; }");
            builder.AppendLine("p { color: #adb7bd; font-family: 'Lucida Sans', Arial, sans-serif; font-size: 16px; line-height: 26px; margin-top: 0; margin-bottom: 0; margin-right: 0; margin-left: 10px }");
            builder.AppendLine("a { color: #fe921f; text-decoration: underline; }");
            builder.AppendLine("a:hover { color: #ffffff }");

            return builder.ToString();
        }

        public void Dispose()
        {
            if(_writer != null)
            {
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.Dispose();
                _writer = null;
            }
        }
    }
}
