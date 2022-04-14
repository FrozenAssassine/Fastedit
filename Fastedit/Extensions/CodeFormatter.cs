using Fastedit.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fastedit.Extensions
{
    public class CodeFormatter
    {
        private static string Indent = DefaultValues.DefaultTabSize;

        public static XmlException FormatXml(string text, out string output)
        {
            output = text;
            if (string.IsNullOrEmpty(text))
                return null;

                XmlDocument xmlDoc = new XmlDocument();
                StringWriter sw = new StringWriter();
            try
            {
                xmlDoc.LoadXml(text);
                xmlDoc.Save(sw);
                output = sw.ToString();
                return null;
            }
            catch (XmlException e)
            {
                return e;
            }
        }
        
        public static string FormatJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in text ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }
        
        public static string FormatHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            try
            {
                return System.Xml.Linq.XElement.Parse(text).ToString();
            }
            catch
            {
                return text;
                // Your input is not a valid xml fragment.
            }
        }

        public static string FormatCs(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace(Indent).ToFullString();
            
        }
    }
}
