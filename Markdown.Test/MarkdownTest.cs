using System;
using System.IO;
using System.Text;
using System.Linq;
using Markdown.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Markdown.Test
{
    [TestClass]
    [DeploymentItem(@"Testdaten\test01.markdown", "Testdaten")]
    [DeploymentItem(@"Testdaten\example.png", "Testdaten")]
    public class MarkdownTest
    {
        [TestMethod]
        [TestCategory("Markdown\\Converter")]
        public void ConvertTest()
        {

            var markdown = File.ReadAllText(@"Testdaten\test01.markdown");
            var settings = new MarkdownHtmlWriterSettings()
            {
                Title = "test"
            };
            using (var writer = new MarkdownHtmlWriter(settings))
            {
                writer.AppendMarkdown(markdown);
                var html = writer.GetHtml();
                File.WriteAllText(@"D:\temp\test01.html", html, Encoding.UTF8);
            }
        }
    }
}
