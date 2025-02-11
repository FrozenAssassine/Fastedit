using System.Collections.Generic;
using System.Linq;

namespace Fastedit.Storage
{
    public class FileExtensions
    {
        public static ExtensionItem FindByExtension(string extension)
        {
            var res = FileExtentionList.Where(x => x.HasExtension(extension));
            if (res.Count() > 0)
                return res.ElementAt(0);
            return null;
        }

        public static List<ExtensionItem> FileExtentionList = new List<ExtensionItem>
        {
            new ExtensionItem()
            {
                Extension = { ".md", ".markdown", ".mdown", ".markdn" },
                ExtensionName = "Markdown",
                ExtensionLongName = "Markdown"
            },
            new ExtensionItem()
            {
                Extension = { ".json" },
                ExtensionName = "Json",
            },
            new ExtensionItem()
            {
                Extension = { ".gcode", ".ngc", ".tap" },
                ExtensionName = "G-Code",
                ExtensionLongName = "G-Code"
            },
            new ExtensionItem()
            {
                Extension = { ".vb" },
                ExtensionName = "Visual Basic",
            },
            new ExtensionItem()
            {
                Extension = { ".ino" },
                ExtensionName = "Arduino sketch",
            },
            new ExtensionItem()
            {
                Extension = { ".php" },
                ExtensionName = "Hypertext preprocessor",
            },
            new ExtensionItem()
            {
                Extension = { ".asm" },
                ExtensionName = "Assembly language",
            },
            new ExtensionItem()
            {
                Extension = { ".kt" },
                ExtensionName = "Kotlin",
            },
            new ExtensionItem()
            {
                Extension = { ".cs" },
                ExtensionName = "CSharp"
            },
            new ExtensionItem()
            {
                Extension = { ".cpp", ".cxx", ".cc", ".hpp" },
                ExtensionName = "C++"
            },
            new ExtensionItem()
            {
                Extension = { ".py", ".py3", ".pyt", ".rpy", ".pyw" },
                ExtensionName = "Python",
                ExtensionLongName = "Python"
            },
            new ExtensionItem()
            {
                Extension = { ".bat" },
                ExtensionName = "Batch",
                ExtensionLongName = "Windows batch"
            },
            new ExtensionItem()
            {
                Extension = { ".xaml" },
                ExtensionName = "Xaml",
                ExtensionLongName = "Extensible Application Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".xml" },
                ExtensionName = "XML",
                ExtensionLongName = "Extensible Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".html", ".htm", ".xhtml" },
                ExtensionName = "HTML",
                ExtensionLongName = "Hypertext Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".txt", ".log" },
                ExtensionName = "Textfile",
                ExtensionLongName = "Textfile"
            },
            new ExtensionItem()
            {
                Extension = { ".reg" },
                ExtensionName = "Registration file",
                ExtensionLongName = "Windows Registration file"
            },
            new ExtensionItem()
            {
                Extension = { ".css" },
                ExtensionName = "Cascading Style Sheets",
                ExtensionLongName = "Cascading Style Sheets"
            },
            new ExtensionItem()
            {
                Extension = { ".java", ".jav", ".j" },
                ExtensionName = "Java",
                ExtensionLongName = "Java"
            },
            new ExtensionItem()
            {
                Extension = { ".ini", ".config", ".inf" },
                ExtensionName = "Configuration file",
                ExtensionLongName = "Configuration file"
            },
            new ExtensionItem()
            {
                Extension = { ".c", ".h" },
                ExtensionName = "C language",
            },
            new ExtensionItem()
            {
                Extension = { ".js", },
                ExtensionName = "JavaScript",
            },
            new ExtensionItem()
            {
                Extension = { ".csv", },
                ExtensionName = "Comma-separated values",
            },
            new ExtensionItem()
            {
                Extension = { ".tex", },
                ExtensionName = "LaTeX",
            },
            new ExtensionItem()
            {
                Extension = { ".toml", },
                ExtensionName = "TOML configuration file",
            },
            new ExtensionItem()
            {
                Extension = { ".sql", },
                ExtensionName = "Structured Query Language",
            }
        };
    }
    public class ExtensionItem
    {
        public bool HasExtension(string extension)
        {
            return Extension.Contains(extension);
        }

        public List<string> Extension = new List<string>();
        public string ExtensionName { get; set; }
        public string ExtensionLongName { get; set; }
    }
}